using System.Collections;
using UnityEngine;

public class PlayerUIController : MonoBehaviour
{
    [SerializeField] private GameObject uiRoot;
    [SerializeField] private float showDuration = 0.5f;
    [SerializeField] private float cooldown = 0.5f;

    private Coroutine showRoutine;
    private float lastShowTime = -999f;

    public void Show()
    {
        if (Time.time < lastShowTime + cooldown)
            return;

        lastShowTime = Time.time;

        if (showRoutine != null)
            StopCoroutine(showRoutine);

        showRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        if (uiRoot != null)
            uiRoot.SetActive(true);

        yield return new WaitForSeconds(showDuration);

        if (uiRoot != null)
            uiRoot.SetActive(false);

        showRoutine = null;
    }
}