using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance { get; private set; }

    [Header("Themes")]
    public ThemeData[] themes;
    public int selectedThemeIndex = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ApplyTheme(int themeIndex)
    {
        if (themeIndex < 0 || themeIndex >= themes.Length) return;
        selectedThemeIndex = themeIndex;

        // Find all shapes and slots and update visuals
    var shapes = FindObjectsOfType<DraggableShape>();
    var slots = FindObjectsOfType<ShapeSlot>();

        for (int i = 0; i < shapes.Length; i++)
        {
            ApplySpriteToShape(shapes[i]);
        }

        for (int i = 0; i < slots.Length; i++)
        {
            ApplySpriteToSlot(slots[i]);
        }
    }

    public void ApplySpriteToShape(DraggableShape shape)
    {
        if (themes == null || themes.Length == 0) return;
        var theme = themes[selectedThemeIndex];
        int index = (int)shape.shapeType;
        if (index >= 0 && theme.shapeSprites != null && index < theme.shapeSprites.Length)
        {
            shape.SetSprite(theme.shapeSprites[index]);
        }
    }

    public void ApplySpriteToSlot(ShapeSlot slot)
    {
        if (themes == null || themes.Length == 0) return;
        var theme = themes[selectedThemeIndex];
        int index = (int)slot.acceptedShapeType;
        var image = slot.GetComponent<UnityEngine.UI.Image>();
        if (image == null) return;

        // If the slot specifies an override sprite and is not set to use theme sprite, honor that
        if (!slot.useThemeSprite && slot.overrideSprite != null)
        {
            image.sprite = slot.overrideSprite;
            return;
        }

        // Otherwise, prefer a dedicated slot sprite if provided, otherwise fall back to the shape sprite
        if (theme.slotSprites != null && index >= 0 && index < theme.slotSprites.Length && theme.slotSprites[index] != null)
        {
            image.sprite = theme.slotSprites[index];
            return;
        }

        if (theme.shapeSprites != null && index >= 0 && index < theme.shapeSprites.Length)
        {
            image.sprite = theme.shapeSprites[index];
        }
    }
}
