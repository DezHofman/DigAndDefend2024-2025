using UnityEngine;
using UnityEngine.UI;
using TMPro; // Import TextMeshPro namespace
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
    [SerializeField] private Button gameOverRetryButton; // Button for retry on game over canvas
    [SerializeField] private Button winRetryButton;     // Button for retry on win canvas
    [SerializeField] private Button previousButton; // Button for previous page
    [SerializeField] private Button nextButton; // Button for next page
    [SerializeField] private Button closeButton; // Button to close guide
    [SerializeField] private GameObject[] guidePanels; // Single array for all panels
    public int time;

    private bool isPaused = false;
    private int currentPage = 1; // Start at page 1
    private const int itemsPerPage = 1; // One panel per page

    private void Start()
    {
        // Initial setup
        ApplyTimeScale();
        ShowMainMenu();

        // Assign button listeners
        playButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(OpenSettings);
        guideButton.onClick.AddListener(OpenGuide);
        quitButtonMain.onClick.AddListener(QuitGame);
        continueButton.onClick.AddListener(ContinueGame);
        settingsButtonPause.onClick.AddListener(OpenSettings);
        quitButtonPause.onClick.AddListener(RestartToMainMenu);
        gameOverRetryButton.onClick.AddListener(RetryGame); // Retry button for game over
        winRetryButton.onClick.AddListener(RetryGame);      // Retry button for win
        previousButton.onClick.AddListener(PreviousPage);
        nextButton.onClick.AddListener(NextPage);
        closeButton.onClick.AddListener(CloseGuide);

        // Ensure all panels are initially inactive
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
        isPaused = true; // Main menu is a paused state
        ApplyTimeScale();
    }

    private void StartGame()
    {
        mainMenuCanvas.enabled = false; // Disable main menu when starting game
        inGameCanvas.enabled = true;
        pauseMenuCanvas.enabled = false;
        gameOverCanvas.enabled = false;
        winCanvas.enabled = false;
        guideMenuCanvas.enabled = false;
        EnableMainMenuButtons(false);
        isPaused = false; // Game is running
        ApplyTimeScale();
    }

    private void PauseGame()
    {
        isPaused = true;
        pauseMenuCanvas.enabled = true;
        ApplyTimeScale(); // Set Time.timeScale to 0
    }

    private void ContinueGame()
    {
        isPaused = false;
        pauseMenuCanvas.enabled = false;
        SettingsMenu settings = FindFirstObjectByType<SettingsMenu>();
        if (settings != null && settings.settingsCanvas != null)
        {
            settings.settingsCanvas.enabled = false; // Close settings if open
        }
        ApplyTimeScale(); // Restore Time.timeScale to time
    }

    private void OpenSettings()
    {
        SettingsMenu settings = FindFirstObjectByType<SettingsMenu>();
        if (settings != null && settings.settingsCanvas != null)
        {
            settings.settingsCanvas.enabled = true;
            isPaused = true; // Pause game when settings are open
            ApplyTimeScale();
        }
        else
        {
            Debug.LogWarning("Settings canvas not assigned or SettingsMenu not found.");
        }
    }

    private void OpenGuide()
    {
        guideMenuCanvas.enabled = true;
        inGameCanvas.enabled = false; // Hide in-game UI during guide
        EnableMainMenuButtons(false); // Disable main menu buttons
        currentPage = 1; // Start at page 1
        PopulateGuideMenu();
        isPaused = true; // Pause game during guide
        ApplyTimeScale();
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        // In Editor, log and return to main menu for testing
        Debug.Log("Quit attempted in Editor, returning to main menu");
        RestartToMainMenu();
#else
        // In built application, quit the game
        Application.Quit();
#endif
    }

    private void RestartToMainMenu()
    {
        // Destroy any persistent GameManager before reloading
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            Destroy(gameManager.gameObject);
            Debug.Log("Destroyed persistent GameManager");
        }

        // Reload the current scene to fully restart the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void RetryGame()
    {
        // Destroy any persistent GameManager before reloading
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            Destroy(gameManager.gameObject);
            Debug.Log("Destroyed persistent GameManager");
        }

        // Reload the current scene and start directly in-game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        // Ensure in-game state is set after loading (rely on StartGame logic)
        StartGame();
    }

    private void ShowGameOver()
    {
        gameOverCanvas.enabled = true;
        inGameCanvas.enabled = false;
        EnableMainMenuButtons(false);
        isPaused = true; // Game over is a paused state
        ApplyTimeScale();
    }

    private void ShowWin()
    {
        winCanvas.enabled = true;
        inGameCanvas.enabled = false;
        EnableMainMenuButtons(false);
        isPaused = true; // Win is a paused state
        ApplyTimeScale();
    }

    private void PopulateGuideMenu()
    {
        // Deactivate all panels
        foreach (var panel in guidePanels)
        {
            if (panel != null) panel.SetActive(false);
        }

        // Activate the current page's panel
        int index = currentPage - 1; // Convert to 0-based index
        if (index >= 0 && index < guidePanels.Length && guidePanels[index] != null)
        {
            guidePanels[index].SetActive(true);
            Debug.Log($"Activating panel at index {index}");
        }
    }

    private void PreviousPage()
    {
        if (currentPage > 1) // Start at 1, so check > 1
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
        inGameCanvas.enabled = true; // Return to in-game state
        EnableMainMenuButtons(true); // Re-enable main menu buttons
        isPaused = false; // Resume game
        ApplyTimeScale();
        RestartToMainMenu(); // Send back to main menu with full restart
    }

    private void EnableMainMenuButtons(bool enabled)
    {
        playButton.interactable = enabled;
        settingsButton.interactable = enabled;
        guideButton.interactable = enabled;
        quitButtonMain.interactable = enabled;
    }

    // Public methods for external calls (e.g., from game logic)
    public void TriggerGameOver()
    {
        ShowGameOver();
    }

    public void TriggerWin()
    {
        ShowWin();
    }
}