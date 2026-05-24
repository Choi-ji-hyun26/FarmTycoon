using UnityEngine;

public class VehicleMountController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private VehicleMovement vehicleMovement;
    [SerializeField] private InputDispatcher dispatcher;
    [SerializeField] private float vehicleGroundOffset = 3f; // 바퀴 높이만큼 인스펙터에서 조정

    private bool isMounted;

    public bool IsMounted => isMounted;

    // 탑승 조건 판단 후 Mount / Dismount 수행
    // isHarvesting && toolTier == Vehicle 일 때만 탑승, 이외에는 하차
    public void Tick(bool isHarvesting, FarmingToolTier toolTier)
    {
        if (isHarvesting && toolTier == FarmingToolTier.Vehicle)
            Mount();
        else
            Dismount();
    }

    public void Mount()
    {
        if (isMounted) return;
        isMounted = true;

        Vector3 mountPos = transform.position;
        mountPos.y += vehicleGroundOffset;
        vehicleMovement.transform.position = mountPos;
        vehicleMovement.transform.rotation = playerTransform.rotation;
        vehicleMovement.gameObject.SetActive(true);

        playerMovement.SetActive(false);
        vehicleMovement.SetMountedPlayer(playerTransform);
        vehicleMovement.SetActive(true);
        dispatcher.SetTarget(vehicleMovement);
    }

    public void Dismount()
    {
        if (!isMounted) return;
        isMounted = false;

        vehicleMovement.SetMountedPlayer(null);
        vehicleMovement.SetActive(false);
        vehicleMovement.gameObject.SetActive(false);

        playerMovement.SetActive(true);
        dispatcher.SetTarget(playerMovement);
    }
}