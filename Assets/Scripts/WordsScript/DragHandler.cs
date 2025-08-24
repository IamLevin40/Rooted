using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private WordBuildingScript wordBuilding;
    private string affixText;
    private bool isDropZone;
    private bool isDragging = false;
    private bool wasOriginallyPrefix;

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
            HandleDropZoneDrag(eventData);
        }
        else
        {
            wordBuilding.StartDrag(gameObject, affixText, eventData.position);
        }
    }

    private void HandleDropZoneDrag(PointerEventData eventData)
    {
        var text = GetComponentInChildren<Text>();
        if (text != null && !string.IsNullOrEmpty(text.text))
        {
            affixText = text.text;
            wasOriginallyPrefix = wordBuilding.prefixTile == gameObject;
            wordBuilding.StartDrag(gameObject, affixText, eventData.position);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (wordBuilding != null && isDragging)
            wordBuilding.UpdateDrag(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (wordBuilding == null || !isDragging) return;
        
        isDragging = false;
        var (wasDropped, isPrefix) = DetectDropZone(eventData);
        
        if (isDropZone && !wasDropped)
            wordBuilding.RemoveAffix(gameObject);
        
        wordBuilding.EndDrag(wasDropped, isPrefix, affixText, isDropZone, wasOriginallyPrefix);
    }

    private (bool wasDropped, bool isPrefix) DetectDropZone(PointerEventData eventData)
    {
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        foreach (var result in results)
        {
            var dropZone = result.gameObject.GetComponent<DropZone>();
            if (dropZone != null)
                return (true, dropZone.IsPrefix());
        }
        
        return (false, false);
    }
}
