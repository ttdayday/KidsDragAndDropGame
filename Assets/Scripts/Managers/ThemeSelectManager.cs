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
        // Best effort auto-wiring to reduce setup friction in Editor
        TryAutoWire();

        if (autoBuildFromThemes)
            BuildButtonsFromThemes();
    }

    public void BuildButtonsFromThemes()
    {
        if (ThemeManager.Instance == null)
        {
            Debug.LogWarning("ThemeSelectManager: No ThemeManager instance found. Make sure you enter through the 'Boot' scene or place a ThemeManager in the scene.", this);
            return;
        }
        if (gridParent == null)
        {
            Debug.LogWarning("ThemeSelectManager: 'gridParent' is not assigned. Assign a Transform with a GridLayoutGroup (e.g., your ThemeGrid content).", this);
            return;
        }
        if (themeButtonPrefab == null)
        {
            Debug.LogWarning("ThemeSelectManager: 'themeButtonPrefab' is not assigned. Create a Button prefab (with Image + Text/TMP) and assign it.", this);
            return;
        }
        var themes = ThemeManager.Instance.themes;
        if (themes == null || themes.Length == 0)
        {
            Debug.LogWarning("ThemeSelectManager: No themes configured. Open the Boot scene and either (1) place a ThemeManager and assign its 'themes' array with ThemeData assets, or (2) assign a ThemeManager prefab to BootLoader.", this);
            return;
        }

        // Clear existing children
        for (int i = gridParent.childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);

        for (int i = 0; i < themes.Length; i++)
        {
            int index = i; // capture
            var btn = Instantiate(themeButtonPrefab, gridParent);
            // Prefer a child named "Icon" for the art; fallback to the Button's own Image
            Image img = null;
            var iconTf = btn.transform.Find("Icon");
            if (iconTf != null) img = iconTf.GetComponent<Image>();
            if (img == null) img = btn.GetComponent<Image>();
            if (img != null && themes[i] != null && themes[i].themeIcon != null)
            {
                img.sprite = themes[i].themeIcon;
                img.preserveAspect = true;
            }

            // Optional label
            var label = btn.GetComponentInChildren<Text>();
            if (label != null && themes[i] != null)
            {
                label.text = string.IsNullOrEmpty(themes[i].themeName) ? $"Theme {i+1}" : themes[i].themeName;
            }
            else
            {
                // Try TextMeshPro if present
                var tmp = btn.GetComponentInChildren<TMPro.TMP_Text>();
                if (tmp != null && themes[i] != null)
                    tmp.text = string.IsNullOrEmpty(themes[i].themeName) ? $"Theme {i+1}" : themes[i].themeName;
            }

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

    // Try to auto-assign common references to avoid silent no-op
    void TryAutoWire()
    {
        if (gridParent == null)
        {
            var grid = FindObjectOfType<UnityEngine.UI.GridLayoutGroup>();
            if (grid != null) gridParent = grid.transform;
        }
        // We don't auto-guess the prefab to avoid instantiating the wrong thing; logs in BuildButtonsFromThemes will guide setup.
    }
}
