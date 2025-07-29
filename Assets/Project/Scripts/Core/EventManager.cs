using System;

public static class EventManager
{
    // Bir şekil ızgaraya başarıyla yerleştirildiğinde bu event tetiklenir.
    // Artık hangi slotun boşaldığını bildirmeye gerek yok.
    public static event Action OnShapePlaced;

    public static void RaiseOnShapePlaced()
    {
        OnShapePlaced?.Invoke();
    }
}