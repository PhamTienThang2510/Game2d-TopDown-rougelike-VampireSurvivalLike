using UnityEngine;

public class PlayerExp : MonoBehaviour
{
    [Header("EXP")]
    [SerializeField] private int currentExp;
    [SerializeField] private int expToLevelUp = 5;
    [SerializeField] private int level = 1;
    [SerializeField] private Player Player;

    private void Awake()
    {
        if(Player == null)
            Player = GetComponent<Player>();
    }
    private void Start()
    {
        // init UI
        EventBus<OnExpChangedEvent>.Publish(new OnExpChangedEvent(currentExp, expToLevelUp, level));
    }

    public void AddExp(int value)
    {
        currentExp += value;

        if (currentExp >= expToLevelUp)
        {
            LevelUp();
        }

        EventBus<OnExpChangedEvent>.Publish(new OnExpChangedEvent(currentExp, expToLevelUp, level));
    }
    public void LevelUpByTreasure()
    {
        Time.timeScale = 0f;
        Player.Heal(100); // hồi máu khi lên cấp
        EventBus<OnLevelUpEvent>.Publish();
        EventBus<OnExpChangedEvent>.Publish(new OnExpChangedEvent(currentExp, expToLevelUp, level));
    }
    private void LevelUp()
    {
        currentExp -= expToLevelUp;
        level++;
        expToLevelUp += 5;
        Player.Heal(100); // hồi máu khi lên cấp
        Debug.Log("LEVEL UP! " + level);
        Time.timeScale = 0f;
        EventBus<OnLevelUpEvent>.Publish();
    }
}
