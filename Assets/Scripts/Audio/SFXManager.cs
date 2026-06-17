using System;
using UnityEngine;

// Plays SFX by string id using a custom HashMap 
public class SFXManager : MonoBehaviour
{
    [System.Serializable]
    public struct SoundEntry
    {
        // The name for the sound.
        public string id;

        // The sound file.
        public AudioClip clip;
    }

    // Singleton.
    public static SFXManager Instance { get; private set; }

    [Header("Sound Library")]
    // Like a list of SFX.
    [SerializeField] private SoundEntry[] sounds;

    [Header("Playback")]
    // How loud to play the sounds. 1.0 is full volume.
    [SerializeField] private float volume = 1f;
    [SerializeField] private bool persistAcrossScenes = true;

    [Header("Background Music")]
    [SerializeField] private string backgroundMusicId = BgmSoundId;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.4f;
    [SerializeField] private bool playMusicOnStart = true;

    // The map that lets us look up a clip by its name quickly.
    // Uses the custom hash map.
    private readonly CustomHashMap<string, AudioClip> soundMap = new CustomHashMap<string, AudioClip>();

    // The AudioSource that actually plays the sounds.
    private AudioSource audioSource;
    private AudioSource musicSource;
    private bool musicPausedByGame;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // If it should persist, make it do not destroy.
        if (persistAcrossScenes)
            DontDestroyOnLoad(gameObject);

        // Try to find an AudioSource on the speaker.
        audioSource = GetComponent<AudioSource>();

        // If there isn't one, add it so we can play sounds.
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // don't play sounds automatically when the scene starts.
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.priority = 0;

        SetupMusicSource();

        // Build the quick lookup map from the Inspector list.
        BuildSoundMap();
        PreloadSounds();
    }

    private void Start()
    {
        if (playMusicOnStart)
            PlayBackgroundMusic();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        musicVolume = Mathf.Clamp01(musicVolume);
        volume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }
#endif

    private void SetupMusicSource()
    {
        var sources = GetComponents<AudioSource>();
        musicSource = sources.Length > 1 ? sources[1] : gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.priority = 128;
    }

    private void OnDestroy()
    {
        // clear the Instance so a new manager can be created later.
        if (Instance == this)
            Instance = null;
    }

    private void BuildSoundMap()
    {
        soundMap.Clear();

        if (sounds == null)
            return;

        foreach (SoundEntry entry in sounds)
        {
            // If the name is empty or the clip is missing, skip this entry.
            if (string.IsNullOrWhiteSpace(entry.id) || entry.clip == null)
                continue;

            // Put the name + clip into the map.
            RegisterSound(entry.id, entry.clip);
        }
    }

    private void PreloadSounds()
    {
        if (sounds == null)
            return;

        foreach (SoundEntry entry in sounds)
        {
            if (entry.clip == null)
                continue;

            if (IsBackgroundMusicId(entry.id))
                continue;

            if (entry.clip.loadState == AudioDataLoadState.Unloaded)
                entry.clip.LoadAudioData();
        }
    }

    // Stores or updates a sound in the HashMap.
    public void RegisterSound(string id, AudioClip clip)
    {
        // Ignore bad input: no name or no clip.
        if (string.IsNullOrWhiteSpace(id) || clip == null)
            return;

        // Put the clip into the map under the given name.
        soundMap.Put(id, clip);
    }

    // Looks up the clip by string id in the HashMap and plays it.
    public void PlaySound(string id)
    {
        // If the caller passed an empty name, do nothing.
        if (string.IsNullOrWhiteSpace(id))
            return;

        if (IsBackgroundMusicId(id))
        {
            Debug.LogWarning($"SFXManager: '{id}' is background music; use PlayBackgroundMusic() instead.");
            return;
        }

        // Try to get the clip from the map.
        if (!soundMap.TryGet(id, out AudioClip clip) || clip == null)
        {
            Debug.LogWarning($"SFXManager: no sound registered for id '{id}'.");
            return;
        }

        // Play the clip once.
        audioSource.PlayOneShot(clip, volume);
    }

    public void PlayBackgroundMusic()
    {
        if (musicSource == null)
            return;

        if (musicSource.isPlaying && musicSource.clip != null)
            return;

        if (!TryGetBackgroundMusicClip(out AudioClip clip))
            return;

        musicSource.clip = clip;
        ApplyVolumes();
        musicSource.Play();
        musicPausedByGame = false;
    }

    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = Mathf.Clamp01(value);
            ApplyVolumes();
        }
    }

    public float SfxVolume
    {
        get => volume;
        set => volume = Mathf.Clamp01(value);
    }

    private void ApplyVolumes()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    public void PauseBackgroundMusic()
    {
        if (musicSource == null || !musicSource.isPlaying)
            return;

        musicSource.Pause();
        musicPausedByGame = true;
    }

    public void ResumeBackgroundMusic()
    {
        if (musicSource == null || musicSource.clip == null || !musicPausedByGame)
            return;

        musicSource.UnPause();
        musicPausedByGame = false;
    }

    private bool TryGetBackgroundMusicClip(out AudioClip clip)
    {
        if (soundMap.TryGet(backgroundMusicId, out clip) && clip != null)
            return true;

        clip = Resources.Load<AudioClip>("SFX/BGM");
        if (clip != null)
        {
            RegisterSound(backgroundMusicId, clip);
            return true;
        }

        Debug.LogWarning($"SFXManager: no background music clip for id '{backgroundMusicId}'.");
        clip = null;
        return false;
    }

    private static bool IsBackgroundMusicId(string id)
    {
        return string.Equals(id, BgmSoundId, StringComparison.OrdinalIgnoreCase);
    }

    public const string ClickSoundId = "click";
    public const string CrashSoundId = "crash";
    public const string HurtSoundId = "hurt";
    public const string NotificationSoundId = "notification";
    public const string ShootSoundId = "shoot";
    public const string JumpSoundId = "jump";
    public const string BgmSoundId = "BGM";

    public static void Play(string id)
    {
        // If the singleton exists, play sound.
        if (Instance != null)
            Instance.PlaySound(id);
    }

    public static void PauseMusic()
    {
        Instance?.PauseBackgroundMusic();
    }

    public static void ResumeMusic()
    {
        Instance?.ResumeBackgroundMusic();
    }

    public static void SetMusicVolume(float value)
    {
        if (Instance != null)
            Instance.MusicVolume = value;
    }
}
