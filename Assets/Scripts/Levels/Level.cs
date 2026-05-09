using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public abstract class Level : MonoBehaviour
{
    public abstract string SceneName { get; }
    public abstract float Weight { get; }

    public abstract bool IsEligible(int completionsThisRun);
    public abstract void OnCompleted();
    public abstract void ResetRunState();

    public virtual void OnLevelEnter() { }

    public virtual void OnLevelExit()
    {
        SceneManager.LoadScene(SceneName);
    }
}

