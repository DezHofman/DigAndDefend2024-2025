using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Canvas mainMenuCanvas;
    [SerializeField] private Canvas inGameCanvas;
    [SerializeField] private Canvas pauseMenuCanvas;
    [SerializeField] private Canvas gameOverCanvas;
    [SerializeField] private Canvas winCanvas;
    [SerializeField] private Canvas guideMenuCanvas;
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button guideButton;
    [SerializeField] private Button quitButtonMain;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButtonPause;
    [SerializeField] private Button quitButtonPause;
    [SerializeField] private Button gameOverRetryButton;
    [SerializeField] private Button winRetryButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject[] guidePanels;
    public int time;

    private bool isPaused;
    private int currentPage = 1;
    private const int itemsPerPage = 1;

    private void Start()
    {
        ApplyTimeScale();
        ShowMainMenu();

        playButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(OpenSettings);
        guideButton.onClick.AddListener(OpenGuide);
        quitButtonMain.onClick.AddListener(QuitGame);
        continueButton.onClick.AddListener(ContinueGame);
        settingsButtonPause.onClick.AddListener(OpenSettings);
        quitButtonPause.onClick.AddListener(RestartToMainMenu);
        gameOverRetryButton.onClick.AddListener(RetryGame);
        winRetryButton.onClick.AddListener(RetryGame);
        previousButton.onClick.AddListener(PreviousPage);
        nextButton.onClick.AddListener(NextPage);
        closeButton.onClick.AddListener(CloseGuide);

        foreach (var panel in guidePanels)
        {
            if (panel != null) panel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !mainMenuCanvas.enabled && !gameOverCanvas.enabled && !winCanvas.enabled)
        {
            if (isPaused)
            {
                ContinueGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void ApplyTimeScale()
    {
        Time.timeScale = isPaused ? 0f : time;
    }

    private void ShowMainMenu()
    {
        mainMenuCanvas.enabled = true;
        inGameCanvas.enabled = false;
        pauseMenuCanvas.enabled = false;
        gameOverCanvas.enabled = false;
        winCanvas.enabled = false;
        guideMenuCanvas.enabled = false;
        EnableMainMenuButtons(true);
        isPaused = true;
        ApplyTimeScale();
    }

    private void StartGame()
    {
        mainMenuCanvas.enabled = false;
        inGameCanvas.enabled = true;
        pauseMenuCanvas.enabled = false;
        gameOverCanvas.enabled = false;
        winCanvas.enabled = false;
        guideMenuCanvas.enabled = false;
        EnableMainMenuButtons(false);
        isPaused = false;
        ApplyTimeScale();
    }

    private void PauseGame()
    {
        isPaused = true;
        pauseMenuCanvas.enabled = true;
        ApplyTimeScale();
    }

    private void ContinueGame()
    {
        isPaused = false;
        pauseMenuCanvas.enabled = false;
        SettingsMenu settings = FindFirstObjectByType<SettingsMenu>();
        if (settings != null && settings.settingsCanvas != null)
        {
            settings.settingsCanvas.enabled = false;
        }
        ApplyTimeScale();
    }

    private void OpenSettings()
    {
        SettingsMenu settings = FindFirstObjectByType<SettingsMenu>();
        if (settings != null && settings.settingsCanvas != null)
        {
            settings.settingsCanvas.enabled = true;
            isPaused = true;
            ApplyTimeScale();
        }
    }

    private void OpenGuide()
    {
        guideMenuCanvas.enabled = true;
        inGameCanvas.enabled = false;
        EnableMainMenuButtons(false);
        currentPage = 1;
        PopulateGuideMenu();
        isPaused = true;
        ApplyTimeScale();
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        RestartToMainMenu();
#else
        // In built application, quit the game
        Application.Quit();
#endif
    }

    private void RestartToMainMenu()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            Destroy(gameManager.gameObject);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void RetryGame()
    {
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            Destroy(gameManager.gameObject);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        StartGame();
    }

    private void ShowGameOver()
    {
        gameOverCanvas.enabled = true;
        inGameCanvas.enabled = false;
        EnableMainMenuButtons(false);
        isPaused = true;
        ApplyTimeScale();
    }

    private void ShowWin()
    {
        winCanvas.enabled = true;
        inGameCanvas.enabled = false;
        EnableMainMenuButtons(false);
        isPaused = true;
        ApplyTimeScale();
    }

    private void PopulateGuideMenu()
    {
        foreach (var panel in guidePanels)
        {
            if (panel != null) panel.SetActive(false);
        }

        int index = currentPage - 1;
        if (index >= 0 && index < guidePanels.Length && guidePanels[index] != null)
        {
            guidePanels[index].SetActive(true);
        }
    }

    private void PreviousPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            PopulateGuideMenu();
        }
    }

    private void NextPage()
    {
        int maxPages = guidePanels.Length;
        if (currentPage < maxPages)
        {
            currentPage++;
            PopulateGuideMenu();
        }
    }

    private void CloseGuide()
    {
        guideMenuCanvas.enabled = false;
        inGameCanvas.enabled = true;
        EnableMainMenuButtons(true);
        isPaused = false;
        ApplyTimeScale();
        RestartToMainMenu();
    }

    private void EnableMainMenuButtons(bool enabled)
    {
        playButton.interactable = enabled;
        settingsButton.interactable = enabled;
        guideButton.interactable = enabled;
        quitButtonMain.interactable = enabled;
    }

    public void TriggerGameOver()
    {
        ShowGameOver();
    }

    public void TriggerWin()
    {
        ShowWin();
    }
}