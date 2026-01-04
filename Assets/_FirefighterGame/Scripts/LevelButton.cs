using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// Button for level selection.
/// Attach to button GameObject, works with XR Ray Interactor.
/// </summary>
[RequireComponent(typeof(Button))]
public class LevelButton : MonoBehaviour
{
    [Header("Level Info")]
    [Tooltip("Level index in MainMenuManager's levels array")]
    public int levelIndex = 0;
    [Tooltip("Or load by scene name directly")]
    public string sceneName = "";

    [Header("UI References")]
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI highScoreText;
    public Image lockedIcon;
    public Image unlockedIcon;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color lockedColor = Color.gray;
    public Color hoverColor = new Color(0.8f, 0.8f, 1f);

    // Components
    private Button button;
    private MainMenuManager menuManager;
    private bool isLocked = false;

    void Awake()
    {
        button = GetComponent<Button>();
        
        // Make sure button works with XR
        if (GetComponent<XRUIInputModule>() == null)
        {
            // Add TrackedDeviceGraphicRaycaster if using XR
            // This is usually handled by XR Interaction Toolkit automatically
        }
    }

    void Start()
    {
        menuManager = FindFirstObjectByType<MainMenuManager>();

        if (menuManager != null && menuManager.levels != null && levelIndex < menuManager.levels.Length)
        {
            LevelData levelData = menuManager.levels[levelIndex];
            isLocked = !levelData.isUnlocked;

            // Update UI
            if (levelNameText != null)
                levelNameText.text = levelData.displayName;

            if (highScoreText != null && levelData.highScore > 0)
                highScoreText.text = $"Best: {levelData.highScore}";
            else if (highScoreText != null)
                highScoreText.text = "";

            // Show/hide locked icon
            if (lockedIcon != null)
                lockedIcon.gameObject.SetActive(isLocked);

            if (unlockedIcon != null)
                unlockedIcon.gameObject.SetActive(!isLocked);

            // Disable button if locked
            if (button != null)
                button.interactable = !isLocked;
        }
        else if (!string.IsNullOrEmpty(sceneName))
        {
            // Use scene name directly
            if (levelNameText != null)
                levelNameText.text = sceneName;
        }

        // Setup button click
        if (button != null)
            button.onClick.AddListener(OnButtonClicked);
    }

    void OnButtonClicked()
    {
        if (isLocked)
        {
            Debug.Log("[LevelButton] Level is locked!");
            return;
        }

        if (menuManager != null && !string.IsNullOrEmpty(sceneName))
        {
            menuManager.LoadLevel(sceneName);
        }
        else if (menuManager != null)
        {
            menuManager.LoadLevel(levelIndex);
        }
        else if (!string.IsNullOrEmpty(sceneName))
        {
            // Fallback: load directly
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>
    /// Set button as locked/unlocked
    /// </summary>
    public void SetLocked(bool locked)
    {
        isLocked = locked;

        if (button != null)
            button.interactable = !locked;

        if (lockedIcon != null)
            lockedIcon.gameObject.SetActive(locked);

        if (unlockedIcon != null)
            unlockedIcon.gameObject.SetActive(!locked);
    }
}


