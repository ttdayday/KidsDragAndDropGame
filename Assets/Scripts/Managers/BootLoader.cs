using UnityEngine;
using UnityEngine.SceneManagement;

// Put this on an empty GameObject in the Boot scene.
public class BootLoader : MonoBehaviour
{
    [Tooltip("Optional: If ThemeManager is not present in Boot, create one at runtime.")]
    public ThemeManager themeManagerPrefab; // leave null if you placed ThemeManager in Boot

    void Awake()
    {
        // Ensure ThemeManager exists and persists
        // 1) Prefer an existing ThemeManager placed in the Boot scene (keeps your configured themes)
        var existing = FindObjectOfType<ThemeManager>();
        if (existing != null)
        {
            DontDestroyOnLoad(existing.gameObject);
            return;
        }

        // 2) If no existing one, instantiate a prefab if assigned
        if (themeManagerPrefab != null)
        {
            var tm = Instantiate(themeManagerPrefab);
            DontDestroyOnLoad(tm.gameObject);
            return;
        }

        // 3) Fallback: create a minimal ThemeManager (empty themes)
        var go = new GameObject("ThemeManager");
        go.AddComponent<ThemeManager>();
        DontDestroyOnLoad(go);
    }

    void Start()
    {
        // Move to ThemeSelect as the next step of the flow
        SceneManager.LoadScene("ThemeSelect");
    }
}
