using UnityEngine;
using UnityEngine.UI;

public class ShapeSlot : MonoBehaviour
{
    [Header("Slot Settings")]
    public ShapeType acceptedShapeType;
    public bool isOccupied = false;

    [Header("Theme Overrides")]
    [Tooltip("If false and overrideSprite is set, the slot will keep the override sprite instead of the Theme sprite.")]
    public bool useThemeSprite = true;
    [Tooltip("Optional per-slot sprite to use instead of the theme sprite.")]
    public Sprite overrideSprite;
    [Tooltip("Multiplier applied to shapes when they are scaled to fit this slot. Use to compensate for different art proportions.")]
    public float spriteScaleMultiplier = 1f;

    [Header("Visual Feedback")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    public Color correctColor = Color.green;

    private Image slotImage;
    private DraggableShape currentShape;

    [Header("Audio")]
    public AudioClip correctSound;
    public AudioClip wrongSound;

    void Start()
    {
        slotImage = GetComponent<Image>();
        if (slotImage == null)
            slotImage = gameObject.AddComponent<Image>();
    slotImage.preserveAspect = true;

        // Ensure slot is visible and properly initialized on scene start
        slotImage.enabled = true;
        slotImage.color = normalColor;
        isOccupied = false;
        currentShape = null;

        // Apply theme sprite if ThemeManager exists
        if (UnityEngine.Object.FindObjectOfType<ThemeManager>() != null)
        {
            ThemeManager tm = UnityEngine.Object.FindObjectOfType<ThemeManager>();
            tm.ApplySpriteToSlot(this);
        }
    }

    public bool CanAcceptShape(DraggableShape shape)
    {
        return !isOccupied && shape.shapeType == acceptedShapeType;
    }

    public void PlaceShape(DraggableShape shape)
    {
        if (!CanAcceptShape(shape))
        {
            // Play wrong sound
            if (wrongSound != null)
                AudioSource.PlayClipAtPoint(wrongSound, Camera.main.transform.position, 0.5f);
            return;
        }

        // Place the shape
        currentShape = shape;
        isOccupied = true;
        shape.isPlaced = true;

        // Move shape to slot position
        shape.transform.SetParent(transform);
        shape.transform.localPosition = Vector3.zero;

        // Hide the slot visual
        slotImage.enabled = false;

        // Play correct sound
        if (correctSound != null)
            AudioSource.PlayClipAtPoint(correctSound, Camera.main.transform.position, 0.5f);

        // Check if puzzle is complete
        FindObjectOfType<GameManager>()?.CheckPuzzleComplete();
    }

    public void RemoveShape()
    {
        if (currentShape != null)
        {
            currentShape.ResetPosition();
            currentShape = null;
        }

        isOccupied = false;
        
        // Ensure we have a valid slot image
        if (slotImage == null)
        {
            slotImage = GetComponent<Image>();
            if (slotImage == null)
                slotImage = gameObject.AddComponent<Image>();
        }
        
        slotImage.enabled = true;  // Show the slot visual again
        slotImage.color = normalColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<DraggableShape>() != null)
        {
            slotImage.color = highlightColor;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<DraggableShape>() != null && !isOccupied)
        {
            slotImage.color = normalColor;
        }
    }
}