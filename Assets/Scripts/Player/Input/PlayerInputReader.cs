using UnityEngine;

public class PlayerInputReader : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }

    [Header("PC Input")]
    [SerializeField] private bool useKeyboardInEditor = true;

    [Header("Mobile Joystick")]
    [SerializeField] private Joystick joystick;

    // 카메라 이동 이벤트 발생 시 움직임 차단
    private bool isInputBlocked = false;

    public void SetInputBlocked(bool blocked)
    {
        isInputBlocked = blocked;

        if(blocked)
            MoveInput = Vector2.zero;
    }

    private void Update()
    {
        // 입력 차단 중이면 MoveInput 갱신 안함
        if(isInputBlocked) return;

        Vector2 keyboardInput = Vector2.zero;

        if (useKeyboardInEditor)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            keyboardInput = new Vector2(x, y).normalized;
        }

        Vector2 joystickInput = Vector2.zero;
        if (joystick != null)
        {
            joystickInput = new Vector2(joystick.Horizontal, joystick.Vertical);
            joystickInput = Vector2.ClampMagnitude(joystickInput, 1f);
        }

        // 모바일 조이스틱 입력이 있으면 우선 사용
        if (joystickInput.sqrMagnitude > 0.001f)
            MoveInput = joystickInput;
        else
            MoveInput = keyboardInput;
    }
}