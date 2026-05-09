using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlaylistManager : MonoBehaviour
{
    [Header("Music Playlist")]
    public AudioClip[] songs;
    public bool shuffle = false;

    [Header("Settings")]
    public float volume = 0.35f;
    public float delayBetweenSongs = 0.5f;

    private AudioSource audioSource;
    private int currentSongIndex = 0;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = volume;
    }

    void Start()
    {
        if (songs.Length > 0)
        {
            StartCoroutine(PlayPlaylist());
        }
        else
        {
            Debug.LogWarning("MusicPlaylistManager: No songs assigned.");
        }
    }

    private IEnumerator PlayPlaylist()
    {
        while (true)
        {
            if (shuffle)
            {
                currentSongIndex = Random.Range(0, songs.Length);
            }

            AudioClip song = songs[currentSongIndex];

            audioSource.clip = song;
            audioSource.volume = volume;
            audioSource.Play();

            yield return new WaitForSeconds(song.length + delayBetweenSongs);

            if (!shuffle)
            {
                currentSongIndex++;

                if (currentSongIndex >= songs.Length)
                {
                    currentSongIndex = 0;
                }
            }
        }
    }
}
