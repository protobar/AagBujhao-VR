using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Manages win/lose conditions and game state.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Win Conditions")]
    public bool winOnAllFiresExtinguished = true;
    public float winDelay = 2f; // Wait before showing win screen

    [Header("Lose Conditions")]
    public bool loseOnTimeLimit = false;
    public float timeLimit = 300f; // 5 minutes default
    public bool loseOnTooManyFires = false;
    public int maxFiresAllowed = 10;

    [Header("UI References")]
    public GameObject winScreen;
    public GameObject loseScreen;
    public TextMeshProUGUI winScoreText;
    public TextMeshProUGUI winTimeText;
    public TextMeshProUGUI loseReasonText;

    [Header("Buttons")]
    public UnityEngine.UI.Button restartButton;
    public UnityEngine.UI.Button nextLevelButton;
    public UnityEngine.UI.Button mainMenuButton;

    [Header("Other")]
    public bool pauseOnWinLose = true;
    public string nextSceneName = "";

    // Runtime
    private bool gameEnded = false;
    private bool gameWon = false;
    private float gameStartTime;
    private GameUI gameUI;
    private FireArrowGuide arrowGuide;

    void Start()
    {
        gameStartTime = Time.time;
        gameUI = FindFirstObjectByType<GameUI>();
        arrowGuide = FindFirstObjectByType<FireArrowGuide>();

        // Hide screens at start
        if (winScreen != null) winScreen.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);

        SetupButtons();
    }

    void Update()
    {
        if (gameEnded) return;

        CheckWinConditions();
        CheckLoseConditions();
    }

    void SetupButtons()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartLevel);

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(LoadNextLevel);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(LoadMainMenu);
    }

    void CheckWinConditions()
    {
        if (!winOnAllFiresExtinguished) return;

        // Check if all fires are extinguished
        Fire[] allFires = FindObjectsByType<Fire>(FindObjectsSortMode.None);
        int activeFires = 0;

        foreach (var fire in allFires)
        {
            if (fire.IsAlive)
                activeFires++;
        }

        if (activeFires == 0 && allFires.Length > 0)
        {
            StartCoroutine(WinGame());
        }
    }

    void CheckLoseConditions()
    {
        // Time limit
        if (loseOnTimeLimit && !gameEnded)
        {
            float elapsed = Time.time - gameStartTime;
            if (elapsed >= timeLimit)
            {
                LoseGame("Time's Up!");
                return;
            }
        }

        // Too many fires
        if (loseOnTooManyFires && !gameEnded)
        {
            Fire[] allFires = FindObjectsByType<Fire>(FindObjectsSortMode.None);
            int activeFires = 0;

            foreach (var fire in allFires)
            {
                if (fire.IsAlive)
                    activeFires++;
            }

            if (activeFires >= maxFiresAllowed)
            {
                LoseGame("Too Many Fires!");
                return;
            }
        }
    }

    IEnumerator WinGame()
    {
        if (gameEnded) yield break;

        gameEnded = true;
        gameWon = true;

        Debug.Log("[GameManager] You Win!");

        // Hide arrow
        if (arrowGuide != null)
            arrowGuide.SetVisible(false);

        // Wait a bit before showing screen
        yield return new WaitForSeconds(winDelay);

        // Pause game
        if (pauseOnWinLose)
            Time.timeScale = 0f;

        // Show win screen
        if (winScreen != null)
        {
            winScreen.SetActive(true);

            // Update win screen info
            if (winScoreText != null && gameUI != null)
            {
                // Get score from GameUI (you might need to expose it)
                winScoreText.text = $"Score: {GetCurrentScore()}";
            }

            if (winTimeText != null)
            {
                float timeElapsed = Time.time - gameStartTime;
                int minutes = Mathf.FloorToInt(timeElapsed / 60f);
                int seconds = Mathf.FloorToInt(timeElapsed % 60f);
                winTimeText.text = $"Time: {minutes:00}:{seconds:00}";
            }

            // Show/hide next level button
            if (nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(!string.IsNullOrEmpty(nextSceneName));
            }
        }
    }

    public void LoseGame(string reason)
    {
        if (gameEnded) return;

        gameEnded = true;
        gameWon = false;

        Debug.Log($"[GameManager] You Lose! Reason: {reason}");

        // Hide arrow
        if (arrowGuide != null)
            arrowGuide.SetVisible(false);

        // Pause game
        if (pauseOnWinLose)
            Time.timeScale = 0f;

        // Show lose screen
        if (loseScreen != null)
        {
            loseScreen.SetActive(true);

            if (loseReasonText != null)
                loseReasonText.text = reason;
        }
    }

    int GetCurrentScore()
    {
        if (gameUI != null)
        {
            return gameUI.CurrentScore;
        }
        return 0;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadNextLevel()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("[GameManager] No next scene name set!");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Assuming main menu is scene 0
    }

    public bool IsGameEnded => gameEnded;
    public bool IsGameWon => gameWon;
    public float GameTime => Time.time - gameStartTime;
}

