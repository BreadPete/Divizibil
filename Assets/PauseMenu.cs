using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu: MonoBehaviour
{
    [Header("UI References")]
    public GameObject PauseMenuOBJ;
    public Button Pause;
    public Button Back;
    public Button Menu;
    
    [Header("Scene Settings")]
    public string sceneToLoad = "MainMenu"; // Change this to your scene name

    private GameManager gameManager;

    void Start()
    {
        // Hide popup at start
        PauseMenuOBJ.SetActive(false);

        // ADD THIS LINE - Find GameManager
        gameManager = FindFirstObjectByType<GameManager>();

        // Add button listeners
        Pause.onClick.AddListener(OpenPopup);
        Back.onClick.AddListener(ClosePopup);
        Menu.onClick.AddListener(LoadNewScene);
    }
    
    public void OpenPopup()
    {
        PauseMenuOBJ.SetActive(true);
        //Time.timeScale = 0f; // Optional: pause the game

        
        if (gameManager != null)
        {
            gameManager.PauseGame();
        }

    }
    
    public void ClosePopup()
    {
        PauseMenuOBJ.SetActive(false);
        //Time.timeScale = 1f; // Optional: resume the game

        if (gameManager != null)
        {
            gameManager.ResumeGame();
        }
    }
    
    public void LoadNewScene()
    {
        Time.timeScale = 1f; // Resume time before loading
        SceneManager.LoadScene(sceneToLoad);
    }
    
}
