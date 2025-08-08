using UnityEngine;
using System.Collections.Generic;

/// Tek bir seviyenin tüm yapılandırma verilerini içerir.

[System.Serializable] public struct LockedNodeData
{
    public Vector2Int Position;
    public int MarblesToUnlock;
}

[System.Serializable]
public struct PrePlacedShapeData
{
    public ShapeData_SO ShapeData; 
    public Vector2Int AnchorPosition; 
}

[CreateAssetMenu(fileName = "Level_00", menuName = "Marble Smash/Level Data")]
public class LevelData_SO : ScriptableObject
{
    [Header("Seviye Bilgileri")]
    public int LevelID;

    [Header("Izgara Ayarları")]
    public Vector2Int GridDimensions = new Vector2Int(8, 10);
    
    [Tooltip("Bu koordinatlardaki noktalar ızgarada oluşturulmayacak.")]
    public List<Vector2Int> DisabledNodes;
    
    [Header("Kilitli Nokta Ayarları")]
    public List<LockedNodeData> LockedNodes;
    
    [Tooltip("Seviye başında ızgarada hazır olarak gelecek şekiller.")]
    public List<PrePlacedShapeData> PrePlacedShapes;

    [Header("Oyun Kuralları")]
    public int ExplosionGoal = 20;

    [Tooltip("Bu seviyede ortaya çıkabilecek mermerlerin renk paleti.")]
    public ColorPalette_SO AvailableColors;
    
    [Header("Ödüller")]
    public int Reward = 50;
}