using System.Collections;
using UnityEngine;
/*
역할
1. 특정 존/오브젝트 처음 공개할 때 카메라 연출 실행
2. 카메라를 포커스 포인트로 이동
3. 대상 오브젝트 활성화
4. 플레이어쪽으로 복귀
5. 연출 중복 실행 방지
6. 단순 포커스 연출도 재사용 가능
*/
public class ZoneRevealDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private PlayerInputReader playerInputReader;

    [Header("Timing")]
    [SerializeField] private float moveDuration = 1.5f;
    [SerializeField] private float holdBeforeZoneOpen = 0.5f;
    [SerializeField] private float holdAfterZoneOpen = 2.0f;

    private bool isPlaying = false;
    public bool IsPlaying => isPlaying;

    // 특정 오브젝트 공개하는 연출 시작
    public void PlayReveal(GameObject revealObject, Transform focusPoint)
    {
        if(isPlaying)
            return;
        if(revealObject == null || focusPoint == null)
            return;

        StartCoroutine(PlayRevealSequence(revealObject, focusPoint));
    }
    
    // 외부에서 yield return 으로 기다릴 수 있는 포커스 전용 루틴
    public IEnumerator PlayFocusOnlyRoutine(Transform focusPoint)
    {
        if (isPlaying)
            yield break;

        if (focusPoint == null)
            yield break;

        yield return PlayFocusOnlySequence(focusPoint);
    }

    // 카메라 연출 전체 순서 제어
    private IEnumerator PlayRevealSequence(GameObject revealObject, Transform focusPoint) 
    {
        // 연출 시작 — 입력 차단
        playerInputReader?.SetInputBlocked(true);

        isPlaying = true;

        if(cameraFollow != null)
            cameraFollow.PauseFollow(); // 기존 카메라 추적 중지

        // 포커스 위치로 이동
        if(mainCamera != null)
            yield return MoveCamera(mainCamera.transform, focusPoint.position, moveDuration);
        
        // 공개 직전 잠깐 대기
        if(holdBeforeZoneOpen > 0f)
            yield return new WaitForSeconds(holdBeforeZoneOpen);

        // 대상 오브젝트 활성화
        revealObject.SetActive(true);
        
        // 공개 상태 보여주기
        if(holdAfterZoneOpen > 0f)
            yield return new WaitForSeconds(holdAfterZoneOpen);

        yield return ReturnToPlayer();
        
        isPlaying = false;

        // 연출 끝 — 입력 해제
        playerInputReader?.SetInputBlocked(false);
    } 
    // 포커스 전용 연출
    private IEnumerator PlayFocusOnlySequence(Transform focusPoint)
    {
        // 연출 시작 — 입력 차단
        playerInputReader?.SetInputBlocked(true);

        isPlaying = true;

        if (cameraFollow != null)
            cameraFollow.PauseFollow();

        // 감옥 쪽으로 카메라 이동
        if (mainCamera != null)
            yield return MoveCamera(mainCamera.transform, focusPoint.position, moveDuration);

        // 확장된 상태를 잠깐 보여줌
        if (holdAfterZoneOpen > 0f)
            yield return new WaitForSeconds(holdAfterZoneOpen);

        // 플레이어로 복귀
        yield return ReturnToPlayer();

        isPlaying = false;

        // 연출 끝 — 입력 해제
        playerInputReader?.SetInputBlocked(false);
    }


    // 카메라 복귀 + follow 활성화
    private IEnumerator ReturnToPlayer()
    {
        if (cameraFollow != null && playerTarget != null)
        {
            cameraFollow.SetTarget(playerTarget);

            if (mainCamera != null)
            {
                Vector3 returnPosition = cameraFollow.GetDesiredPosition();
                yield return MoveCamera(mainCamera.transform, returnPosition, moveDuration);
                mainCamera.transform.position = returnPosition;
            }
        }

        if (cameraFollow != null)
            cameraFollow.ResumeFollow();
    }
    private IEnumerator MoveCamera(Transform cameraTransform, Vector3 targetPosition, float duration)
    {
        if(cameraTransform == null)
            yield break;

        if(duration <= 0f)
        {
            cameraTransform.position = targetPosition;
            yield break;
        }

        Vector3 startPos = cameraTransform.position;
        float elapsed = 0f;

        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            cameraTransform.position = Vector3.Lerp(startPos, targetPosition, t);
            yield return null;
        }
        cameraTransform.position = targetPosition;
    }

}
