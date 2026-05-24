using UnityEngine;

/*
역할
1. 외부에서 TriggerTransition() 호출을 받음
2. 기존 오브젝트를 비활성화하고 다음 오브젝트들을 즉시 활성화
3. 1회 실행 방지
*/
public class UpgradeStepTransitionTrigger : MonoBehaviour
{
    [Header("Disable Targets")]
    [SerializeField] private GameObject[] disableTargets;

    [Header("Enable Targets")]
    [SerializeField] private GameObject[] enableTargets;

    private bool hasTriggered;

    private void Awake()
    {
        if (enableTargets != null)
        {
            for (int i = 0; i < enableTargets.Length; i++)
            {
                if (enableTargets[i] != null)
                {
                    enableTargets[i].SetActive(false);
                }
            }
        }
    }

    public void TriggerTransition()
    {
        if (hasTriggered)
            return;

        hasTriggered = true;

        if (disableTargets != null)
        {
            for (int i = 0; i < disableTargets.Length; i++)
            {
                if (disableTargets[i] != null)
                {
                    disableTargets[i].SetActive(false);
                }
            }
        }

        if (enableTargets != null)
        {
            for (int i = 0; i < enableTargets.Length; i++)
            {
                if (enableTargets[i] != null)
                {
                    enableTargets[i].SetActive(true);
                }
            }
        }
        Sfx.Play(SoundId.ZoneUnlock);
    }
}