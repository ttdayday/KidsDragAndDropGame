using UnityEngine;

[CreateAssetMenu(menuName = "Theme/ThemeData", fileName = "NewThemeData")]
public class ThemeData : ScriptableObject
{
    public string themeName;
    [Tooltip("Icon shown on the Theme Select screen for this theme")]
    public Sprite themeIcon;
    // Sprites for shape slots (index corresponds to ShapeType enum order)
    public Sprite[] shapeSprites;
    // Optional separate sprites for the slot visuals (index aligns with shapeSprites)
    public Sprite[] slotSprites;
}
