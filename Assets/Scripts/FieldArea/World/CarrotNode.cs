using UnityEngine;
using System.Collections;

/*
역할
1. 수확 HP 관리
2. farmer가 당근을 선점(claim)할 수 있도록 지원
3. HP가 0이 되면 비활성화
4. 일정 시간 후 respawn
*/
public class CarrotNode : MonoBehaviour
{
    [Header("Harvesting")]
    [SerializeField] private int maxHp = 3;
    [SerializeField] private int carrotAmount = 1;

    [Header("Respawn")]
    [SerializeField] private float respawnTime = 3f;
    [SerializeField] private bool useRespawn = true;

    [Header("References")]
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private Collider harvestCollider;

    // 현재 HP
    private int currentHp;

    // 현재 파괴 상태 여부
    private bool isDepleted;

    // 리스폰 코루틴 참조
    private Coroutine respawnCoroutine;

    // 현재 이 당근을 점유하고 있는 farmer
    private FarmerWorker currentFarmer;

    public bool IsDepleted => isDepleted;
    public int CarrotAmount => carrotAmount;

    // 시작 시 현재 HP를 최대 HP로 초기화
    private void Awake()
    {
        currentHp = maxHp;
    }


    // 이 당근을 지정한 farmer가 점유할 수 있는지 확인
    // 이미 다른 farmer가 점유 중이면 false
    public bool CanBeClaimedBy(FarmerWorker farmer)
    {
        if (isDepleted)
            return false;

        return currentFarmer == null || currentFarmer == farmer;
    }

    // 당근 점유를 시도
    // 성공하면 해당 farmer가 현재 소유자가 됨
    public bool TryClaim(FarmerWorker farmer)
    {
        if (!CanBeClaimedBy(farmer))
            return false;

        currentFarmer = farmer;
        return true;
    }

    // 현재 farmerRk 이 당근을 점유하고 있다면 claim을 해제
    public void ReleaseClaim(FarmerWorker farmer)
    {
        if (currentFarmer == farmer)
            currentFarmer = null;
    }

    // 수확 데미지를 적용
    // HP가 0 이하면 당근을 파괴하고 true를 반환
    public bool TakeHit(int damage)
    {
        if (isDepleted)
            return false;

        currentHp -= damage;

        if (currentHp <= 0)
        {
            Deplete();
            return true;
        }

        return false;
    }

    // 당근을 파괴 상태로 전환
    // 시각 오브젝트와 collider를 끄고 필요 시 respawn을 시작
    private void Deplete()
    {
        isDepleted = true;
        currentFarmer = null;

        if (visualRoot != null)
            visualRoot.SetActive(false);

        if (harvestCollider != null)
            harvestCollider.enabled = false;

        if (!useRespawn)
            return;

        if (respawnCoroutine != null)
            StopCoroutine(respawnCoroutine);

        respawnCoroutine = StartCoroutine(RespawnRoutine());
    }

    // respawnTime만큼 대기한 뒤 당근을 다시 생성
    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnTime);
        Respawn();
    }

    // 당근을 원래 상태로 복구
    // HP, 시각, collider, claim 상태를 초기화
    private void Respawn()
    {
        currentHp = maxHp;
        isDepleted = false;
        currentFarmer = null;

        if (visualRoot != null)
            visualRoot.SetActive(true);

        if (harvestCollider != null)
            harvestCollider.enabled = true;

        respawnCoroutine = null;
    }
}