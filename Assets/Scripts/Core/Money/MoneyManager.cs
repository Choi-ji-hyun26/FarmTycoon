using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text moneyText;

    public static MoneyManager Instance { get; private set; }

    private int currentMoney = 0;

    public int CurrentMoney => currentMoney;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddMoney(int amount)
    {
        if(amount <= 0)
            return;

        currentMoney += amount;
        UpdateUI();
    }
    
    // public int SpendMoneyUpTo(int amount)
    // {
    //     if(amount <= 0)
    //         return 0;

    //     int paid = Mathf.Min(currentMoney, amount); // 더 작은 값을 지불하도록
    //     currentMoney -= paid;
    //     UpdateUI();

    //     return paid;
    // }

    public bool TrySpendMoney(int amount) // amount : actualPay = Mathf.Min(playerMoney, remainingCost);
    {
        if (amount <= 0)
            return false;

        if (currentMoney < amount)
            return false;

        currentMoney -= amount;
        UpdateUI();
        return true;
    }

    private void UpdateUI()
    {
        moneyText.text = currentMoney.ToString();
    }

    public int GetMoney() => currentMoney;
}