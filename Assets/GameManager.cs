using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI numberY;
    public TextMeshProUGUI numberX;
    public TextMeshProUGUI heartsDisplay;
    public Image timerGauge;
    public GameObject retryButton;
    public GameObject pauseButton;

    [Header("Sound Effects")]
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip bonusLifeSound;
    public AudioClip gameOverSound;
    public AudioClip newHighscoreSound;
    private AudioSource audioSource;    


    private int y = 1;
    private long x;
    private int lives = 3;
    private int maxLives = 3;
    private int GlobalHighscore;
    private int currentRound = 1;
    private int maxRound = 7;

    private float timeRemaining = 10f;
    private float maxTime = 30f;
    private bool isGameActive = true;
    private bool isPaused = false;
    private float resumeInputDelay = 0f;

    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private float lastSwipeTime = 0f;
    private float swipeCooldown = 0.3f;



    void Start()
    {
        // ADD THESE 5 LINES:
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        GlobalHighscore = PlayerPrefs.GetInt("Highscore", 0);
        // ... rest of your Start code
        GlobalHighscore = PlayerPrefs.GetInt("Highscore", 0);
        if (retryButton != null)
        {
            retryButton.SetActive(false);
        }
        GenerateNewX();
        UpdateUI();
    }

    void PlaySound(AudioClip clip)
    {
        bool soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        if (soundEnabled && audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private string[] highscoreMessages = new string[]
    {
        "NEW RECORD!",
        "NEW HIGHSCORE!",
        "LIMITLESS!",
        "UNSTOPPABLE!",
        "YOU'RE ON FIRE!",
        "CRUSHING IT!",
        "GENIUS!",
        "SHARPER AND SHARPER",
        "INCREDIBLE!",
        "MIND BLOWING!",
        "YOU SURPASSED YOURSELF",
        "HALL OF FAME!"
    };

    void Update()
    {
        if (!isGameActive) return;

        timeRemaining -= Time.deltaTime;
        timerGauge.fillAmount = timeRemaining / maxTime;

        if (timeRemaining <= 5f)
        {
            timerGauge.color = Color.red;
        }
        else if (timeRemaining <= 10f)
        {
            timerGauge.color = Color.yellow;
        }
        else
        {
            timerGauge.color = Color.green;
        }

        if (timeRemaining <= 3f && timeRemaining > 0f)
        {
            float fractionalPart = timeRemaining - Mathf.Floor(timeRemaining);
            if (fractionalPart > 0.95f)
            {
                Vibrate();
            }
        }

        if (timeRemaining <= 0f)
        {
            TimeOut();
            return;
        }

        // ⚠️ ADD THIS LINE HERE - Blocks input when paused
        if (isPaused) return;

        if (resumeInputDelay > 0f)
        {
            resumeInputDelay -= Time.deltaTime;
            return;  // Block input during delay
        }

        // TOUCH INPUT - Android/iOS
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startTouchPosition = touch.position;
            }

            if (touch.phase == TouchPhase.Ended)
            {
                endTouchPosition = touch.position;
                DetectSwipe();
            }
        }

        // MOUSE INPUT - Unity Editor only (for testing)
#if UNITY_EDITOR
    if (Input.GetMouseButtonDown(0))
    {
        startTouchPosition = Input.mousePosition;
    }

    if (Input.GetMouseButtonUp(0))
    {
        endTouchPosition = Input.mousePosition;
        DetectSwipe();
    }
#endif
    }

    void DetectSwipe()
    {
        // Cooldown check - prevent double detection
        if (Time.time - lastSwipeTime < swipeCooldown)
        {
            Debug.Log("Swipe ignored - too soon!");
            return;
        }

        // Calculate swipe direction (Y axis - vertical swipe)
        float swipeDistance = endTouchPosition.y - startTouchPosition.y;

        if (Mathf.Abs(swipeDistance) > 100)
        {
            // Record this swipe time to prevent rapid fire
            lastSwipeTime = Time.time;

            Debug.Log("Swipe detected! Distance: " + swipeDistance);

            if (swipeDistance > 0)
            {
                CheckAnswer(true); // Swipe UP
            }
            else
            {
                CheckAnswer(false); // Swipe DOWN
            }
        }
    }

    void CheckAnswer(bool playerSaysMultiple)
    {
        bool isActuallyMultiple = (x % y == 0);

        if (playerSaysMultiple == isActuallyMultiple)
        {
            Debug.Log("Corect!");
            y++;

            // Check if round is complete (Y reached 30)
            if (y > 30)
            {
                if (currentRound < maxRound)
                {
                    currentRound++; // Move to next round
                    y = 1; // Reset Y to 1
                    Debug.Log($"ROUND {currentRound} START!");
                    PlaySound(bonusLifeSound); // Celebrate new round
                }
                else
                {
                    // Already on final round (Round 7) - keep going
                    // Y continues past 30 on final round
                }
            }

            if (y % 10 == 0)
            {
                // Check if we can still add hearts
                if (lives < 5)
                {
                    lives++;
                    maxLives = Mathf.Min(maxLives + 1, 5);
                    Debug.Log("BONUS! +1 viață!");
                    PlaySound(bonusLifeSound);
                }
                else
                {
                    // Already at max hearts - give bonus time instead
                    timeRemaining = Mathf.Min(timeRemaining + 10f, 30f);
                    Debug.Log("Max hearts! +10 seconds bonus time!");
                    PlaySound(bonusLifeSound);
                }
            }
            else
            {
                PlaySound(correctSound);
            }

            GenerateNewX();
            ResetTimer();
            UpdateUI();
        }
        else
        {
            Debug.Log("Greșit!");

            lives--;
            Vibrate();

            if (lives <= 0)
            {
                PlaySound(gameOverSound);
                GameOver();
            }
            else
            {
                PlaySound(wrongSound);
                GenerateNewX();

                // Bonus timp dacă Y > 10
                if (y > 10)
                {
                    timeRemaining = 30f;
                }
                else
                {
                    ResetTimer();
                }
                UpdateUI();
            }
        }
    }

    void TimeOut()
    {
        Debug.Log("Time out!");

        PlaySound(gameOverSound);

        lives--;
        Vibrate();

        if (lives <= 0)
        {
            GameOver();
        }
        else
        {
            GenerateNewX();
            // Bonus timp dacă Y > 10
            if (y > 10)
            {
                timeRemaining = 30f;
            }
            else
            {
                timeRemaining = 15f;
            }
            UpdateUI();
        }
    }

    void ResetTimer()
    {
        timeRemaining = Mathf.Min(timeRemaining + 10, 30f);
    }

    void GenerateNewX()
    {
        int minValue = y * 3;
        int maxValue = GetMaxValueForRound();

        if (Random.value < 0.5f)
        {
            // Generate a MULTIPLE of Y with progressive difficulty
            int multiplier = GetMultiplierForCurrentLevel();
            x = y * multiplier;
            if (x > maxValue) x = maxValue;
        }
        else
        {
            // Generate a NON-MULTIPLE of Y
            int attempts = 0;
            do
            {
                x = Random.Range(minValue, maxValue + 1);
                attempts++;
                if (attempts > 100)
                {
                    x = minValue + 1;
                    if (x % y == 0) x++;
                    break;
                }
            }
            while (x % y == 0);
        }

        // Even number fix
        if (y % 2 == 0 && x % 2 != 0)
        {
            x++;
            if (x % y == 0)
            {
                x += 2;
            }
            if (x > maxValue)
            {
                x = minValue + (minValue % 2);
            }
        }
    }

    int GetMaxValueForRound()
    {
        // Max value scales with round difficulty
        switch (currentRound)
        {
            case 1:
                return y * 10;  // Round 1: smaller numbers (Y=4 → max=48)

            case 2:
                return y * 14;  // Round 2: medium numbers (Y=4 → max=72)

            case 3:
                return y * 21;  // Round 3: larger numbers (Y=4 → max=100)

            case 4:
                return y * 20;  // Round 4: getting harder (Y=4 → max=120)

            case 5:
                return y * 24;  // Round 5: expert level

            case 6:
                return y * 32;  // Round 6: extreme

            case 7:
            default:
                return y * 48;  // Round 7+: insane numbers
        }
    }

    int GetMultiplierForCurrentLevel()
    {
        // Progressive difficulty based on ROUND (not Y)

        switch (currentRound)
        {
            case 1:
                return Random.Range(3, 10);   // Round 1: 3-12

            case 2:
                return Random.Range(4, 14);   // Round 2: 4-19

            case 3:
                return Random.Range(6, 21);   // Round 3: 6-29

            case 4:
                return Random.Range(11, 20);  // Round 4: 11-35

            case 5:
                return Random.Range(16, 24);  // Round 5: 16-39

            case 6:
                return Random.Range(21, 32);  // Round 6: 21-55

            case 7:
            default:
                return Random.Range(31, 48);  // Round 7+: 31-79 (final)
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Debug.Log("Game Paused - Timer continues, input blocked");
    }

    public void ResumeGame()
    {
        isPaused = false;

        resumeInputDelay = 0.2f;

        Debug.Log("Game Resumed");
    }

    void UpdateUI()
    {
        numberY.text = y.ToString();
        numberX.text = x.ToString();

        string hearts = "";
        for (int i = 0; i < lives; i++)
        {
            hearts += "♥ ";
        }
        heartsDisplay.text = hearts;
    }

    void Vibrate()
    {// Check if vibrations are enabled
        bool vibrationsEnabled = PlayerPrefs.GetInt("VibrationsEnabled", 1) == 1;

        if (vibrationsEnabled)
        {
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
            Debug.Log("Vibrate!");
        }
    }

    void GameOver()
    {
        isGameActive = false;

        numberX.text = "GAME\nOVER";

        // Calculate total score
        int totalScore = (currentRound - 1) * 30 + y;

        // Get saved highscore data
        int savedHighscoreTotal = PlayerPrefs.GetInt("Highscore", 0);
        int savedHighscoreRound = PlayerPrefs.GetInt("HighscoreRound", 1);
        int savedHighscoreY = PlayerPrefs.GetInt("HighscoreY", 0);

        // DEBUG: Print all values
        Debug.Log("=== GAME OVER DEBUG ===");
        Debug.Log($"Current Round: {currentRound}");
        Debug.Log($"Current Y: {y}");
        Debug.Log($"Total Score: {totalScore}");
        Debug.Log($"Saved Highscore Total: {savedHighscoreTotal}");
        Debug.Log($"Saved Highscore Round: {savedHighscoreRound}");
        Debug.Log($"Saved Highscore Y: {savedHighscoreY}");
        Debug.Log($"Comparison: {totalScore} >= {savedHighscoreTotal} = {totalScore >= savedHighscoreTotal}");

        // Check if this is a new highscore
        if (totalScore >= savedHighscoreTotal)
        {
            Debug.Log("✅ NEW HIGHSCORE BRANCH");
            PlaySound(newHighscoreSound);

            // Save ALL highscore data
            PlayerPrefs.SetInt("Highscore", totalScore);
            PlayerPrefs.SetInt("HighscoreRound", currentRound);
            PlayerPrefs.SetInt("HighscoreY", y);
            PlayerPrefs.Save();

            Debug.Log($"Saved: Highscore={totalScore}, Round={currentRound}, Y={y}");

            // NEW HIGHSCORE FORMAT
            string randomMessage = highscoreMessages[Random.Range(0, highscoreMessages.Length)];
            numberY.text = $"{randomMessage}\nNew Highscore\nR{currentRound}, S{y}";
        }
        else
        {
            Debug.Log("❌ DID NOT BEAT HIGHSCORE BRANCH");

            // NO NEW HIGHSCORE FORMAT
            numberY.text = $"R{currentRound}, {y}\nHighscore:\nR{savedHighscoreRound}, S{savedHighscoreY}";
        }

        // Move numberY higher to avoid overlap
        RectTransform numberYRect = numberY.GetComponent<RectTransform>();
        numberYRect.anchoredPosition = new Vector2(numberYRect.anchoredPosition.x, -130);

        timerGauge.fillAmount = 0;
        heartsDisplay.text = "";

        if (retryButton != null)
        {
            retryButton.SetActive(true);
        }

        if (pauseButton != null)
        {
            pauseButton.SetActive(false);
        }
    }


    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void RestartGame()
    {
        if (pauseButton != null) // ADD THIS LINE
        {
            pauseButton.SetActive(true); // ADD THIS LINE
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}