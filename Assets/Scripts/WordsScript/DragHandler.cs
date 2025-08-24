using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private WordBuildingScript wordBuilding;
    private string affixText;
    private bool isDropZone;
    private bool isDragging = false;

    public void Initialize(WordBuildingScript wordBuildingScript, string text, bool dropZone)
    {
        wordBuilding = wordBuildingScript;
        affixText = text;
        isDropZone = dropZone;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (wordBuilding == null) return;
        
        isDragging = true;
        
        if (isDropZone)
        {
            // If dragging from prefix/suffix tile, get the current text
            Text text = GetComponentInChildren<Text>();
            if (text != null && !string.IsNullOrEmpty(text.text))
            {
                affixText = text.text;
                wordBuilding.StartDrag(gameObject, affixText, eventData.position);
            }
        }
        else
        {
            // Dragging from affix tile
            wordBuilding.StartDrag(gameObject, affixText, eventData.position);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (wordBuilding == null || !isDragging) return;
        
        wordBuilding.UpdateDrag(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (wordBuilding == null || !isDragging) return;
        
        isDragging = false;
        
        // Check if dropped on a valid drop zone
        bool wasDropped = false;
        bool isPrefix = false;
        
        // Raycast to see what we're over
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        foreach (var result in results)
        {
            DropZone dropZone = result.gameObject.GetComponent<DropZone>();
            if (dropZone != null)
            {
                wasDropped = true;
                isPrefix = dropZone.IsPrefix();
                break;
            }
        }
        
        // If dragging from a drop zone and not dropped on another drop zone, remove it
        if (isDropZone && !wasDropped)
        {
            // Remove from current position
            if (gameObject.name.Contains("prefix") || transform.parent.name.Contains("prefix"))
            {
                wordBuilding.RemovePrefix();
            }
            else if (gameObject.name.Contains("suffix") || transform.parent.name.Contains("suffix"))
            {
                wordBuilding.RemoveSuffix();
            }
        }
        
        wordBuilding.EndDrag(wasDropped, isPrefix, affixText);
    }
}
