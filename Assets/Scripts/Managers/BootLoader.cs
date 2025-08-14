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
        if (ThemeManager.Instance == null)
        {
            if (themeManagerPrefab != null)
            {
                var tm = Instantiate(themeManagerPrefab);
                DontDestroyOnLoad(tm.gameObject);
            }
            else
            {
                // Create a minimal ThemeManager if not provided
                var go = new GameObject("ThemeManager");
                go.AddComponent<ThemeManager>();
                DontDestroyOnLoad(go);
            }
        }
    }

    void Start()
    {
        // Move to ThemeSelect as the next step of the flow
        SceneManager.LoadScene("ThemeSelect");
    }
}
