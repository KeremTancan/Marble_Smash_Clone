using System;

public static class EventManager
{
    public static event Action OnTurnCompleted; 
    public static event Action<int> OnMarblesExploded;
    public static event Action<int, int> OnScoreUpdated;
    public static event Action OnLevelCompleted;
    public static event Action OnLevelFailed;
    public static event Action<bool> OnFireworkModeChanged;
    public static event Action<int> OnCurrencyUpdated; 
    public static event Action<int> OnLevelStarted; 
    public static event Action<string, int> OnPowerUpCountChanged;

    public static void RaiseOnCurrencyUpdated(int newAmount) { OnCurrencyUpdated?.Invoke(newAmount); }
    public static void RaiseOnTurnCompleted() { OnTurnCompleted?.Invoke(); }
    public static void RaiseOnMarblesExploded(int amount) { OnMarblesExploded?.Invoke(amount); }
    public static void RaiseOnScoreUpdated(int score, int goal) { OnScoreUpdated?.Invoke(score, goal); }
    public static void RaiseOnLevelCompleted() { OnLevelCompleted?.Invoke(); }
    public static void RaiseOnLevelFailed() { OnLevelFailed?.Invoke(); }
    public static void RaiseOnFireworkModeChanged(bool isActive) {OnFireworkModeChanged?.Invoke(isActive); }
    public static void RaiseOnLevelStarted(int levelID) { OnLevelStarted?.Invoke(levelID); }
    public static void RaiseOnPowerUpCountChanged(string powerUpID, int newCount) { OnPowerUpCountChanged?.Invoke(powerUpID, newCount); }
}