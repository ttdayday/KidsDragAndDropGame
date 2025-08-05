using UnityEngine;
using UnityEngine.UI;

public class ShapeSlot : MonoBehaviour
{
    [Header("Slot Settings")]
    public ShapeType acceptedShapeType;
    public bool isOccupied = false;

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

        slotImage.color = normalColor;
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

        // Visual feedback
        slotImage.color = correctColor;

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