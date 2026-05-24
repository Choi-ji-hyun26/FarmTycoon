using UnityEngine;

public class AudioFollowTarget : MonoBehaviour
{
    private Transform target;

    public void Initialize(Transform followTarget)
    {
        target = followTarget;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        transform.position = target.position;
    }
}