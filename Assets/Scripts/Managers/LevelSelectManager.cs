using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelButton
    {
        public Button button;
        public string levelName;
        public Sprite levelPreview;
        public int buildIndex;
    }

    public LevelButton[] levelButtons;

    void Start()
    {
        SetupLevelButtons();
    }

    void SetupLevelButtons()
    {
        foreach (var levelBtn in levelButtons)
        {
            // Store the build index in a local variable for the closure
            int index = levelBtn.buildIndex;
            
            // Set up button click
            levelBtn.button.onClick.AddListener(() => LoadLevel(index));
            
            // Set preview image if available
            Image buttonImage = levelBtn.button.GetComponent<Image>();
            if (buttonImage != null && levelBtn.levelPreview != null)
            {
                buttonImage.sprite = levelBtn.levelPreview;
            }
        }
    }

    void LoadLevel(int buildIndex)
    {
        SceneManager.LoadScene(buildIndex);
    }
}
