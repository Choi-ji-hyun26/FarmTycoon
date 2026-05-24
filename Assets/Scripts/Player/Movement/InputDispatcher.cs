using UnityEngine;

public class InputDispatcher : MonoBehaviour
{
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private PlayerMovement playerMovement;

    private Controllable currentTarget;

    private void Awake()
    {
        currentTarget = playerMovement;
    }

    public void SetTarget(Controllable target)
    {
        currentTarget = target;
    }

    public float GetCurrentSpeed() => currentTarget?.GetSpeed() ?? 0f;
}