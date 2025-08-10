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
    public Text levelText;
    public ParticleSystem celebrationParticles;

    [Header("Audio")]
    public AudioClip levelCompleteSound;
    public AudioSource backgroundMusic;

    [Header("Level Management")]
    public int currentLevel = 1;
    public int totalLevels = 10;

    private bool puzzleComplete = false;

    void Start()
    {
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
            resetButton.onClick.AddListener(ResetLevel);

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
            winPanel.SetActive(true);

        // Save progress (you'll implement this later)
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
        // Load next scene
        SceneManager.LoadScene(currentScene + 1);
    }
}

