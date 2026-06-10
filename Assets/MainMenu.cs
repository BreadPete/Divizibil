using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject rulesPanel;
    public GameObject settingsPanel;

    [Header("UI Elements")]
    public TextMeshProUGUI highscoreText;
    public Button soundButton; 
    public Button vibrationsButton; 

    private bool soundEnabled = true; 
    private bool vibrationsEnabled = true; 

    void Start()
    {
        // Make sure panels are hidden at start
        if (rulesPanel != null) rulesPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        vibrationsEnabled = PlayerPrefs.GetInt("VibrationsEnabled", 1) == 1;

        UpdateButtonStates();

        UpdateHighscoreDisplay();
    }

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }
    public void Rules()
    {
        if (rulesPanel != null)
        {
            rulesPanel.SetActive(true);
        }
        Debug.Log("Show Rules");
    }

    public void Settings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        Debug.Log("Settings");
    }

    void UpdateButtonStates()
    {
        // Update sound button appearance
        if (soundButton != null)
        {
            ColorBlock colors = soundButton.colors;
            colors.normalColor = soundEnabled ? Color.white : Color.gray;
            colors.selectedColor = soundEnabled ? Color.white : Color.gray;
            soundButton.colors = colors;
        }

        // Update vibrations button appearance
        if (vibrationsButton != null)
        {
            ColorBlock colors = vibrationsButton.colors;
            colors.normalColor = vibrationsEnabled ? Color.white : Color.gray;
            colors.selectedColor = vibrationsEnabled ? Color.white : Color.gray;
            vibrationsButton.colors = colors;
        }
    }

    public void ToggleSound()
    {
        soundEnabled = !soundEnabled;
        PlayerPrefs.SetInt("SoundEnabled", soundEnabled ? 1 : 0);
        PlayerPrefs.Save();
        UpdateButtonStates();
        Debug.Log("Sound: " + (soundEnabled ? "ON" : "OFF"));
    }

    public void ToggleVibrations()
    {
        vibrationsEnabled = !vibrationsEnabled;
        PlayerPrefs.SetInt("VibrationsEnabled", vibrationsEnabled ? 1 : 0);
        PlayerPrefs.Save();
        UpdateButtonStates();
        Debug.Log("Vibrations: " + (vibrationsEnabled ? "ON" : "OFF"));
    }

    public void CloseRulesPanel()
    {
        if (rulesPanel != null)
        {
            rulesPanel.SetActive(false);
        }
    }

    public void CloseSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void Donate()
    {
        Debug.Log("Donate");
    }

    void UpdateHighscoreDisplay()
    {
        if (highscoreText != null)
        {
            int highscoreRound = PlayerPrefs.GetInt("HighscoreRound", 1);
            int highscoreY = PlayerPrefs.GetInt("HighscoreY", 0);
            int totalHighscore = PlayerPrefs.GetInt("Highscore", 0);

            if (totalHighscore > 0)
            {
                highscoreText.text = $"Highscore: R{highscoreRound}, {highscoreY}";
            }
            else
            {
                highscoreText.text = "Highscore: - )";  // Show dash if no games played yet
            }
        }
    }

    public void ResetHighscore()
    {
        // Reset all highscore data
        PlayerPrefs.SetInt("Highscore", 0);
        PlayerPrefs.SetInt("HighscoreRound", 1);
        PlayerPrefs.SetInt("HighscoreY", 0);
        PlayerPrefs.Save();

        UpdateHighscoreDisplay();
        Debug.Log("Highscore has been reset!");
    }

    public void Exit()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
        // For Unity Editor testing (this line only runs in editor)
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
