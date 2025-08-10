using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class DraggableShape : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Shape Settings")]
    public ShapeType shapeType;
    public bool isPlaced = false;

    [Header("Size Multipliers")]
    public float trayScale = 1.0f;      // Size 1.0 - original editor size
    public float dragScale = 0.9f;      // Size 0.9 - when being dragged
    public float slotScale = 1.5f;      // Size 1.5 - when correctly placed in slot

    private Vector3 originalScale;      // Remember the original proportions from editor
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
        
        // Ensure shape is always on top when being dragged
        Image image = GetComponent<Image>();
        if (image != null)
        {
            image.raycastTarget = true;
        }

        // Make sure we have a Canvas Group for drag handling
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        // Add CanvasGroup if it doesn't exist
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        startPosition = transform.position;
        startParent = transform.parent;

        // Remember the original scale from editor (this becomes our "1.0" reference)
        originalScale = transform.localScale;

        // Make sure we start at tray scale (should already be correct from editor)
        transform.localScale = originalScale * trayScale;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        startPosition = transform.position;
        startParent = transform.parent;

        // Move to canvas root for proper layering
        transform.SetParent(transform.root);
        transform.SetAsLastSibling(); // This ensures it's drawn last (on top)

        // Scale to DRAG SIZE (0.8) maintaining original proportions
        transform.localScale = originalScale * dragScale;

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

        // Follow the mouse/finger (maintain drag scale)
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
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            var slot = result.gameObject.GetComponent<ShapeSlot>();
            if (slot != null && slot.CanAcceptShape(this))
            {
                slot.PlaceShape(this);
                wasPlaced = true;

                // Scale to match the slot size exactly
                ScaleToMatchSlot(slot);
                break;
            }
        }

        // If not placed correctly, return to tray and original size
        if (!wasPlaced)
        {
            transform.position = startPosition;
            transform.SetParent(startParent);
            // Return to TRAY SIZE (1.0)
            transform.localScale = originalScale * trayScale;
        }

        // Play drop sound
        if (dropSound != null)
            AudioSource.PlayClipAtPoint(dropSound, Camera.main.transform.position, 0.5f);
    }

    private void ScaleToMatchSlot(ShapeSlot slot)
    {
        // Get the slot's RectTransform
        RectTransform slotRect = slot.GetComponent<RectTransform>();

        // Get the shape's RectTransform  
        RectTransform shapeRect = GetComponent<RectTransform>();

        // Calculate scale needed to match slot size
        float scaleX = slotRect.rect.width / (shapeRect.rect.width / originalScale.x);
        float scaleY = slotRect.rect.height / (shapeRect.rect.height / originalScale.y);

        // Use the smaller scale to ensure shape fits within slot
        float finalScale = Mathf.Min(scaleX, scaleY);

        // Apply the calculated scale while maintaining proportions
        transform.localScale = originalScale * finalScale;
    }

    public void ResetPosition()
    {
        isPlaced = false;
        transform.position = startPosition;
        transform.SetParent(startParent);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Return to TRAY SIZE (1.0) when reset
        transform.localScale = originalScale * trayScale;
    }

    public void UpdateStartPosition(Vector3 newPosition)
    {
        startPosition = newPosition;
    }
}