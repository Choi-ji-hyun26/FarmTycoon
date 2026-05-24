using UnityEngine;

/*
역할
1. 돈 획득 / 지출 포물선 이펙트 관리
2. ItemVisualEffectPool을 통해 코인 오브젝트 재사용
3. PlayerZoneActionHandler에서 호출
*/
public class MoneyFlyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ItemVisualEffectPool effectPool;

    [Header("Arc Settings")]
    [SerializeField] private float flyDuration = 0.4f;
    [SerializeField] private float arcHeight = 1.5f;

    // 돈 획득 시: money zone -> 플레이어 머리
    public void PlayGain(Vector3 from, Vector3 to)
    {
        effectPool.PlayMoveAnimation(from, to, flyDuration, arcHeight, onArrived: null);
    }

    // 돈 지출 시: 플레이어 머리 -> zone
    public void PlaySpend(Vector3 from, Vector3 to)
    {
        effectPool.PlayMoveAnimation(from, to, flyDuration, arcHeight, onArrived: null);
    }
}
