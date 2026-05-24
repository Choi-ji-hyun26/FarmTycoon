using UnityEngine;

public class SimpleRotateVisual : MonoBehaviour
{
    [SerializeField] private Vector3 localAxis = Vector3.forward;
    [SerializeField] private float rotateSpeed = 720f;

    private void Update()
    {
        transform.Rotate(localAxis, rotateSpeed * Time.deltaTime, Space.Self);
    }
}