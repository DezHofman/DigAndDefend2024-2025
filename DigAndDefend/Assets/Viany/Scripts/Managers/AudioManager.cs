using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip gameClip;
    [SerializeField] private AudioClip menuClip;
    [SerializeField] private float fadeDuration = 1.0f; // Duration of fade in/out in seconds

    private float targetVolume = 1.0f; // Default volume (0-1)
    private bool isFading;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        LoadVolume(); // Load saved volume on startup
        UpdateVolume();
    }

    private void Start()
    {
        UpdateMusicBasedOnCanvas();
    }

    private void Update()
    {
        UpdateMusicBasedOnCanvas();
    }

    private void UpdateMusicBasedOnCanvas()
    {
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null && gm.mainMenuCanvas != null)
        {
            bool isMainMenuOpen = gm.mainMenuCanvas.enabled;
            AudioClip targetClip = isMainMenuOpen ? menuClip : gameClip;

            if (audioSource.clip != targetClip && !isFading)
            {
                StartCoroutine(FadeAndSwitchClip(targetClip));
            }
        }
    }

    private IEnumerator FadeAndSwitchClip(AudioClip newClip)
    {
        isFading = true;
        float startVolume = audioSource.volume;

        // Fade out current clip
        if (audioSource.isPlaying)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeDuration);
                yield return null;
            }
            audioSource.Stop();
            audioSource.volume = 0f;
        }

        // Switch to new clip
        audioSource.clip = newClip;
        audioSource.Play();

        // Fade in new clip
        float elapsedTimeIn = 0f;
        while (elapsedTimeIn < fadeDuration)
        {
            elapsedTimeIn += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, elapsedTimeIn / fadeDuration);
            yield return null;
        }
        audioSource.volume = targetVolume;
        isFading = false;
    }

    public void SetVolume(float volume)
    {
        targetVolume = Mathf.Clamp01(volume / 100f); // Convert 0-100 to 0-1
        UpdateVolume();
        PlayerPrefs.SetFloat("MasterVolume", volume); // Save as percentage
        PlayerPrefs.Save();
        Debug.Log($"Set volume to {volume}% ({targetVolume})");
    }

    private void UpdateVolume()
    {
        if (!isFading)
        {
            audioSource.volume = targetVolume;
        }
    }

    private void LoadVolume()
    {
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume");
            targetVolume = Mathf.Clamp01(savedVolume / 100f);
            Debug.Log($"Loaded volume from PlayerPrefs: {savedVolume}%");
        }
    }
}