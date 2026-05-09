using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExitTrigger : MonoBehaviour
{
    public event System.Action OnPlayerExited;

    [SerializeField] private string nextScene = "";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        OnPlayerExited?.Invoke(); // LevelManager listens to this

        if (!string.IsNullOrEmpty(nextScene))
            SceneManager.LoadScene(nextScene); // fallback for testing
    }
}