using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Drop zone component for the warrant locker. Allows warrants to be dropped
/// back from sockets into the locker (free swapping).
/// </summary>
public class WarrantLockerDropZone : MonoBehaviour, IDropHandler
{
    [Header("References")]
    [SerializeField] private WarrantLockerGrid lockerGrid;
    
    public void OnDrop(PointerEventData eventData)
    {
        var payload = GetPayload(eventData);
        if (payload == null || string.IsNullOrEmpty(payload.WarrantId))
            return;
        
        // If payload is a WarrantSocketView, we're dropping from socket back to locker
        // In this case, clear the socket assignment (free swapping - warrant returns to locker)
        if (payload is WarrantSocketView socketView)
        {
            var removedId = socketView.ClearAssignmentForLockerReturn();
            if (!string.IsNullOrEmpty(removedId))
            {
                lockerGrid?.ReturnWarrantToLocker(removedId);
            }
            return;
        }
        
        // If payload is already from locker, no changes required.
    }
    
    private static IWarrantDragPayload GetPayload(PointerEventData eventData)
    {
        if (eventData == null || eventData.pointerDrag == null)
            return null;
        
        return eventData.pointerDrag.GetComponent<IWarrantDragPayload>();
    }
}

