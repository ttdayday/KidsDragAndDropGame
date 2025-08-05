using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableShape : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Shape Settings")]
    public ShapeType shapeType;
    public bool isPlaced = false;

    private Vector3 startPosition;
    private Transform startParent;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    [Header("Audio")]
    public AudioClip pickupSound;
    public AudioClip dropSound;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // Add CanvasGroup if it doesn't exist
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        startPosition = transform.position;
        startParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        startPosition = transform.position;
        startParent = transform.parent;

        // Make semi-transparent while dragging
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;

        // Move to top of UI hierarchy
        transform.SetAsLastSibling();

        // Play pickup sound
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, Camera.main.transform.position, 0.5f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        // Follow the mouse/finger
        rectTransform.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Check if dropped on a valid slot
        bool wasPlaced = false;

        // Raycast to find what we're over
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            var slot = result.gameObject.GetComponent<ShapeSlot>();
            if (slot != null && slot.CanAcceptShape(this))
            {
                slot.PlaceShape(this);
                wasPlaced = true;
                break;
            }
        }

        // If not placed correctly, return to start
        if (!wasPlaced)
        {
            transform.position = startPosition;
            transform.SetParent(startParent);
        }

        // Play drop sound
        if (dropSound != null)
            AudioSource.PlayClipAtPoint(dropSound, Camera.main.transform.position, 0.5f);
    }

    public void ResetPosition()
    {
        isPlaced = false;
        transform.position = startPosition;
        transform.SetParent(startParent);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }
}