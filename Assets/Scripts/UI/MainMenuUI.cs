using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public string levelSceneName = "Level1";

    public CanvasGroup mainMenuPanel;
    public CanvasGroup settingsPanel;

    public float fadeTime = 0.2f;

    private bool busy;

    void Start()
    {
        Show(mainMenuPanel);
        Hide(settingsPanel);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }

    public void StartGame()
    {
				Debug.Log("starting");
        SceneManager.LoadScene(levelSceneName);
    }

    public void OpenSettings()
    {
        if (!busy) {
            StartCoroutine(Switch(mainMenuPanel, settingsPanel));
				}
    }

    public void CloseSettings()
    {
        if (!busy) {
            StartCoroutine(Switch(settingsPanel, mainMenuPanel));
				}
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    IEnumerator Switch(CanvasGroup from, CanvasGroup to)
    {
        busy = true;

        yield return FadeOut(from);
        yield return FadeIn(to);

        busy = false;
    }

    IEnumerator FadeIn(CanvasGroup group)
    {
        group.gameObject.SetActive(true);
        group.interactable = false;
        group.blocksRaycasts = false;

        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);
            yield return null;
        }

        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    IEnumerator FadeOut(CanvasGroup group)
    {
        group.interactable = false;
        group.blocksRaycasts = false;

        float start = group.alpha;
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, 0f, t / fadeTime);
            yield return null;
        }

        group.alpha = 0f;
        group.gameObject.SetActive(false);
    }

    void Show(CanvasGroup group)
    {
        group.gameObject.SetActive(true);
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    void Hide(CanvasGroup group)
    {
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        group.gameObject.SetActive(false);
    }
}
