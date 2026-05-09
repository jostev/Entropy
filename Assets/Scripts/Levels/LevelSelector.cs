using System.Collections.Generic;
using System.Linq;
using static UnityEngine.Mathf;

public enum SelectionMode { Sequential, PureRandom, WeightedRandom, WeightedRandomNoRepeat }

public class LevelSelector
{
    private readonly Level[] _levels;
    private readonly System.Random _rng;

    public SelectionMode Mode { get; set; }
    public int CurrentIndex { get; private set; } = -1;

    // Fixed-size ring buffer of the last K played indices (oldest → newest).
    private readonly Queue<int> _history;
    private readonly int _historySize;

    public IReadOnlyCollection<int> History => _history;

    public LevelSelector(Level[] levels, SelectionMode mode, int historySize = 5, int seed = -1)
    {
        _levels      = levels;
        Mode         = mode;
        _historySize = historySize;
        _history     = new Queue<int>(historySize);
        _rng         = seed < 0 ? new System.Random() : new System.Random(seed);
    }

    /// <summary>
    /// Returns the next eligible level index, or -1 if none exist.
    /// </summary>
    public int Next(int completionsThisRun)
    {
        int next = Mode switch
        {
            SelectionMode.Sequential             => SelectSequential(completionsThisRun),
            SelectionMode.PureRandom             => SelectPureRandom(completionsThisRun),
            SelectionMode.WeightedRandom         => SelectWeighted(completionsThisRun),
            SelectionMode.WeightedRandomNoRepeat => SelectWeighted(completionsThisRun),
            _                                    => SelectWeighted(completionsThisRun),
        };

        if (next >= 0)
        {
            CurrentIndex = next;
            PushHistory(next);
        }

        return next;
    }

    public void Reset()
    {
        CurrentIndex = -1;
        _history.Clear();
    }

    // -------------------------------------------------------------------------
    // Selection
    // -------------------------------------------------------------------------

    private int SelectSequential(int completions)
    {
        for (int offset = 1; offset <= _levels.Length; offset++)
        {
            int i = (CurrentIndex + offset) % _levels.Length;
            if (IsEligible(i, completions)) return i;
        }
        return -1;
    }

    private int SelectPureRandom(int completions)
    {
        var pool = EligibleIndices(completions);
        return pool.Count > 0 ? pool[_rng.Next(pool.Count)] : -1;
    }

    /// <summary>
    /// Weighted random where each level's effective weight is scaled down
    /// based on how recently it appeared in history.
    ///
    /// Most recent entry  → multiplied by (1 / historySize)
    /// Oldest entry       → multiplied by (1 - 1/historySize)
    /// Not in history     → full weight, no penalty
    /// </summary>
    private int SelectWeighted(int completions)
    {
        var pool = EligibleIndices(completions);
        if (pool.Count == 0) return -1;

        float[] weights = new float[pool.Count];
        for (int p = 0; p < pool.Count; p++)
            weights[p] = EffectiveWeight(pool[p]);

        float total = weights.Sum();
        if (total <= 0f) return pool[_rng.Next(pool.Count)];

        float roll = (float)_rng.NextDouble() * total;
        float cumulative = 0f;

        for (int p = 0; p < pool.Count; p++)
        {
            cumulative += weights[p];
            if (roll <= cumulative) return pool[p];
        }
        return pool[^1];
    }

    // -------------------------------------------------------------------------
    // History penalty
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reduces a level's weight the more recently it was played.
    /// Recency penalty: position 0 (oldest) → small penalty, last position → largest.
    /// </summary>
    private float EffectiveWeight(int index)
    {
        float baseWeight = _levels[index].Weight;
        int[] historyArray = _history.ToArray(); // oldest → newest

        for (int h = 0; h < historyArray.Length; h++)
        {
            if (historyArray[h] != index) continue;

            // How recent: 0.0 = oldest entry, 1.0 = most recent entry.
            float recency = historyArray.Length > 1
                ? (float)h / (historyArray.Length - 1)
                : 1f;

            // Penalty scales from near-zero (most recent) to mild (oldest).
            float penalty = Lerp(1f - (1f / _historySize), 1f / _historySize, recency);
            return baseWeight * (1f - penalty);
        }

        return baseWeight; // not in history, no penalty
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void PushHistory(int index)
    {
        if (_history.Count == _historySize)
            _history.Dequeue();
        _history.Enqueue(index);
    }

    private List<int> EligibleIndices(int completions)
    {
        var result = new List<int>();
        for (int i = 0; i < _levels.Length; i++)
            if (IsEligible(i, completions)) result.Add(i);
        return result;
    }

    private bool IsEligible(int i, int completions) =>
        _levels[i] != null && _levels[i].IsEligible(completions);
}
