using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ThemeSelectManager : MonoBehaviour
{
    [Header("Auto-Build UI (optional)")]
    public bool autoBuildFromThemes = true;
    public Transform gridParent; // e.g., a GridLayoutGroup content transform
    public Button themeButtonPrefab; // prefab with Image + Text (TMP optional)

    [Header("Advance Settings")]
    [Tooltip("If true, show a Next arrow in gameplay to skip to the next layout.")]
    public bool showNextArrow = true;
    [Tooltip("If > 0, auto-advance after X seconds on level completion.")]
    public float autoAdvanceSeconds = 3f;

    void Start()
    {
        if (autoBuildFromThemes)
            BuildButtonsFromThemes();
    }

    public void BuildButtonsFromThemes()
    {
        if (ThemeManager.Instance == null || gridParent == null || themeButtonPrefab == null) return;
        var themes = ThemeManager.Instance.themes;
        if (themes == null) return;

        // Clear existing children
        for (int i = gridParent.childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);

        for (int i = 0; i < themes.Length; i++)
        {
            int index = i; // capture
            var btn = Instantiate(themeButtonPrefab, gridParent);
            var img = btn.GetComponent<Image>();
            if (img != null && themes[i] != null && themes[i].themeIcon != null)
                img.sprite = themes[i].themeIcon;

            // Optional label
            var label = btn.GetComponentInChildren<Text>();
            if (label != null && themes[i] != null)
                label.text = string.IsNullOrEmpty(themes[i].themeName) ? $"Theme {i+1}" : themes[i].themeName;

            btn.onClick.AddListener(() => SelectTheme(index));
        }
    }

    // Called by auto-built buttons or manually wired buttons
    public void SelectTheme(int themeIndex)
    {
        if (ThemeManager.Instance != null)
        {
            ThemeManager.Instance.ApplyTheme(themeIndex);
        }

        // Optionally pass advance settings to GameManager via a bootstrap object or static config
        // For simplicity, we load the first gameplay scene; GameManager can read PlayerPrefs for timing.
        PlayerPrefs.SetFloat("AutoAdvanceSeconds", autoAdvanceSeconds);
        PlayerPrefs.Save();

        SceneManager.LoadScene("Level_01");
    }
}
