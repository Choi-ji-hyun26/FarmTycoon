using TMPro;
using UnityEngine;
/*
역할 : 존 연출 전용
*/
public class ZoneCostVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer baseRenderer;
    [SerializeField] private SpriteRenderer iconRenderer;
    [SerializeField] private TMP_Text costText;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color completedColor = Color.green;

    public void SetCost(int currentPaid, int requiredCost)
    {
        int remaining = Mathf.Max(0, requiredCost - currentPaid);
        costText.text = remaining.ToString();
    }

    public void PlayCompletedVisual()
    {
        if (baseRenderer != null) baseRenderer.color = completedColor;
        if (iconRenderer != null) iconRenderer.color = completedColor;
        if (costText != null) costText.color = completedColor;
    }
}