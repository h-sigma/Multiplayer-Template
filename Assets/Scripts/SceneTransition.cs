using UnityEngine;

[CreateAssetMenu(fileName = "Z__RenameSceneTransition", menuName = "Scriptables/Scene Transition")]
public class SceneTransition : ScriptableObject
{
    public SceneEnums from;
    public SceneEnums to;

    public void TryTransitionUI()
    {
        TryTransition(true);
    }

    public void ForceTransitionUI()
    {
        ForceTransition(true);
    }
    
    public bool TryTransition(bool setLoading)
    {
        var result = SceneTransitionManager.Instance.Transition(this, HintGameManagerTransitionOver);
        if (result && setLoading)
        {
            GameManager.Instance.TransitionBegan();
        }

        return result;
    }

    public bool ForceTransition(bool setLoading)
    {
        var result =  SceneTransitionManager.Instance.TransitionUnloadSuperfluous(this, HintGameManagerTransitionOver);
        
        if (result && setLoading)
        {
            GameManager.Instance.TransitionBegan();
        }

        return result;
    }

    public void DoUnload()
    {
        SceneTransitionManager.Instance.UnloadSceneAsync(from, HintGameManagerTransitionOver);
    }

    public void DoLoad()
    {
        SceneTransitionManager.Instance.LoadSceneAsync(to, HintGameManagerTransitionOver);
    }

    public static void HintGameManagerTransitionOver()
    {
        GameManager.Instance.TransitionFinished();
    }
}