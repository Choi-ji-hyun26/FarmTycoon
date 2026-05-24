using System.Linq.Expressions;
using UnityEngine;
/*
역할
1. 실제 카메라 추적만 담당
*/
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 10f, -8f);
    [SerializeField] private float followSpeed = 8f;

    private bool isFollowing = true;

    private void LateUpdate()
    {
        if(!isFollowing) return;
        if (target == null) return;

        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, followSpeed * Time.deltaTime);
    }

    // 플레이어 -> 감옥 등 전환할 떄 사용
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void PauseFollow()
    {
        isFollowing = false;
    }

    public void ResumeFollow()
    {
        isFollowing = true;
    }

    public Vector3 GetDesiredPosition()
    {
        if (target == null)
            return transform.position;

        return target.position + offset;
    }

    public void SnapToPosition(Vector3 position)
    {
        transform.position = position;
    }
}