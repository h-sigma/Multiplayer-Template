using UnityEngine;

[DefaultExecutionOrder(-500)]
public class GameManager : Singleton<GameManager>
{
    public SceneTransition transitionOutOfPersistent;
    public GameObject loadingScreen = default;

    public void Start()
    {
        transitionOutOfPersistent.TryTransition(true);
    }
    
    public void TransitionBegan()
    {
        LockUI();
    }

    public void TransitionFinished()
    {
        UnlockUI();
    }

    private void LockUI()
    {
        loadingScreen.SetActive(true);
    }

    private void UnlockUI()
    {
        loadingScreen.SetActive(false);
    }
}