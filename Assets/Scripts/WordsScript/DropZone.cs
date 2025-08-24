using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    private bool isPrefix;

    public void Initialize(WordBuildingScript wordBuildingScript, bool prefixZone) => isPrefix = prefixZone;

    public void OnDrop(PointerEventData eventData)
    {
        // This is handled by the DragHandler's OnEndDrag method
        // We just need this component to be detected by raycast
    }

    public bool IsPrefix() => isPrefix;
}
