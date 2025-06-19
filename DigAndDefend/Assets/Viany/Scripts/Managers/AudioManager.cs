using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip gameClip;
    [SerializeField] private AudioClip menuClip;
    [SerializeField] private float fadeDuration = 1.0f;

    private float targetVolume = 1.0f;
    private bool isFading;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                transform.SetParent(null, true);
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        LoadVolume();
        UpdateVolume();
    }

    private void Start()
    {
        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null && gm.mainMenuCanvas != null)
        {
            AudioClip targetClip = gm.mainMenuCanvas.enabled ? menuClip : gameClip;
            audioSource.clip = targetClip;
            audioSource.volume = targetVolume;
            audioSource.Play();
        }
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

        audioSource.clip = newClip;
        audioSource.Play();

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
        targetVolume = Mathf.Clamp01(volume / 100f);
        UpdateVolume();
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
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
        }
    }
}