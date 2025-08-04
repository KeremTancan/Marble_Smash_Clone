using System;

public static class EventManager
{
    public static event Action OnShapePlaced;
    
    public static event Action<int> OnMarblesExploded; 
    public static event Action<int, int> OnScoreUpdated; 
    public static event Action OnLevelCompleted; 
    
    public static void RaiseOnShapePlaced()
    {
        OnShapePlaced?.Invoke();
    }
    
    public static void RaiseOnMarblesExploded(int amount)
    {
        OnMarblesExploded?.Invoke(amount);
    }
    
    public static void RaiseOnScoreUpdated(int score, int goal)
    {
        OnScoreUpdated?.Invoke(score, goal);
    }
    
    public static void RaiseOnLevelCompleted()
    {
        OnLevelCompleted?.Invoke();
    }
}