using System.Collections;
using UnityEngine;

/*
역할
1. 게임 클리어 조건 달성 시 외부에서 호출받음
2. 카메라 연출 후 게임 종료 UI 표시
연결 Object : GameClearContorller
*/
public class GameClearController : MonoBehaviour
{
    [Header("Camera Direction")]
    [SerializeField] private ZoneRevealDirector zoneRevealDirector;
    [SerializeField] private Transform focusPoint;  // 우리 포커스 포인트

    [Header("UI")]
    [SerializeField] private GameEndUIController gameEndUIController;

    [Header("Timing")]
    [SerializeField] private float completeDelay = 0.3f;

    private bool hasTriggered;

    public void TriggerGameClear()
    {
        if(hasTriggered) return;

        hasTriggered = true;
        StartCoroutine(GameClearRoutine());
    }

    private IEnumerator GameClearRoutine(){
        if(completeDelay > 0f)
            yield return new WaitForSeconds(completeDelay);
        
        if(zoneRevealDirector != null && focusPoint != null)
            yield return zoneRevealDirector.PlayFocusOnlyRoutine(focusPoint);

        if(gameEndUIController != null)
        {
            gameEndUIController.ShowClearUI();
        }
    }
}
