using System;
using UnityEngine;
using static Player;


public class PlayerExp : MonoBehaviour
{
    [Header("EXP")]
    [SerializeField] private int currentExp;
    [SerializeField] private int expToLevelUp = 5;
    [SerializeField] private int level = 1;
    [SerializeField] private Player Player;
    public event Action OnStateGameUpgrade;
    private void Awake()
    {
        if(Player == null)
            Player = GetComponent<Player>();
    }
    private void Start()
    {
        // init UI
        UIManager.instance.SettingExpUI(currentExp, expToLevelUp);
    }

    public void AddExp(int value)
    {
        currentExp += value;

        if (currentExp >= expToLevelUp)
        {
            LevelUp();
        }

        UIManager.instance.SettingExpUI(currentExp, expToLevelUp);
    }
    public void LevelUpByTreasure()
    {
        Time.timeScale = 0f;
        Player.Heal(100); // hồi máu khi lên cấp
        OnStateGameUpgrade?.Invoke();
        UIManager.instance.SettingExpUI(currentExp, expToLevelUp);
    }
    private void LevelUp()
    {
        currentExp -= expToLevelUp;
        level++;
        expToLevelUp += 5;
        Player.Heal(100); // hồi máu khi lên cấp
        Debug.Log("LEVEL UP! " + level);
        Time.timeScale = 0f;
        OnStateGameUpgrade?.Invoke();
    }
}
