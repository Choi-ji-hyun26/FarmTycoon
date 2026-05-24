using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum CustomerRequestType
{
    Soup,
    Milk
}

public class CustomerController : MonoBehaviour
{
    private enum CustomerState
    {
        None,
        MovingToExit
    }

    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Requirement")]
    [SerializeField] private int requiredProducts = 3;
    [SerializeField] private int rewardPerProducts = 10;

    [Header("Exit")]
    [SerializeField] private Transform exitPoint;

    private int currentReceived = 0;
    private int rewardAmount = 0;

    [Header("Move")]
    [SerializeField] private float moveSpeed = 2.5f;

    [Header("UI")]
    [SerializeField] private TMP_Text remainingProductText;
    [SerializeField] private GameObject requirementUIRoot;
    [SerializeField] private Image requestTypeImage;

    [Header("Request Type Sprites")]
    [SerializeField] private Sprite soupSprite;
    [SerializeField] private Sprite milkSprite;

    private bool isMovingToWaitingSlot = false;
    private Transform waitingSlotTarget;

    private CustomerState customerState = CustomerState.None;

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

    public int RequiredProducts => requiredProducts;
    public int RemainingProducts => requiredProducts - currentReceived;
    public bool IsCompleted => RemainingProducts <= 0;
    public bool IsMovingToWaitingSlot => isMovingToWaitingSlot;
    public int RewardAmount => rewardAmount;

    private CustomerRequestType requestType;
    public CustomerRequestType RequestType => requestType;

    public System.Action<CustomerController> OnCompleted;

    private void Update()
    {
        if (customerState == CustomerState.MovingToExit)
        {
            if (MoveToTarget(exitPoint))
                Destroy(gameObject);
        }
        else if (isMovingToWaitingSlot)
        {
            if (MoveToTarget(waitingSlotTarget))
                isMovingToWaitingSlot = false;
        }

        UpdateAnimation();
    }

    public void Initialize(int amount, CustomerRequestType type = CustomerRequestType.Soup)
    {
        requiredProducts = amount;
        currentReceived = 0;
        rewardAmount = requiredProducts * rewardPerProducts;
        requestType = type;

        isMovingToWaitingSlot = false;
        waitingSlotTarget = null;
        customerState = CustomerState.None;

        if (requirementUIRoot != null)
            requirementUIRoot.SetActive(false);

        UpdateRequestTypeImage();
        UpdateRequirementView();
    }

    public bool TryReceiveProduct(int amount = 1)
    {
        if (IsCompleted)
            return false;

        currentReceived += amount;

        if (currentReceived >= requiredProducts)
        {
            currentReceived = requiredProducts;
            UpdateRequirementView();
            OnRequirementCompleted();
            return true;
        }

        UpdateRequirementView();
        return true;
    }

    private void OnRequirementCompleted()
    {
        customerState = CustomerState.MovingToExit;
        OnCompleted?.Invoke(this);

        // 서빙 완료 카운트 누적
        if (CustomerServedCounter.Instance != null)
            CustomerServedCounter.Instance.NotifyServed();
    }

    public void SetWaitingSlot(Transform slot)
    {
        if (slot == null)
            return;

        waitingSlotTarget = slot;
        isMovingToWaitingSlot = true;
    }

    public void SetRequirementUIVisible(bool visible)
    {
        if (requirementUIRoot != null && !IsCompleted)
            requirementUIRoot.SetActive(visible);
    }

    // 이동 + 도착 여부 반환 — 도착 후 처리는 호출부가 직접 책임
    private bool MoveToTarget(Transform target)
    {
        if (target == null)
            return false;

        Vector3 targetPos = target.position;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
            transform.forward = dir.normalized;

        if (Vector3.Distance(transform.position, targetPos) <= 0.05f)
        {
            transform.position = targetPos;
            return true;
        }

        return false;
    }

    private void UpdateRequestTypeImage()
    {
        if (requestTypeImage == null) return;

        requestTypeImage.sprite = requestType == CustomerRequestType.Soup
            ? soupSprite
            : milkSprite;
    }

    private void UpdateRequirementView()
    {
        if (remainingProductText != null)
            remainingProductText.text = RemainingProducts.ToString();

        if (requirementUIRoot != null)
            requirementUIRoot.SetActive(!IsCompleted);
    }

    private void UpdateAnimation()
    {
        if (animator == null)
            return;

        bool isMoving =
            customerState != CustomerState.None ||
            isMovingToWaitingSlot;

        animator.SetBool(IsMovingHash, isMoving);
    }
}
