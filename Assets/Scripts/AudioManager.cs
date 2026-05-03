using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField][Range(0f, 1f)] private float musicVolume = 0.75f;

    [Header("SFX Clips")]
    [SerializeField] public AudioClip sfxCrash;
    [SerializeField] public AudioClip sfxFuelPickup;
    [SerializeField] public AudioClip sfxCoinPickup;
    [SerializeField] public AudioClip sfxLandingSuccess;
    [SerializeField] public AudioClip sfxThruster;
    [SerializeField] public AudioClip sfxButtonClick;
    [SerializeField] public AudioClip sfxButtonHover;
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 0.85f;

    private const string KEY_MUSIC = "MusicEnabled";
    private const string KEY_SFX = "SFXEnabled";

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioSource thrusterSource;

    public bool MusicEnabled { get; private set; }
    public bool SFXEnabled { get; private set; }

    private void Awake()
    {
        // If an instance already exists and it's not this one, destroy this one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Make THIS GameObject persist — works whether it's on GameManager or standalone
        // If GameManager itself is already DontDestroyOnLoad, this is redundant but harmless
        DontDestroyOnLoad(gameObject);

        // Default ON — only override if player has explicitly saved a preference
        MusicEnabled = PlayerPrefs.GetInt(KEY_MUSIC, 1) == 1;
        SFXEnabled = PlayerPrefs.GetInt(KEY_SFX, 1) == 1;

        SetupSources();
    }

    private void SetupSources()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;
        musicSource.spatialBlend = 0f;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
        sfxSource.spatialBlend = 0f;

        thrusterSource = gameObject.AddComponent<AudioSource>();
        thrusterSource.loop = true;
        thrusterSource.playOnAwake = false;
        thrusterSource.volume = sfxVolume * 0.55f;
        thrusterSource.spatialBlend = 0f;
        if (sfxThruster != null) thrusterSource.clip = sfxThruster;
    }

    private void Start()
    {
        // Start music in Start so AudioSource is fully initialised
        if (MusicEnabled && musicClip != null)
        {
            musicSource.Play();
            Debug.Log("AudioManager: Music started");
        }
        else
        {
            Debug.Log($"AudioManager: Music NOT started. MusicEnabled={MusicEnabled}, clip={musicClip}");
        }
    }

    // ── SFX ───────────────────────────────────────────────────────────────
    public void PlaySFX(AudioClip clip)
    {
        if (!SFXEnabled || clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayCrash() => PlaySFX(sfxCrash);
    public void PlayFuelPickup() => PlaySFX(sfxFuelPickup);
    public void PlayCoinPickup() => PlaySFX(sfxCoinPickup);
    public void PlayLandingSuccess() => PlaySFX(sfxLandingSuccess);
    public void PlayButtonClick() => PlaySFX(sfxButtonClick);
    public void PlayButtonHover() => PlaySFX(sfxButtonHover);

    public void StartThruster()
    {
        if (!SFXEnabled || thrusterSource == null || sfxThruster == null) return;
        if (!thrusterSource.isPlaying) thrusterSource.Play();
    }

    public void StopThruster()
    {
        if (thrusterSource != null && thrusterSource.isPlaying)
            thrusterSource.Stop();
    }

    // ── Toggles ───────────────────────────────────────────────────────────
    public void ToggleMusic()
    {
        MusicEnabled = !MusicEnabled;
        PlayerPrefs.SetInt(KEY_MUSIC, MusicEnabled ? 1 : 0);
        PlayerPrefs.Save();

        if (MusicEnabled)
        {
            if (musicClip != null && !musicSource.isPlaying)
                musicSource.Play();
        }
        else
        {
            musicSource.Stop();
        }

        Debug.Log($"AudioManager: Music toggled = {MusicEnabled}");
    }

    public void ToggleSFX()
    {
        SFXEnabled = !SFXEnabled;
        PlayerPrefs.SetInt(KEY_SFX, SFXEnabled ? 1 : 0);
        PlayerPrefs.Save();

        if (!SFXEnabled) thrusterSource?.Stop();

        Debug.Log($"AudioManager: SFX toggled = {SFXEnabled}");
    }

    public void EnsureMusicPlaying()
    {
        if (MusicEnabled && musicClip != null && musicSource != null && !musicSource.isPlaying)
            musicSource.Play();
    }
}