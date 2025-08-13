using UnityEngine;

[CreateAssetMenu(menuName = "Theme/ThemeData", fileName = "NewThemeData")]
public class ThemeData : ScriptableObject
{
    public string themeName;
    // Sprites for shape slots (index corresponds to ShapeType enum order)
    public Sprite[] shapeSprites;
    // Optional separate sprites for the slot visuals (index aligns with shapeSprites)
    public Sprite[] slotSprites;
}
