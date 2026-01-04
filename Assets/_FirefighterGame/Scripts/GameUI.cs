using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Universal game UI manager.
/// Handles score, fires remaining, time, and fire health bars.
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("Score Display")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI scoreChangeText;
    [Tooltip("How long score change text shows")]
    public float scoreChangeDuration = 2f;
    public float scoreAnimationSpeed = 500f;

    [Header("Fires Remaining")]
    public TextMeshProUGUI firesRemainingText;
    public string firesRemainingFormat = "Fires: {0}/{1}";

    [Header("Time Display")]
    public TextMeshProUGUI timeText;
    public string timeFormat = "Time: {0:00}:{1:00}";
    public bool countUp = true; // true = count up, false = count down
    public float timeLimit = 0f; // 0 = no limit

    [Header("Fire Health Bars")]
    [Tooltip("Prefab for fire health bar (will be instantiated per fire)")]
    public GameObject fireHealthBarPrefab;
    [Tooltip("Parent for health bars")]
    public Transform healthBarsParent;
    public Color healthBarFullColor = Color.red;
    public Color healthBarMidColor = Color.yellow;
    public Color healthBarLowColor = Color.green;

    [Header("Tank UI (Optional)")]
    public Image tankFillBar;
    public TextMeshProUGUI tankText;
    public Extinguisher extinguisher;

    [Header("Fire Arrow Guide (Optional)")]
    public FireArrowGuide fireArrowGuide;

    // Runtime
    private int currentScore = 0;
    private float displayedScore = 0;
    private float gameStartTime;
    private int totalFires = 0;
    private int extinguishedFires = 0;
    private Dictionary<Fire, GameObject> fireHealthBars = new Dictionary<Fire, GameObject>();
    private float scoreChangeTimer = 0f;

    // Public access
    public int CurrentScore => Mathf.RoundToInt(displayedScore);

    void Start()
    {
        gameStartTime = Time.time;
        
        // Find all fires
        FindAllFires();
        
        // Create health bar parent if not set
        if (healthBarsParent == null)
        {
            GameObject parent = new GameObject("HealthBarsParent");
            parent.transform.SetParent(transform);
            healthBarsParent = parent.transform;
        }
    }

    void Update()
    {
        UpdateScore();
        UpdateTime();
        UpdateFiresRemaining();
        UpdateFireHealthBars();
        UpdateTankUI();
        UpdateScoreChangeText();
    }

    void FindAllFires()
    {
        Fire[] fires = FindObjectsByType<Fire>(FindObjectsSortMode.None);
        totalFires = fires.Length;
        extinguishedFires = 0;

        foreach (var fire in fires)
        {
            if (fire.IsAlive)
                CreateHealthBarForFire(fire);
        }

        UpdateFiresRemaining();
    }

    void CreateHealthBarForFire(Fire fire)
    {
        if (fireHealthBarPrefab == null) return;
        if (fireHealthBars.ContainsKey(fire)) return;

        GameObject barObj = Instantiate(fireHealthBarPrefab, healthBarsParent);
        fireHealthBars[fire] = barObj;
        
        // Initially hide until fire takes damage
        barObj.SetActive(false);
    }

    void UpdateFireHealthBars()
    {
        // Remove bars for extinguished fires
        List<Fire> toRemove = new List<Fire>();
        foreach (var kvp in fireHealthBars)
        {
            if (kvp.Key == null || !kvp.Key.IsAlive)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var fire in toRemove)
        {
            fireHealthBars.Remove(fire);
        }

        // Update existing bars
        foreach (var kvp in fireHealthBars)
        {
            Fire fire = kvp.Key;
            GameObject barObj = kvp.Value;

            if (fire == null || barObj == null) continue;

            // Show bar when fire takes damage
            float healthPercent = fire.HealthPercent;
            if (healthPercent < 1f && !barObj.activeSelf)
                barObj.SetActive(true);

            // Update fill
            Image fillBar = barObj.GetComponentInChildren<Image>();
            if (fillBar != null)
            {
                RectTransform fillRect = fillBar.GetComponent<RectTransform>();
                fillRect.anchorMax = new Vector2(healthPercent, 1f);

                // Update color
                if (healthPercent > 0.5f)
                    fillBar.color = Color.Lerp(healthBarMidColor, healthBarFullColor, (1f - healthPercent) * 2f);
                else
                    fillBar.color = Color.Lerp(healthBarLowColor, healthBarMidColor, healthPercent * 2f);
            }

            // Update text if exists
            TextMeshProUGUI text = barObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = $"{Mathf.RoundToInt(healthPercent * 100)}%";
            }
        }
    }

    void UpdateScore()
    {
        if (scoreText == null) return;

        // Animate score counting up
        if (displayedScore < currentScore)
        {
            displayedScore = Mathf.MoveTowards(displayedScore, currentScore, scoreAnimationSpeed * Time.deltaTime);
        }

        scoreText.text = Mathf.RoundToInt(displayedScore).ToString("N0");
    }

    void UpdateScoreChangeText()
    {
        if (scoreChangeText == null) return;

        if (scoreChangeTimer > 0)
        {
            scoreChangeTimer -= Time.deltaTime;
            
            // Fade out
            CanvasGroup cg = scoreChangeText.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = scoreChangeText.gameObject.AddComponent<CanvasGroup>();
            
            cg.alpha = scoreChangeTimer / scoreChangeDuration;
        }
        else
        {
            scoreChangeText.gameObject.SetActive(false);
        }
    }

    void UpdateTime()
    {
        if (timeText == null) return;

        float elapsed = Time.time - gameStartTime;
        float time = countUp ? elapsed : (timeLimit > 0 ? timeLimit - elapsed : elapsed);

        if (time < 0) time = 0;

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        timeText.text = string.Format(timeFormat, minutes, seconds);

        // Change color if time running out
        if (timeLimit > 0 && !countUp && time < 30f)
        {
            timeText.color = time < 10f ? Color.red : Color.yellow;
        }
    }

    void UpdateFiresRemaining()
    {
        if (firesRemainingText == null) return;

        int remaining = totalFires - extinguishedFires;
        firesRemainingText.text = string.Format(firesRemainingFormat, remaining, totalFires);
    }

    void UpdateTankUI()
    {
        if (extinguisher == null) return;

        if (tankFillBar != null)
        {
            float percent = extinguisher.TankPercent;
            RectTransform fillRect = tankFillBar.GetComponent<RectTransform>();
            fillRect.anchorMax = new Vector2(percent, 1f);

            // Color
            if (percent <= 0.25f)
                tankFillBar.color = Color.red;
            else if (percent <= 0.5f)
                tankFillBar.color = Color.yellow;
            else
                tankFillBar.color = Color.green;
        }

        if (tankText != null)
        {
            tankText.text = $"{Mathf.RoundToInt(extinguisher.TankPercent * 100)}%";
        }
    }

    /// <summary>
    /// Add score (with animation)
    /// </summary>
    public void AddScore(int points, string reason = "")
    {
        currentScore += points;

        // Show score change text
        if (scoreChangeText != null)
        {
            scoreChangeText.text = $"+{points}";
            if (reason != "")
                scoreChangeText.text += $" {reason}";
            
            scoreChangeText.gameObject.SetActive(true);
            scoreChangeTimer = scoreChangeDuration;

            CanvasGroup cg = scoreChangeText.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = scoreChangeText.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
        }
    }

    /// <summary>
    /// Called when a fire is extinguished
    /// </summary>
    public void OnFireExtinguished(Fire fire)
    {
        extinguishedFires++;
        UpdateFiresRemaining();

        // Award score
        AddScore(100, "Fire Extinguished!");

        // Remove health bar
        if (fireHealthBars.ContainsKey(fire))
        {
            if (fireHealthBars[fire] != null)
                Destroy(fireHealthBars[fire]);
            fireHealthBars.Remove(fire);
        }
    }

    /// <summary>
    /// Register a new fire (call from FireSpawner)
    /// </summary>
    public void RegisterFire(Fire fire)
    {
        totalFires++;
        CreateHealthBarForFire(fire);
        UpdateFiresRemaining();
    }

    /// <summary>
    /// Set extinguisher reference
    /// </summary>
    public void SetExtinguisher(Extinguisher ext)
    {
        extinguisher = ext;
    }
}

