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

    // The map that lets us look up a clip by its name quickly.
    // Uses the custom hash map.
    private readonly CustomHashMap<string, AudioClip> soundMap = new CustomHashMap<string, AudioClip>();

    // The AudioSource that actually plays the sounds.
    private AudioSource audioSource;

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

        // Build the quick lookup map from the Inspector list.
        BuildSoundMap();
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

        // Try to get the clip from the map.
        if (!soundMap.TryGet(id, out AudioClip clip) || clip == null)
        {
            Debug.LogWarning($"SFXManager: no sound registered for id '{id}'.");
            return;
        }

        // Play the clip once at the configured volume.
        audioSource.PlayOneShot(clip, volume);
    }

    // Convenience for other scripts. Example: SFXManager.Play("click");
    public const string ClickSoundId = "click";

    public static void Play(string id)
    {
        // If the singleton exists, ask it to play the sound.
        if (Instance != null)
            Instance.PlaySound(id);
    }
}
