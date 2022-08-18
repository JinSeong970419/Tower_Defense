using UnityEngine;
using System.Collections;
using TMPro;

public class Wallet : MonoBehaviour
{
    const int startingGold = 100;

    [SerializeField] const float baseInterestRate = 0.05f;

    [SerializeField] float interestRate;
    [SerializeField] float interestRateFrequency = 10.0f;
    float interestRateTimer;

    [SerializeField] int currentGold;

    //GUI
    [SerializeField] TextMeshProUGUI guiGoldText;
    [SerializeField] TextMeshProUGUI guiKillText;
    int killCount = 0;

    void Start()
    {
        currentGold = startingGold;
        interestRate = baseInterestRate;
        interestRateTimer = interestRateFrequency;
        guiGoldText.text = currentGold.ToString();
    }

    void FixedUpdate()
    {
        Timer();
        if (interestRateTimer <= 0.0f)
        {
            AppendInterest();
            interestRateTimer = interestRateFrequency;
        }
    }

    private void AppendInterest()
    {
        currentGold += (int)(currentGold * interestRate);
        guiGoldText.text = currentGold.ToString();
    }
    
    // 웨이브 종료 시 골드 지급
    public void WaveCompletionReward(int fixedSum)
    {
        currentGold += fixedSum;
        guiGoldText.text = currentGold.ToString();

    }

    // 적 처치 시 골드 추가
    public void EnemyKillReward(int fixedSum)
    {
        killCount++;
        currentGold += fixedSum;
        guiGoldText.text = currentGold.ToString();
        guiKillText.text = killCount.ToString();
    }

    // 돈 사용가능 여부 확인
    public bool CheckMoney(int fixedSum)
    {
        if (currentGold >= fixedSum) return true;
        else return false;
    }

    // 골드 삭제
    public void SpendMoney(int fixedSum)
    {
        currentGold -= fixedSum;
        guiGoldText.text = currentGold.ToString();

    }

    // 재설정
    public void ResetWallet()
    {
        currentGold = startingGold;
        guiGoldText.text = currentGold.ToString();
    }

    public void AddMoney(int amount)
    {
        currentGold += amount;
        guiGoldText.text = currentGold.ToString();
    }

    public void Timer() { interestRateTimer -= Time.deltaTime; }
}
