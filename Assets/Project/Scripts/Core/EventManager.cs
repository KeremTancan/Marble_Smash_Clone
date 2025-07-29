using System;

public static class EventManager
{
    public static event Action OnShapePlaced;

    public static void RaiseOnShapePlaced()
    {
        OnShapePlaced?.Invoke();
    }
}