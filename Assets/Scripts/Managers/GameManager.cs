using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public enum GameState { Loading, ThemeSelect, Playing, Completed }

    [Header("State")]
    public GameState currentState = GameState.Playing;
    [Header("Game Objects")]
    public ShapeSlot[] allSlots;
    public DraggableShape[] allShapes;

    [Header("UI Elements")]
    public GameObject winPanel;
    public Button nextLevelButton;
    public Button resetButton;
    public Button menuButton;
    public Text levelText;
    public ParticleSystem celebrationParticles;
    [Tooltip("Optional label to show auto-advance countdown on the win screen (TMP).")]
    public TMP_Text autoAdvanceTMPLabel;
    [Tooltip("Optional label to show auto-advance countdown on the win screen (legacy UI.Text).")]
    public Text autoAdvanceTextLabel;

    [Header("Completion FX (no text)")]
    [Tooltip("Optional: RectTransform of the side tray to slide off-screen on completion.")]
    public RectTransform sideTray;
    [Tooltip("How far to slide the tray on completion (positive X moves it right off-screen).")]
    public float traySlideOffset = 600f;
    [Tooltip("Seconds to slide the tray.")]
    public float traySlideTime = 0.35f;
    [Tooltip("Balloon prefab (UI Image with BalloonFloat). We'll instantiate a few near the right side.")]
    public GameObject balloonPrefab;
    [Tooltip("Parent transform for spawned balloons (usually the WinPanel or a top-level UI container).")]
    public RectTransform balloonParent;

    [Header("Audio")]
    public AudioClip levelCompleteSound;
    public AudioSource backgroundMusic;

    [Header("Level Management")]
    public int currentLevel = 1;
    public int totalLevels = 2;  // default; will be overridden by ThemeData.levelCount if available
    [Tooltip("If true the game will automatically advance to the next level after a delay when the puzzle is completed.")]
    public bool enableAutoAdvance = true;
    [Tooltip("Seconds to wait before auto-advancing to the next level.")]
    public float autoAdvanceDelay = 3f;
    [Tooltip("If true the player can skip the delay and go to the next level by clicking/tapping the screen after completion.")]
    public bool allowSkipByClick = true;

    private bool puzzleComplete = false;
    private Coroutine autoAdvanceCoroutine = null;

    void Start()
    {
        // Pull auto-advance config saved by ThemeSelect (if present)
        float savedAuto = PlayerPrefs.GetFloat("AutoAdvanceSeconds", -1f);
        if (savedAuto >= 0f) {
            autoAdvanceDelay = savedAuto;
        }

        // If ThemeManager has a selected theme, override totalLevels from it
        if (ThemeManager.Instance != null && ThemeManager.Instance.themes != null && ThemeManager.Instance.themes.Length > 0)
        {
            var tm = ThemeManager.Instance;
            int idx = Mathf.Clamp(tm.selectedThemeIndex, 0, tm.themes.Length - 1);
            var theme = tm.themes[idx];
            if (theme != null)
            {
                int count = theme.levelCount;
                if (count <= 0) count = 1; // fallback for older ThemeData assets
                totalLevels = count;
            }
        }

        // Set current level based on scene name
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.StartsWith("Level_"))
        {
            string levelNumber = sceneName.Substring(6); // "Level_01" -> "01"
            if (int.TryParse(levelNumber, out int level))
            {
                currentLevel = level;
            }
        }

    // Always refresh slots and shapes for each scene
    allSlots = FindObjectsOfType<ShapeSlot>();
    allShapes = FindObjectsOfType<DraggableShape>();

        // Setup UI
        // If levelText was assigned to objects from another scene, try to find a local one
        if (levelText == null)
        {
            var txt = GameObject.Find("LevelText");
            if (txt != null) levelText = txt.GetComponent<Text>();
        }
        if (levelText != null)
            levelText.text = "Level " + currentLevel;

        if (winPanel != null)
            winPanel.SetActive(false);

        // Setup buttons
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(LoadNextLevel);
        // If player clicks the next level button we should stop any auto-advance coroutine
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(StopAutoAdvance);
        if (resetButton != null)
            resetButton.onClick.AddListener(RestartLevel);
        if (menuButton != null)
            menuButton.onClick.AddListener(ReturnToLevelSelect);

        puzzleComplete = false;
    }

    // Methods for external UI to call based on the new storyboard
    public void EnterLoadingState()
    {
        currentState = GameState.Loading;
        // Optionally disable gameplay while loading
        // For now this is a lightweight hook for UI scenes
        Debug.Log("Entered Loading State");
    }

    public void EnterThemeSelectState()
    {
        currentState = GameState.ThemeSelect;
        Debug.Log("Entered Theme Select State");
    }

    public void CheckPuzzleComplete()
    {
        if (puzzleComplete) return;

        // Check if all slots are filled
        Debug.Log($"CheckPuzzleComplete: checking {allSlots.Length} slots", this);
        foreach (var slot in allSlots)
        {
            Debug.Log($"  Slot {slot.name} occupied={slot.isOccupied}", slot);
            if (!slot.isOccupied)
                return; // Puzzle not complete yet
        }

        // Puzzle is complete!
        puzzleComplete = true;
    StartCoroutine(OnPuzzleComplete());
    }

    IEnumerator OnPuzzleComplete()
    {
        // Play celebration sound
        if (levelCompleteSound != null)
            AudioSource.PlayClipAtPoint(levelCompleteSound, Camera.main.transform.position);

        // Play particles
        if (celebrationParticles != null)
            celebrationParticles.Play();

        // Wait a moment for effect
        yield return new WaitForSeconds(0.3f);

        // Slide tray off-screen (if assigned)
        if (sideTray != null)
            yield return StartCoroutine(SlideRectX(sideTray, traySlideOffset, traySlideTime));

        // Show win panel first so spawned children are visible
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            

        // Spawn a few balloons (if prefab provided)
        SpawnBalloons(4);
            // Debug logging
            Debug.Log($"Current Level: {currentLevel}, Total Levels: {totalLevels}");
            
            // Update button for last level
            if (currentLevel >= totalLevels)
            {
                Debug.Log("This is the last level, updating button text...");
                if (nextLevelButton != null)
                {
                    Debug.Log("Next level button found");
                    // Try both Text and TMPro.TMP_Text components
                    Text buttonText = nextLevelButton.GetComponentInChildren<Text>(true);
                    TMPro.TMP_Text tmpText = nextLevelButton.GetComponentInChildren<TMPro.TMP_Text>(true);
                    
                    if (buttonText != null)
                    {
                        buttonText.text = "Back to Menu";
                        Debug.Log("Updated UI.Text component to 'Back to Menu'");
                    }
                    else if (tmpText != null)
                    {
                        tmpText.text = "Back to Menu";
                        Debug.Log("Updated TMPro text component to 'Back to Menu'");
                    }
                    else
                    {
                        Debug.LogWarning("Could not find text component on button!");
                    }
                }
                else
                {
                    Debug.LogWarning("Next level button is null!");
                }
            }
            else
            {
                if (nextLevelButton != null)
                {
                    Text buttonText = nextLevelButton.GetComponentInChildren<Text>(true);
                    TMPro.TMP_Text tmpText = nextLevelButton.GetComponentInChildren<TMPro.TMP_Text>(true);
                    
                    if (buttonText != null)
                        buttonText.text = "Next Level";
                    else if (tmpText != null)
                        tmpText.text = "Next Level";
                }
            }
        }

        // Save progress
        PlayerPrefs.SetInt("UnlockedLevel", Mathf.Max(PlayerPrefs.GetInt("UnlockedLevel", 1), currentLevel + 1));
        PlayerPrefs.Save();

        // Start auto advance if enabled (and not last level -> in which case it will go back to menu)
        if (enableAutoAdvance)
        {
            UpdateAutoAdvanceLabel(autoAdvanceDelay);
            // make sure we don't start multiple coroutines
            if (autoAdvanceCoroutine != null)
                StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfterDelay());
        }
        else
        {
            UpdateAutoAdvanceLabel(0f); // hide if not used
        }
    }

    IEnumerator AutoAdvanceAfterDelay()
    {
        float remaining = autoAdvanceDelay;
        // Simple countdown loop so we could extend to show remaining time in UI later
        while (remaining > 0f)
        {
            // allow immediate skip if player clicks
            if (allowSkipByClick && Input.GetMouseButtonDown(0))
            {
                break;
            }
            UpdateAutoAdvanceLabel(remaining);
            yield return null;
            remaining -= Time.deltaTime;
        }

        autoAdvanceCoroutine = null;
        UpdateAutoAdvanceLabel(0f);
        LoadNextLevel();
    }

    void StopAutoAdvance()
    {
        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = null;
        }
    UpdateAutoAdvanceLabel(0f);
    }

    void Update()
    {
        // Allow skipping the auto-advance by clicking/tapping anywhere after completion
        if (puzzleComplete && allowSkipByClick && Input.GetMouseButtonDown(0))
        {
            StopAutoAdvance();
            LoadNextLevel();
        }
    }

    public void ResetLevel()
    {
        puzzleComplete = false;

        // Reset all shapes
        foreach (var shape in allShapes)
        {
            shape.ResetPosition();
        }

        // Reset all slots
        foreach (var slot in allSlots)
        {
            slot.RemoveShape();
        }

        // Hide win panel
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    public void LoadNextLevel()
    {
        // Decide based on theme's levelCount and currentLevel
        if (currentLevel < totalLevels)
        {
            // Load next numbered level scene, e.g., Level_01 -> Level_02
            int next = currentLevel + 1;
            string nextName = $"Level_{next:00}";
            SceneManager.LoadScene(nextName);
        }
        else
        {
            // Last level for this theme -> back to ThemeSelect
            SceneManager.LoadScene("ThemeSelect");
        }
    }

    public void LoadPreviousLevel()
    {
        int prev = Mathf.Max(1, currentLevel - 1);
        if (prev == currentLevel)
        {
            // already on first level, go back to theme select
            SceneManager.LoadScene("ThemeSelect");
            return;
        }
        string prevName = $"Level_{prev:00}";
        SceneManager.LoadScene(prevName);
    }

    public void RestartLevel()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToLevelSelect()
    {
        // Can be called from pause button or back button
        SceneManager.LoadScene("ThemeSelect");
    }

    void UpdateAutoAdvanceLabel(float seconds)
    {
        // Hide when seconds <= 0
        bool show = seconds > 0.05f;
        // For toddlers we avoid letters; keep label empty but you may show a clock icon via UI instead
        string text = string.Empty;
        if (autoAdvanceTMPLabel != null)
        {
            autoAdvanceTMPLabel.text = text;
            autoAdvanceTMPLabel.gameObject.SetActive(show);
        }
        if (autoAdvanceTextLabel != null)
        {
            autoAdvanceTextLabel.text = text;
            autoAdvanceTextLabel.gameObject.SetActive(show);
        }
    }

    // Helpers
    IEnumerator SlideRectX(RectTransform rt, float deltaX, float duration)
    {
        Vector2 start = rt.anchoredPosition;
        Vector2 target = start + new Vector2(deltaX, 0f);
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            rt.anchoredPosition = Vector2.Lerp(start, target, k);
            yield return null;
        }
        rt.anchoredPosition = target;
    }

    void SpawnBalloons(int count)
    {
        if (balloonPrefab == null)
        {
            Debug.LogWarning("GameManager: Balloon prefab not assigned—no balloons will spawn.", this);
            return;
        }
        // Fallback: if no parent set, try winPanel or the scene canvas
        RectTransform parent = balloonParent;
        if (parent == null)
        {
            if (winPanel != null) parent = winPanel.GetComponent<RectTransform>();
            if (parent == null)
            {
                var canvas = FindObjectOfType<Canvas>();
                if (canvas != null) parent = canvas.GetComponent<RectTransform>();
            }
        }
        if (parent == null) return;

        var rect = parent.rect;
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(balloonPrefab, parent);
            var rt = go.GetComponent<RectTransform>();
            float x = rect.width * 0.7f + Random.Range(-40f, 40f);
            float y = -rect.height * 0.3f + Random.Range(-40f, 40f);
            rt.anchoredPosition = new Vector2(x, y);
            go.transform.SetAsLastSibling(); // ensure on top of win visuals
        }
    }
}

