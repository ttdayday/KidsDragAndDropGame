using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class ResponsiveCanvasScaler : MonoBehaviour
{
    private CanvasScaler scaler;
    private float defaultResolutionWidth = 1920f;
    private float defaultResolutionHeight = 1080f;
    private float targetAspectRatio = 16f / 9f;

    void Start()
    {
        scaler = GetComponent<CanvasScaler>();
        UpdateCanvasScaling();
    }

    void UpdateCanvasScaling()
    {
        float screenAspect = (float)Screen.width / Screen.height;

        if (screenAspect >= targetAspectRatio)
        {
            // Screen is wider than target - use height as reference
            float scaleFactor = Screen.height / defaultResolutionHeight;
            scaler.referenceResolution = new Vector2(defaultResolutionWidth, defaultResolutionHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f; // Match height
        }
        else
        {
            // Screen is taller than target - use width as reference
            float scaleFactor = Screen.width / defaultResolutionWidth;
            scaler.referenceResolution = new Vector2(defaultResolutionWidth, defaultResolutionHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0f; // Match width
        }
    }
}
