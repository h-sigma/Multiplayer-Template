using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : Singleton<SceneTransitionManager>
{
    /// <summary>
    /// Doesn't include persistent scene. Can only transition from top-most scene to wanted scene.
    /// </summary>
    [SerializeField]
    private Stack<SceneEnums> loadedScenes = new Stack<SceneEnums>();

    private bool                 _inTransition   = false;
    private List<AsyncOperation> _inProgress     = new List<AsyncOperation>();
    private List<Action>         _waitingActions = new List<Action>();

    public bool IsLoadPending => _inTransition;

    private void DoPreValidatedTransition(SceneTransition valid, Action onTransitionFinish)
    {
    }

    /// <summary>
    /// Tries to transition from the most recently loaded scene to the scene passed as parameter.
    /// The transition finish action can't start another transition.
    /// </summary>
    /// <returns>True if such transition is allowed and no other transition is in progress, false otherwise.</returns>
    public bool Transition(SceneTransition transition, Action onTransitionFinish)
    {
        if (IsLoadPending) return false;

        if (transition.from != SceneEnums.NULL && transition.from != loadedScenes.Peek()) return false;

        _inTransition = true;

        if (transition.from != SceneEnums.NULL)
        {
            UnloadSceneAsync(transition.from);
        }

        if (transition.to != SceneEnums.NULL)
        {
            LoadSceneAsync(transition.to);
        }

        _waitingActions.Add(onTransitionFinish);

        return true;
    }

    /// <summary>
    /// Unloads all scenes loaded after "from", then unloads "from". Fails if "from" is not loaded. Fails if transition from-to is not allowed.
    /// This allows some code to load as many "non-important" scenes as they want without needing to keep track. 
    /// </summary>
    public bool TransitionUnloadSuperfluous(SceneTransition transition, Action onTransitionFinish)
    {
        if (!loadedScenes.Contains(transition.from) || IsLoadPending || transition.from == SceneEnums.NULL)
            return false;

        //do the actual unloading of superfluous scenes
        var top = SceneEnums.NULL;
        while (loadedScenes.Peek() != transition.from)
        {
            top = loadedScenes.Pop();
            var op = SceneManager.UnloadSceneAsync((int) top);
            _inProgress.Add(op);
        }

        return Transition(transition, onTransitionFinish);
    }

    /// <summary>
    /// Tries to load a scene if it isn't already loaded.
    /// The action is run when all currently pending operations are finished.
    /// </summary>
    public AsyncOperation LoadSceneAsync(SceneEnums toLoad, Action onFinish = null)
    {
        var op = SceneManager.LoadSceneAsync((int) toLoad, LoadSceneMode.Additive);

        _inProgress.Add(op);

        if (onFinish != null)
            _waitingActions.Add(onFinish);

        loadedScenes.Push(toLoad);
        return op;
    }

    /// <summary>
    /// Tries to unload a scene.
    /// The action is run when all currently pending operations are finished.
    /// </summary>
    public AsyncOperation UnloadSceneAsync(SceneEnums toUnload, Action onFinish = null)
    {
        if (!loadedScenes.Contains(toUnload)) return null;

        var op = SceneManager.UnloadSceneAsync((int) toUnload);

        _inProgress.Add(op);

        if (onFinish != null)
            _waitingActions.Add(onFinish);

        //save the scenes we don't want to unload 
        Stack<SceneEnums> dontTouch = new Stack<SceneEnums>();
        while (loadedScenes.Peek() != toUnload)
        {
            dontTouch.Push(loadedScenes.Pop());
        }

        loadedScenes.Pop();
        //restore untouched scenes
        while (dontTouch.Count > 0)
        {
            loadedScenes.Push(dontTouch.Pop());
        }

        return op;
    }

    public void Update()
    {
        if (!IsLoadPending) return;

        CheckProgress();
    }

    private void CheckProgress()
    {
        for (int i = 0; i < _inProgress.Count; i++)
        {
            if (!_inProgress[i].isDone) return; //return if all operations haven't finished
        }

        _inProgress.Clear();

        for (int i = _waitingActions.Count - 1; i >= 0; i--)
        {
            _waitingActions[i]?.Invoke();
        }

        _waitingActions.Clear();
        _inTransition = false;
    }
}