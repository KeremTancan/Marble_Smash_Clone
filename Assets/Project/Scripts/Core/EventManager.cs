using System;

public static class EventManager
{
    // OnShapePlaced'in adını OnTurnCompleted olarak değiştiriyoruz.
    public static event Action OnTurnCompleted; 
    
    public static event Action<int> OnMarblesExploded;
    public static event Action<int, int> OnScoreUpdated;
    public static event Action OnLevelCompleted;
    public static event Action OnLevelFailed;

    // RaiseOnShapePlaced'in adını RaiseOnTurnCompleted olarak değiştiriyoruz.
    public static void RaiseOnTurnCompleted() 
    { 
        OnTurnCompleted?.Invoke(); 
    }
    
    public static void RaiseOnMarblesExploded(int amount) { OnMarblesExploded?.Invoke(amount); }
    public static void RaiseOnScoreUpdated(int score, int goal) { OnScoreUpdated?.Invoke(score, goal); }
    public static void RaiseOnLevelCompleted() { OnLevelCompleted?.Invoke(); }
    public static void RaiseOnLevelFailed() { OnLevelFailed?.Invoke(); }
}