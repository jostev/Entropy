using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Source Pool")]
    [SerializeField] private int poolSize = 16;
    private AudioSource[] sources;
    private int sourceIndex;

    [Header("Player / Gun")]
    public AudioClip gunShot;
    public AudioClip bulletImpact;
    public AudioClip zap;
    public AudioClip runLoop;
    public AudioClip achievement;

    [Header("Volume")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float footstepVolume = 0.35f;

    private AudioSource runSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sources = new AudioSource[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            sources[i] = source;
        }

        runSource = gameObject.AddComponent<AudioSource>();
        runSource.playOnAwake = false;
        runSource.loop = true;
        runSource.spatialBlend = 0f;
        runSource.volume = footstepVolume;
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, float pitchRandomness = 0.05f)
    {
        if (clip == null) return;

        AudioSource source = sources[sourceIndex];
        sourceIndex = (sourceIndex + 1) % sources.Length;

        source.clip = clip;
        source.volume = volume * sfxVolume;
        source.pitch = Random.Range(1f - pitchRandomness, 1f + pitchRandomness);
        source.spatialBlend = 0f;
        source.Play();
    }

    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volume = 1f, float pitchRandomness = 0.05f)
    {
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, position, volume * sfxVolume);
    }

    public void SetRunning(bool running)
    {
        if (runSource == null || runLoop == null) return;

        if (running)
        {
            if (!runSource.isPlaying)
            {
                runSource.clip = runLoop;
                runSource.volume = footstepVolume;
                runSource.Play();
            }
        }
        else
        {
            if (runSource.isPlaying)
            {
                runSource.Stop();
            }
        }
    }
}
