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
    [SerializeField] private Button gameOverButton;
    [SerializeField] private Button winButton;
    [SerializeField] private Button previousButton; // Button for previous page
    [SerializeField] private Button nextButton; // Button for next page
    [SerializeField] private Button closeButton; // Button to close guide
    [SerializeField] private GameObject[] guidePanels; // Single array for all panels

    private bool isPaused = false;
    private int currentPage = 1; // Start at page 1
    private const int itemsPerPage = 1; // One panel per page

    private void Start()
    {
        // Initial setup
        Time.timeScale = 1f;
        ShowMainMenu();

        // Assign button listeners
        playButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(OpenSettings);
        guideButton.onClick.AddListener(OpenGuide);
        quitButtonMain.onClick.AddListener(QuitGame);
        continueButton.onClick.AddListener(ContinueGame);
        settingsButtonPause.onClick.AddListener(OpenSettings);
        quitButtonPause.onClick.AddListener(RestartToMainMenu);
        gameOverButton.onClick.AddListener(RestartToMainMenu);
        winButton.onClick.AddListener(RestartToMainMenu);
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
                ContinueGame();
            else
                PauseGame();
        }
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
        Time.timeScale = 0f;
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
        Time.timeScale = 1f;
    }

    private void PauseGame()
    {
        isPaused = true;
        pauseMenuCanvas.enabled = true;
        Time.timeScale = 0f; // inGameCanvas remains enabled
    }

    private void ContinueGame()
    {
        isPaused = false;
        pauseMenuCanvas.enabled = false;
        Time.timeScale = 1f;
    }

    private void OpenSettings()
    {
        // Placeholder for future settings implementation
        Debug.Log("Settings menu opened (to be implemented)");
    }

    private void OpenGuide()
    {
        guideMenuCanvas.enabled = true;
        inGameCanvas.enabled = false; // Hide in-game UI during guide
        EnableMainMenuButtons(false); // Disable main menu buttons
        currentPage = 1; // Start at page 1
        PopulateGuideMenu();
        Time.timeScale = 0f;
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
        GameObject gameManager = GameObject.Find("GameManager");
        if (gameManager != null)
        {
            Destroy(gameManager);
            Debug.Log("Destroyed persistent GameManager");
        }

        // Reload the current scene to fully restart the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ShowGameOver()
    {
        inGameCanvas.enabled = false;
        gameOverCanvas.enabled = true;
        EnableMainMenuButtons(false);
        Time.timeScale = 0f;
    }

    private void ShowWin()
    {
        inGameCanvas.enabled = false;
        winCanvas.enabled = true;
        EnableMainMenuButtons(false);
        Time.timeScale = 0f;
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
        Time.timeScale = 1f;
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