using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shows tank level on the extinguisher.
/// Attach to the extinguisher or create separately and link.
/// </summary>
public class ExtinguisherUI : MonoBehaviour
{
    [Header("Extinguisher Reference")]
    public Extinguisher extinguisher;

    [Header("UI Mode")]
    [Tooltip("World = floating near extinguisher, Screen = corner of screen")]
    public UIMode uiMode = UIMode.World;

    [Header("World Space Settings")]
    public Vector3 worldOffset = new Vector3(0, 0.2f, 0);
    public float worldScale = 0.002f;
    public bool faceCamera = true;

    [Header("Screen Space Settings")]
    public Vector2 screenPosition = new Vector2(50, 50);

    [Header("UI Elements (Auto-created if not set)")]
    public Image tankFillBar;
    public TextMeshProUGUI tankText;
    public Image backgroundBar;

    [Header("Colors")]
    public Color fullColor = Color.green;
    public Color midColor = Color.yellow;
    public Color lowColor = Color.red;
    public Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

    [Header("Thresholds")]
    [Range(0, 1)] public float lowThreshold = 0.25f;
    [Range(0, 1)] public float midThreshold = 0.5f;

    // Private
    private Canvas canvas;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (extinguisher == null)
            extinguisher = GetComponentInParent<Extinguisher>();

        if (tankFillBar == null)
            CreateUI();
    }

    void CreateUI()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("TankUI");
        
        if (uiMode == UIMode.World)
        {
            canvasObj.transform.SetParent(extinguisher != null ? extinguisher.transform : transform);
            canvasObj.transform.localPosition = worldOffset;
            canvasObj.transform.localScale = Vector3.one * worldScale;
        }

        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = uiMode == UIMode.World ? RenderMode.WorldSpace : RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        if (uiMode == UIMode.World)
        {
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(200, 30);
        }

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform);
        backgroundBar = bgObj.AddComponent<Image>();
        backgroundBar.color = backgroundColor;
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        if (uiMode == UIMode.World)
        {
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
        }
        else
        {
            bgRect.anchorMin = new Vector2(0, 1);
            bgRect.anchorMax = new Vector2(0, 1);
            bgRect.pivot = new Vector2(0, 1);
            bgRect.anchoredPosition = screenPosition * new Vector2(1, -1);
            bgRect.sizeDelta = new Vector2(200, 30);
        }

        // Fill bar
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(bgObj.transform);
        tankFillBar = fillObj.AddComponent<Image>();
        tankFillBar.color = fullColor;
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.offsetMin = new Vector2(2, 2);
        fillRect.offsetMax = new Vector2(-2, -2);

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(bgObj.transform);
        tankText = textObj.AddComponent<TextMeshProUGUI>();
        tankText.alignment = TextAlignmentOptions.Center;
        tankText.fontSize = uiMode == UIMode.World ? 18 : 14;
        tankText.color = Color.white;
        tankText.text = "100%";
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    void Update()
    {
        if (extinguisher == null) return;

        UpdateUI();

        // Face camera for world space
        if (uiMode == UIMode.World && faceCamera && canvas != null && mainCamera != null)
        {
            canvas.transform.LookAt(mainCamera.transform);
            canvas.transform.Rotate(0, 180, 0);
        }
    }

    void UpdateUI()
    {
        float percent = extinguisher.TankPercent;

        // Update fill
        if (tankFillBar != null)
        {
            RectTransform fillRect = tankFillBar.GetComponent<RectTransform>();
            fillRect.anchorMax = new Vector2(percent, 1);

            // Update color
            if (percent <= lowThreshold)
                tankFillBar.color = lowColor;
            else if (percent <= midThreshold)
                tankFillBar.color = midColor;
            else
                tankFillBar.color = fullColor;
        }

        // Update text
        if (tankText != null)
        {
            tankText.text = $"{Mathf.RoundToInt(percent * 100)}%";
        }
    }
}

public enum UIMode
{
    World,
    Screen
}


