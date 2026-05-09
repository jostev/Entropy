using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Level Pool")]
    [SerializeField] private Level[] levels; // K levels assigned in Inspector

    [Header("Settings")]
    [SerializeField] private string fallbackScene = "MainMenu";
    [SerializeField] private SelectionMode selectionMode = SelectionMode.WeightedRandom;

    public int CompletionsThisRun { get; private set; }
    public Level CurrentLevel { get; private set; }

    private int _currentIndex = -1;
    private readonly List<int> _history = new();

    public enum SelectionMode { Sequential, PureRandom, WeightedRandom, WeightedRandomNoRepeat }

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Hooks into the current level's exit trigger each time a scene loads.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var exitTrigger = FindFirstObjectByType<LevelExitTrigger>();
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
        _currentIndex = -1;
        _history.Clear();
        foreach (var l in levels) l.ResetRunState();
        LoadNextLevel();
    }

    // -------------------------------------------------------------------------

    private void LoadNextLevel()
    {
        int next = SelectNext();

        if (next < 0)
        {
            SceneManager.LoadScene(fallbackScene);
            return;
        }

        _currentIndex = next;
        _history.Add(next);
        CurrentLevel = levels[next];

        SceneManager.LoadScene(CurrentLevel.SceneName);
    }

    private int SelectNext() => selectionMode switch
    {
        SelectionMode.Sequential             => SelectSequential(),
        SelectionMode.PureRandom             => SelectPureRandom(),
        SelectionMode.WeightedRandom         => SelectWeighted(allowRepeat: true),
        SelectionMode.WeightedRandomNoRepeat => SelectWeighted(allowRepeat: false),
        _                                    => SelectWeighted(allowRepeat: true),
    };

    private int SelectSequential()
    {
        for (int offset = 1; offset <= levels.Length; offset++)
        {
            int i = (_currentIndex + offset) % levels.Length;
            if (IsEligible(i)) return i;
        }
        return -1;
    }

    private int SelectPureRandom()
    {
        var pool = EligibleIndices(allowRepeat: true);
        return pool.Count > 0 ? pool[Random.Range(0, pool.Count)] : -1;
    }

    private int SelectWeighted(bool allowRepeat)
    {
        var pool = EligibleIndices(allowRepeat);
        if (pool.Count == 0) return -1;

        float total = pool.Sum(i => levels[i].Weight);
        if (total <= 0f) return pool[Random.Range(0, pool.Count)];

        float roll = Random.Range(0f, total), cumulative = 0f;
        foreach (int i in pool)
        {
            cumulative += levels[i].Weight;
            if (roll <= cumulative) return i;
        }
        return pool[^1];
    }

    private List<int> EligibleIndices(bool allowRepeat)
    {
        var result = new List<int>();
        for (int i = 0; i < levels.Length; i++)
        {
            if (!allowRepeat && i == _currentIndex) continue;
            if (IsEligible(i)) result.Add(i);
        }
        return result;
    }

    private bool IsEligible(int i) =>
        levels[i] != null && levels[i].IsEligible(CompletionsThisRun);
}
