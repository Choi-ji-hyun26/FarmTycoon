using System.Collections.Generic;
using UnityEngine;

/*
역할
1. 현재 도구 스펙에 따라 주변 당근 탐색
2. 가장 가까운 당근부터 최대 n개까지 동시 수확
3. 실제 도구 · 비주얼 관리는 담당하지 않음
*/
public class PlayerFarmer : MonoBehaviour
{
    private struct CarrotCandidate
    {
        public CarrotNode carrot;
        public Vector3 point;

        public CarrotCandidate(CarrotNode carrot, Vector3 point)
        {
            this.carrot = carrot;
            this.point = point;
        }
    }

    [Header("References")]
    [SerializeField] private PlayerFarmingToolController farmingToolController;
    [SerializeField] private PlayerUIController maxUIController;
    [SerializeField] private PlayerInventory inventory;

    [Header("Harvesting")]
    [SerializeField] private int hitDamage = 3;
    [SerializeField] private float harvestInterval = 0.3f;
    [SerializeField] private LayerMask carrotLayer;

    [Header("Targeting")]
    [SerializeField] private Transform harvestingPoint;
    [SerializeField] private float closeRange = 0.6f;
    [SerializeField, Range(-1f, 1f)] private float forwardDotThreshold = 0.4f; // Sickle만 사용

    [Header("Rake Box Settings")]
    [SerializeField] private Vector3 rakeHalfExtents = new Vector3(0.5f, 0.5f, 2.0f); // 전방으로 긴 박스
    [SerializeField] private float rakeBoxOffset = 1.5f; // 박스 중심을 전방으로 얼마나 밀지

    [Header("Vehicle Box Settings")]
    [SerializeField] private Vector3 vehicleHalfExtents = new Vector3(2.0f, 0.5f, 2.0f); // 넓은 박스
    [SerializeField] private float vehicleBoxOffset = 1.5f;

    public FarmingToolTier CurrentToolTier => farmingToolController.CurrentTier;
    private float harvestTimer;

    public bool HasCarrotTarget { get; private set; }

    private void Update()
    {
        harvestTimer += Time.deltaTime;
        RefreshCarrotTarget();
    }

    // 주변 수확 가능 당근 탐색 — HasCarrotTarget 갱신
    private void RefreshCarrotTarget()
    {
        List<CarrotCandidate> candidateCarrots = GetCandidateCarrots();
        HasCarrotTarget = candidateCarrots.Count > 0;
    }

    // 실제 수확 실행 — PlayerZoneActionHandler.HandleZoneActions()에서 호출
    public void TryExecuteHarvest()
    {
        if (harvestTimer < harvestInterval)
            return;

        List<CarrotCandidate> candidateCarrots = GetCandidateCarrots();
        HasCarrotTarget = candidateCarrots.Count > 0;

        if (candidateCarrots.Count == 0)
            return;

        harvestTimer = 0f;

        int maxTargets = GetCurrentMaxTargets();
        int harvestCount = Mathf.Min(maxTargets, candidateCarrots.Count);

        Sfx.PlayAtPoint(SoundId.Harvest, transform.position);

        for (int i = 0; i < harvestCount; i++)
        {
            CarrotCandidate candidate = candidateCarrots[i];
            bool destroyedThisHit = candidate.carrot.Harvest(hitDamage, candidate.point);

            if (!destroyedThisHit)
                continue;

            bool added = inventory.TryAddCarrot(candidate.carrot.CarrotAmount);

            if (!added)
            {
                if (maxUIController != null)
                    maxUIController.Show();
            }
        }

        RefreshCarrotTarget();
    }

    private List<CarrotCandidate> GetCandidateCarrots()
    {
        Vector3 origin = harvestingPoint != null ? harvestingPoint.position : transform.position;
        Collider[] hits = GetHitsForCurrentTier(origin);

        List<CarrotCandidate> candidateCarrots = new List<CarrotCandidate>();
        HashSet<CarrotNode> addedCarrots = new HashSet<CarrotNode>();
        FarmingToolTier tier = farmingToolController.CurrentTier;

        foreach (Collider hit in hits)
        {
            CarrotNode carrot = hit.GetComponentInParent<CarrotNode>();
            if (carrot == null || carrot.IsDepleted)
                continue;

            if (addedCarrots.Contains(carrot))
                continue;

            Vector3 targetPoint = hit.ClosestPoint(origin);
            Vector3 toCarrot = targetPoint - origin;
            float sqrDistance = toCarrot.sqrMagnitude;

            // Sickle만 방향 필터 적용
            // Rake / Vehicle은 OverlapBox 자체가 전방 방향으로 잡히므로 불필요
            if (tier == FarmingToolTier.Sickle)
            {
                if (sqrDistance > closeRange * closeRange)
                {
                    float dot = Vector3.Dot(transform.forward, toCarrot.normalized);
                    if (dot < forwardDotThreshold)
                        continue;
                }
            }

            candidateCarrots.Add(new CarrotCandidate(carrot, targetPoint));
            addedCarrots.Add(carrot);
        }

        // 가까운 순 정렬
        candidateCarrots.Sort((a, b) =>
        {
            float distA = (a.point - origin).sqrMagnitude;
            float distB = (b.point - origin).sqrMagnitude;
            return distA.CompareTo(distB);
        });

        return candidateCarrots;
    }

    // 도구 티어에 따라 탐지 방식 선택
    // Sickle  → OverlapSphere (원형)
    // Rake    → OverlapBox (전방 긴 박스, 한 줄 수확)
    // Vehicle → OverlapBox (넓은 박스, 광역 수확)
    private Collider[] GetHitsForCurrentTier(Vector3 origin)
    {
        switch (farmingToolController.CurrentTier)
        {
            case FarmingToolTier.Rake:
                return Physics.OverlapBox(
                    origin + transform.forward * rakeBoxOffset,
                    rakeHalfExtents,
                    transform.rotation,
                    carrotLayer);

            case FarmingToolTier.Vehicle:
                return Physics.OverlapBox(
                    origin + transform.forward * vehicleBoxOffset,
                    vehicleHalfExtents,
                    transform.rotation,
                    carrotLayer);

            default: // Sickle
                return Physics.OverlapSphere(
                    origin,
                    GetCurrentHarvestRadius(),
                    carrotLayer);
        }
    }

    private float GetCurrentHarvestRadius()
    {
        if (farmingToolController == null)
            return 1.2f;

        return farmingToolController.CurrentHarvestRadius;
    }

    private int GetCurrentMaxTargets()
    {
        if (farmingToolController == null)
            return 1;

        return farmingToolController.CurrentMaxTargets;
    }

    private void OnDrawGizmosSelected()
    {
        if (farmingToolController == null) return;

        Vector3 origin = harvestingPoint != null ? harvestingPoint.position : transform.position;

        switch (farmingToolController.CurrentTier)
        {
            case FarmingToolTier.Rake:
                Gizmos.color = Color.cyan;
                Gizmos.matrix = Matrix4x4.TRS(
                    origin + transform.forward * rakeBoxOffset,
                    transform.rotation,
                    Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, rakeHalfExtents * 2f);
                Gizmos.matrix = Matrix4x4.identity;
                break;

            case FarmingToolTier.Vehicle:
                Gizmos.color = Color.green;
                Gizmos.matrix = Matrix4x4.TRS(
                    origin + transform.forward * vehicleBoxOffset,
                    transform.rotation,
                    Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, vehicleHalfExtents * 2f);
                Gizmos.matrix = Matrix4x4.identity;
                break;

            default: // Sickle
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(origin, farmingToolController.CurrentHarvestRadius);
                break;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + transform.forward * 1.0f);
    }
}
