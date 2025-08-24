using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private WordBuildingScript wordBuilding;
    private string affixText;
    private bool isDropZone;
    private bool isDragging = false;
    private bool wasOriginallyPrefix; // Track if this tile was originally in prefix position

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
            var text = GetComponentInChildren<Text>();
            if (text != null && !string.IsNullOrEmpty(text.text))
            {
                affixText = text.text;
                
                // Determine if this is originally a prefix or suffix tile
                wasOriginallyPrefix = wordBuilding.prefixTile == gameObject;
                
                wordBuilding.StartDrag(gameObject, affixText, eventData.position);
            }
        }
        else
        {
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
        
        bool wasDropped = false;
        bool isPrefix = false;
        
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        foreach (var result in results)
        {
            var dropZone = result.gameObject.GetComponent<DropZone>();
            if (dropZone != null)
            {
                wasDropped = true;
                isPrefix = dropZone.IsPrefix();
                break;
            }
        }
        
        if (isDropZone && !wasDropped)
        {
            wordBuilding.RemoveAffix(gameObject);
        }
        
        // Pass information about the original position to EndDrag
        wordBuilding.EndDrag(wasDropped, isPrefix, affixText, isDropZone, wasOriginallyPrefix);
    }
}
