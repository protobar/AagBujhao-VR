using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Main menu manager for navigation and scene loading.
/// Works with XR Ray Interactor for button selection.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    [Tooltip("Main menu panel (Start, Quit buttons)")]
    public GameObject mainMenuPanel;
    [Tooltip("Level selection panel")]
    public GameObject levelSelectionPanel;
    [Tooltip("Settings panel (optional)")]
    public GameObject settingsPanel;

    [Header("Level Buttons")]
    [Tooltip("Array of level data (scene name + display name)")]
    public LevelData[] levels;

    [Header("Audio")]
    public AudioSource buttonAudio;
    public AudioClip buttonClickSound;

    // Runtime
    private GameObject currentPanel;

    void Start()
    {
        // Show main menu, hide others
        ShowPanel(mainMenuPanel);
    }

    /// <summary>
    /// Called by Start button
    /// </summary>
    public void OnStartButtonClicked()
    {
        PlayButtonSound();
        ShowPanel(levelSelectionPanel);
    }

    /// <summary>
    /// Called by Back button (from level selection)
    /// </summary>
    public void OnBackButtonClicked()
    {
        PlayButtonSound();
        ShowPanel(mainMenuPanel);
    }

    /// <summary>
    /// Called by Settings button
    /// </summary>
    public void OnSettingsButtonClicked()
    {
        PlayButtonSound();
        if (settingsPanel != null)
            ShowPanel(settingsPanel);
    }

    /// <summary>
    /// Called by Quit button
    /// </summary>
    public void OnQuitButtonClicked()
    {
        PlayButtonSound();
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Load a level by index
    /// </summary>
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Length)
        {
            Debug.LogError($"[MainMenuManager] Invalid level index: {levelIndex}");
            return;
        }

        PlayButtonSound();
        string sceneName = levels[levelIndex].sceneName;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"[MainMenuManager] Level {levelIndex} has no scene name!");
            return;
        }

        Debug.Log($"[MainMenuManager] Loading level: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Load a level by scene name
    /// </summary>
    public void LoadLevel(string sceneName)
    {
        PlayButtonSound();
        SceneManager.LoadScene(sceneName);
    }

    void ShowPanel(GameObject panel)
    {
        if (panel == null) return;

        // Hide current panel
        if (currentPanel != null)
            currentPanel.SetActive(false);

        // Show new panel
        panel.SetActive(true);
        currentPanel = panel;
    }

    void PlayButtonSound()
    {
        if (buttonAudio != null && buttonClickSound != null)
        {
            buttonAudio.PlayOneShot(buttonClickSound);
        }
    }
}

[System.Serializable]
public class LevelData
{
    [Tooltip("Display name shown on button")]
    public string displayName = "Level 1";
    
    [Tooltip("Scene name to load")]
    public string sceneName = "Level1";
    
    [Tooltip("Is this level unlocked?")]
    public bool isUnlocked = true;
    
    [Tooltip("Optional: High score for this level")]
    public int highScore = 0;
}


