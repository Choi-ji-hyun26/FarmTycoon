using System;
using UnityEngine;

public class PlayerZoneDetector : MonoBehaviour
{
    [SerializeField] private LayerMask zoneLayer;

    public Zone CurrentZone { get; private set; }
    public Zone PreviousZone { get; private set; }

    public event Action<Zone> OnZoneEntered;
    public event Action<Zone> OnZoneExited;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsInLayerMask(other.gameObject.layer, zoneLayer))
            return;

        Zone zone = other.GetComponent<Zone>();
        if (zone == null)
            return;

        if (zone == CurrentZone)
            return;

        PreviousZone = CurrentZone;

        if (PreviousZone != null)
            OnZoneExited?.Invoke(PreviousZone);

        CurrentZone = zone;
        OnZoneEntered?.Invoke(CurrentZone);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsInLayerMask(other.gameObject.layer, zoneLayer))
            return;

        Zone zone = other.GetComponent<Zone>();
        if (zone == null)
            return;

        if (zone != CurrentZone)
            return;

        PreviousZone = CurrentZone;
        CurrentZone = null;

        OnZoneExited?.Invoke(zone);
    }

    private bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}