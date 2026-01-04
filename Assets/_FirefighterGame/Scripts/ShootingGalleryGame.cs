using UnityEngine;
using TMPro;

/// <summary>
/// Game manager for shooting gallery mode.
/// Tracks score, targets remaining, and win/lose conditions.
/// </summary>
public class ShootingGalleryGame : MonoBehaviour
{
    [Header("Game Settings")]
    [Tooltip("Total targets to spawn")]
    public int totalTargets = 10;
    [Tooltip("Targets to destroy to win")]
    public int targetsToWin = 10;
    [Tooltip("Time limit (0 = no limit)")]
    public float timeLimit = 60f;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI targetsRemainingText;
    public TextMeshProUGUI timeText;
    public GameObject winScreen;
    public GameObject loseScreen;
    public UnityEngine.UI.Button restartButton;

    [Header("Target Spawning")]
    [Tooltip("Target prefab to spawn")]
    public GameObject targetPrefab;
    [Tooltip("Spawn points for targets")]
    public Transform[] spawnPoints;
    [Tooltip("Spawn new target when one is destroyed")]
    public bool autoSpawn = true;
    [Tooltip("Delay before spawning new target")]
    public float spawnDelay = 2f;

    // Runtime
    private int currentScore = 0;
    private int targetsDestroyed = 0;
    private float gameStartTime;
    private bool gameEnded = false;

    void Start()
    {
        gameStartTime = Time.time;

        // Hide screens
        if (winScreen != null) winScreen.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);

        // Setup restart button
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        // Spawn initial targets
        SpawnInitialTargets();
    }

    void Update()
    {
        if (gameEnded) return;

        UpdateUI();
        CheckWinLose();
    }

    void SpawnInitialTargets()
    {
        if (targetPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[ShootingGalleryGame] Missing target prefab or spawn points!");
            return;
        }

        int targetsToSpawn = Mathf.Min(totalTargets, spawnPoints.Length);

        for (int i = 0; i < targetsToSpawn; i++)
        {
            if (spawnPoints[i] != null)
            {
                Instantiate(targetPrefab, spawnPoints[i].position, spawnPoints[i].rotation);
            }
        }
    }

    void UpdateUI()
    {
        // Score
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }

        // Targets remaining
        if (targetsRemainingText != null)
        {
            int remaining = targetsToWin - targetsDestroyed;
            targetsRemainingText.text = $"Targets: {remaining}/{targetsToWin}";
        }

        // Time
        if (timeText != null && timeLimit > 0)
        {
            float timeRemaining = timeLimit - (Time.time - gameStartTime);
            if (timeRemaining < 0) timeRemaining = 0;

            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            timeText.text = $"Time: {minutes:00}:{seconds:00}";

            // Change color if time running out
            if (timeRemaining < 10f)
                timeText.color = Color.red;
            else if (timeRemaining < 30f)
                timeText.color = Color.yellow;
        }
    }

    void CheckWinLose()
    {
        // Win condition
        if (targetsDestroyed >= targetsToWin)
        {
            WinGame();
            return;
        }

        // Lose condition (time limit)
        if (timeLimit > 0 && Time.time - gameStartTime >= timeLimit)
        {
            LoseGame("Time's Up!");
            return;
        }
    }

    public void AddScore(int points)
    {
        currentScore += points;
    }

    public void OnTargetDestroyed()
    {
        targetsDestroyed++;

        // Spawn new target if auto-spawn enabled
        if (autoSpawn && targetsDestroyed < totalTargets)
        {
            Invoke(nameof(SpawnRandomTarget), spawnDelay);
        }
    }

    void SpawnRandomTarget()
    {
        if (spawnPoints == null || spawnPoints.Length == 0 || targetPrefab == null)
            return;

        // Find empty spawn point
        Transform spawnPoint = null;
        foreach (var sp in spawnPoints)
        {
            if (sp != null && !HasTargetAt(sp.position))
            {
                spawnPoint = sp;
                break;
            }
        }

        if (spawnPoint != null)
        {
            Instantiate(targetPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }

    bool HasTargetAt(Vector3 position)
    {
        MovingTarget[] targets = FindObjectsByType<MovingTarget>(FindObjectsSortMode.None);
        foreach (var target in targets)
        {
            if (Vector3.Distance(target.transform.position, position) < 0.5f)
                return true;
        }
        return false;
    }

    void WinGame()
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log("[ShootingGalleryGame] You Win!");

        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    void LoseGame(string reason)
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log($"[ShootingGalleryGame] You Lose! {reason}");

        if (loseScreen != null)
        {
            loseScreen.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}


