using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Pool")]
    [SerializeField] private Level[] levels;

    [Header("Settings")]
    [SerializeField] private string fallbackScene = "MainMenu";
    [SerializeField] private SelectionMode selectionMode = SelectionMode.WeightedRandom;

    public int CompletionsThisRun { get; private set; }
    public Level CurrentLevel { get; private set; }

    private LevelSelector _selector;
    private readonly List<int> _history = new();

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _selector = new LevelSelector(levels, selectionMode);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // var exitTrigger = FindFirstObjectByType<LevelExitTrigger>();
				var exitTrigger = FindAnyObjectByType<LevelExitTrigger>();
        if (exitTrigger != null)
            exitTrigger.OnPlayerExited += CompleteCurrentLevel;
    }

    // -------------------------------------------------------------------------

    public void CompleteCurrentLevel()
    {
        CurrentLevel?.OnCompleted();
        CompletionsThisRun++;
        LoadNextLevel();
    }

    public void RestartRun()
    {
        CompletionsThisRun = 0;
        _history.Clear();
        _selector.Reset();
        foreach (var l in levels) l.ResetRunState();
        LoadNextLevel();
    }

    // -------------------------------------------------------------------------

    private void LoadNextLevel()
    {
        int next = _selector.Next(CompletionsThisRun);

        if (next < 0)
        {
            SceneManager.LoadScene(fallbackScene);
            return;
        }

        _history.Add(next);
        CurrentLevel = levels[next];

        SceneManager.LoadScene(CurrentLevel.SceneName);
    }
}
