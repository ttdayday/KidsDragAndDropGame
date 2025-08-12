using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
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

    [Header("Audio")]
    public AudioClip levelCompleteSound;
    public AudioSource backgroundMusic;

    [Header("Level Management")]
    public int currentLevel = 1;
    public int totalLevels = 2;  // Updated to match actual number of levels

    private bool puzzleComplete = false;

    void Start()
    {
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

        // Find all slots and shapes if not assigned
        if (allSlots.Length == 0)
            allSlots = FindObjectsOfType<ShapeSlot>();
        if (allShapes.Length == 0)
            allShapes = FindObjectsOfType<DraggableShape>();

        // Setup UI
        if (levelText != null)
            levelText.text = "Level " + currentLevel;

        if (winPanel != null)
            winPanel.SetActive(false);

        // Setup buttons
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(LoadNextLevel);
        if (resetButton != null)
            resetButton.onClick.AddListener(RestartLevel);
        if (menuButton != null)
            menuButton.onClick.AddListener(ReturnToLevelSelect);

        puzzleComplete = false;
    }

    public void CheckPuzzleComplete()
    {
        if (puzzleComplete) return;

        // Check if all slots are filled
        foreach (var slot in allSlots)
        {
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
        yield return new WaitForSeconds(1f);

        // Show win panel
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            
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
        // Get current scene index
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        
        // If there's a next level, load it
        if (currentScene < SceneManager.sceneCountInBuildSettings - 1)
        {
            SceneManager.LoadScene(currentScene + 1);
        }
        else
        {
            // If it's the last level, go back to level select
            SceneManager.LoadScene("LevelSelect");
        }
    }

    public void RestartLevel()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToLevelSelect()
    {
        // Can be called from pause button or back button
        SceneManager.LoadScene("LevelSelect");
    }
}

