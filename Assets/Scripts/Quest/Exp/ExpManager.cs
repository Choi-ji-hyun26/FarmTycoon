using UnityEngine;

public class ExpManager : MonoBehaviour
{
    public static ExpManager Instance { get; private set; }

    private int _totalExp;
    public int TotalExp => _totalExp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddExp(int amount)
    {
        if (amount <= 0) return;
        _totalExp += amount;
    }
}