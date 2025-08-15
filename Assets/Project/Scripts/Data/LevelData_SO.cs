using UnityEngine;
using System.Collections.Generic;

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
    public List<Vector2Int> DisabledNodes;
    public List<PrePlacedShapeData> PrePlacedShapes;

    [Header("Oyun Kuralları")]
    public int ExplosionGoal = 20;

    [Tooltip("Bu listenin BOYUTU, ShapeManager'daki ana listeden kaç tane şeklin (en baştan başlayarak) kullanılabileceğini belirler. İçeriği önemli değil, sadece sayısı kullanılır.")]
    public List<ShapeData_SO> AllowedShapes;

    public ColorPalette_SO AvailableColors;
    
    [Header("Ödüller")]
    public int Reward = 50;

    [Header("Kilitli Nokta Ayarları")]
    public List<LockedNodeData> LockedNodes;
}

[System.Serializable]
public struct LockedNodeData
{
    public Vector2Int Position;
    public int MarblesToUnlock;
}