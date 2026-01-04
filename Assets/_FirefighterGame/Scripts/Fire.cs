using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// Simple fire that can be extinguished.
/// Add to your fire particle prefab.
/// </summary>
public class Fire : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Fire Effects")]
    public ParticleSystem fireParticles;
    public AudioSource fireAudio;

    [Header("Steam Effect (When Being Extinguished)")]
    [Tooltip("Steam/smoke effect prefab to spawn when water hits fire")]
    public GameObject steamEffectPrefab;
    [Tooltip("Or assign an existing particle system on this object")]
    public ParticleSystem steamParticles;

    [Header("Extinguish Sound")]
    public AudioClip extinguishSound;
    public AudioClip steamSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;

    [Header("Health Display")]
    public bool showHealthBar = true;
    [Tooltip("Auto-create health bar (World Space) or use GameUI manager (Screen Space)")]
    public bool autoCreateHealthBar = false;
    public float healthBarHeight = 1.5f;
    public Color healthBarColor = Color.red;
    public Color healthBarBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

    [Header("Damage Multipliers")]
    [Tooltip("Damage multiplier when hit with water")]
    public float waterMultiplier = 1f;
    [Tooltip("Damage multiplier when hit with nitrogen")]
    public float nitrogenMultiplier = 1.5f;

    // State
    private bool isAlive = true;
    private bool isBeingDamaged = false;
    private float lastDamageTime;

    // Health bar UI (for auto-created world space only)
    private Canvas healthCanvas;
    private Image healthBarFill;
    private Image healthBarBackground;

    // GameUI reference
    private GameUI gameUI;

    public bool IsAlive => isAlive;
    public float HealthPercent => currentHealth / maxHealth;

    void Start()
    {
        currentHealth = maxHealth;

        if (fireParticles == null)
            fireParticles = GetComponentInChildren<ParticleSystem>();

        if (fireAudio == null)
            fireAudio = GetComponent<AudioSource>();

        // Make sure steam is off at start
        if (steamParticles != null)
            steamParticles.Stop();

        // Find GameUI
        gameUI = FindFirstObjectByType<GameUI>();

        // Create health bar only if auto-create is enabled
        if (showHealthBar && autoCreateHealthBar)
            CreateHealthBar();
    }

    void Update()
    {
        // Check if we stopped being damaged (for steam effect)
        if (isBeingDamaged && Time.time - lastDamageTime > 0.2f)
        {
            isBeingDamaged = false;
            StopSteamEffect();
        }

        // Make health bar face camera (only for auto-created world space)
        if (healthCanvas != null && autoCreateHealthBar)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                healthCanvas.transform.LookAt(cam.transform);
                healthCanvas.transform.Rotate(0, 180, 0);
            }
        }
    }

    void CreateHealthBar()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("HealthBar");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = Vector3.up * healthBarHeight;

        healthCanvas = canvasObj.AddComponent<Canvas>();
        healthCanvas.renderMode = RenderMode.WorldSpace;
        healthCanvas.sortingOrder = 100; // Render on top of everything
        
        RectTransform canvasRect = healthCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1f, 0.15f);
        canvasRect.localScale = Vector3.one * 0.5f;

        // Add CanvasGroup to control transparency
        canvasObj.AddComponent<CanvasGroup>();

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform);
        healthBarBackground = bgObj.AddComponent<Image>();
        healthBarBackground.color = healthBarBackgroundColor;
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(canvasObj.transform);
        healthBarFill = fillObj.AddComponent<Image>();
        healthBarFill.color = healthBarColor;
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        // Initially hide until damaged
        healthCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// Call this to damage the fire.
    /// </summary>
    public void TakeDamage(float damage, bool isWater = true)
    {
        if (!isAlive) return;

        float multiplier = isWater ? waterMultiplier : nitrogenMultiplier;
        currentHealth -= damage * multiplier;
        lastDamageTime = Time.time;

        // Show we're being damaged
        if (!isBeingDamaged)
        {
            isBeingDamaged = true;
            StartSteamEffect();
        }

        // Update visuals
        UpdateFireIntensity();
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Extinguish();
        }
    }

    void StartSteamEffect()
    {
        // Play steam particles
        if (steamParticles != null)
        {
            steamParticles.Play();
        }
        else if (steamEffectPrefab != null)
        {
            // Spawn steam effect
            GameObject steam = Instantiate(steamEffectPrefab, transform.position, Quaternion.identity, transform);
            steamParticles = steam.GetComponent<ParticleSystem>();
            if (steamParticles != null)
                steamParticles.Play();
        }

        // Play steam sound
        if (steamSound != null)
        {
            AudioSource.PlayClipAtPoint(steamSound, transform.position, soundVolume);
        }
    }

    void StopSteamEffect()
    {
        if (steamParticles != null)
        {
            steamParticles.Stop();
        }
    }

    void UpdateHealthBar()
    {
        // GameUI handles screen space health bars
        // Only update auto-created world space bars here
        if (healthCanvas != null && !healthCanvas.gameObject.activeSelf)
            healthCanvas.gameObject.SetActive(true);

        if (healthBarFill != null)
        {
            RectTransform fillRect = healthBarFill.GetComponent<RectTransform>();
            fillRect.anchorMax = new Vector2(HealthPercent, 1f);

            // Change color based on health
            if (HealthPercent > 0.5f)
                healthBarFill.color = Color.Lerp(Color.yellow, Color.red, (1f - HealthPercent) * 2f);
            else
                healthBarFill.color = Color.Lerp(Color.green, Color.yellow, (0.5f - HealthPercent) * 2f);
        }
    }

    void UpdateFireIntensity()
    {
        if (fireParticles == null) return;

        float healthPercent = currentHealth / maxHealth;

        var main = fireParticles.main;
        main.startSizeMultiplier = Mathf.Lerp(0.2f, 1f, healthPercent);

        var emission = fireParticles.emission;
        emission.rateOverTimeMultiplier = Mathf.Lerp(0.2f, 1f, healthPercent);
    }

    void Extinguish()
    {
        isAlive = false;
        Debug.Log($"[Fire] {gameObject.name} extinguished!");

        // Stop fire
        if (fireParticles != null)
            fireParticles.Stop();

        if (fireAudio != null)
            fireAudio.Stop();

        // Stop steam
        StopSteamEffect();

        // Play extinguish sound
        if (extinguishSound != null)
        {
            AudioSource.PlayClipAtPoint(extinguishSound, transform.position, soundVolume);
        }

        // Hide health bar
        if (healthCanvas != null)
            healthCanvas.gameObject.SetActive(false);

        // Notify GameUI
        if (gameUI != null)
        {
            gameUI.OnFireExtinguished(this);
            
            // Refresh arrow guide to point to next fire
            if (gameUI.fireArrowGuide != null)
                gameUI.fireArrowGuide.RefreshTarget();
        }

        // Destroy after particles fade out
        Destroy(gameObject, 2f);
    }
}
