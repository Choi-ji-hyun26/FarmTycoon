using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
역할
1. 슬롯 위치 기준 우리 안 배회
2. 일정 주기마다 생산물 수거함에 추가
3. 배회 애니메이션 상태 관리
*/
public class AnimalController : MonoBehaviour
{
    [Header("Wander")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float wanderRadius = 1.5f;
    [SerializeField] private float minWaitTime = 1f;
    [SerializeField] private float maxWaitTime = 3f;

    [Header("Production")]
    [SerializeField] private float productionInterval = 10f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

    private Vector3 homePosition;
    private Vector3 currentTarget;
    private float waitTimer;
    private float productionTimer;
    private PenCollectionBox collectionBox;
    private bool isInitialized;

    // 슬롯 위치와 수거함 참조 받아 초기화
    public void Initialize(Vector3 slotPosition, PenCollectionBox box)
    {
        homePosition = slotPosition;
        collectionBox = box;
        isInitialized = true;
        productionTimer = productionInterval;
        PickNewTarget();
    }

    private void Update()
    {
        if (!isInitialized) return;
        HandleWander();
        HandleProduction();
        UpdateAnimation();
    }

    // 홈 포지션 기준 랜덤 반경 내 배회
    // 목적지 도달 시 잠깐 대기 후 새 목적지 선택
    private void HandleWander()
    {
        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget);

        if (distance <= 0.1f)
        {
            transform.position = currentTarget;
            waitTimer = Random.Range(minWaitTime, maxWaitTime);
            PickNewTarget();
            return;
        }
    }

    // productionInterval마다 수거함에 생산물 1개 추가
    // 수거함이 가득 차면 생산 중단
    private void HandleProduction()
    {
        if (collectionBox == null) return;
        if (collectionBox.IsFull) return;

        productionTimer -= Time.deltaTime;

        if (productionTimer <= 0f)
        {
            productionTimer = productionInterval;
            collectionBox.TryAddProductWithAnimation(1, transform.position);
        }
    }

    // 대기 중이 아닐 때만 이동 애니메이션 재생
    private void UpdateAnimation()
    {
        if (animator == null) return;
        animator.SetBool(IsMovingHash, waitTimer <= 0f);
    }

    // 홈 포지션 기준 랜덤 반경 내 새 목적지 선택
    private void PickNewTarget()
    {
        Vector2 random = Random.insideUnitCircle * wanderRadius;
        currentTarget = homePosition + new Vector3(random.x, 0f, random.y);
    }
}
