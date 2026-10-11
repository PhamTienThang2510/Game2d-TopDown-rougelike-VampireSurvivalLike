using UnityEngine;

public interface IEvent { }

public struct OnHealthChangedEvent : IEvent
{
    public int CurrentHealth;
    public int MaxHealth;

    public OnHealthChangedEvent(int currentHealth, int maxHealth)
    {
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
    }
}

public struct OnExpChangedEvent : IEvent
{
    public int CurrentExp;
    public int ExpToLevelUp;
    public int Level;

    public OnExpChangedEvent(int currentExp, int expToLevelUp, int level)
    {
        CurrentExp = currentExp;
        ExpToLevelUp = expToLevelUp;
        Level = level;
    }
}

public struct OnLevelUpEvent : IEvent { }

public struct OnGameOverEvent : IEvent { }

public struct OnPlayerDeathEvent : IEvent { }

public struct OnPlayerMovementStateEvent : IEvent
{
    public bool IsWalking;

    public OnPlayerMovementStateEvent(bool isWalking)
    {
        IsWalking = isWalking;
    }
}

public struct OnEnemyKilledEvent : IEvent
{
    public Vector2 Position;

    public OnEnemyKilledEvent(Vector2 position)
    {
        Position = position;
    }
}

public struct OnWaveChangedEvent : IEvent
{
    public int WaveIndex;
    public bool IsBossWave;

    public OnWaveChangedEvent(int waveIndex, bool isBossWave)
    {
        WaveIndex = waveIndex;
        IsBossWave = isBossWave;
    }
}