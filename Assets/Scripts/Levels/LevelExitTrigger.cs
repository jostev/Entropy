using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExitTrigger : MonoBehaviour
{
    public event System.Action OnPlayerExited;

    [SerializeField] private string nextScene = "";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

				SoundManager.Instance?.PlaySFX(SoundManager.Instance.zap, 0.6f, 0.1f);

        OnPlayerExited?.Invoke(); // LevelManager listens to this

        if (!string.IsNullOrEmpty(nextScene))
            SceneManager.LoadScene(nextScene); // fallback for testing
    }
}
