
using UnityEngine;

/*
역할
1. 현재 활성화 도구 단계에 따라 비주얼 표시
2. 도구 비주얼 온오프 담당
*/
public class PlayerToolVisualController : MonoBehaviour
{
    [Header("Tool Visuals")]
    [SerializeField] private GameObject sickleVisual;
    [SerializeField] private GameObject rakeVisual;

    public void ShowTool(FarmingToolTier tier)
    {
        sickleVisual?.SetActive(tier == FarmingToolTier.Sickle);
        rakeVisual?.SetActive(tier == FarmingToolTier.Rake);
    }

    public void HideAll()
    {
        sickleVisual?.SetActive(false);
        rakeVisual?.SetActive(false);
    }
}