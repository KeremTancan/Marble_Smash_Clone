using UnityEngine;
using System.Collections.Generic;

/// Tek bir seviyenin tüm yapılandırma verilerini içerir.

[CreateAssetMenu(fileName = "Level_00", menuName = "Marble Smash/Level Data")]
public class LevelData_SO : ScriptableObject
{
    [Header("Seviye Bilgileri")]
    public int LevelID;

    [Header("Izgara Ayarları")]
    public Vector2Int GridDimensions = new Vector2Int(8, 10);
    
    [Tooltip("Bu koordinatlardaki noktalar ızgarada oluşturulmayacak.")]
    public List<Vector2Int> DisabledNodes;

    [Header("Oyun Kuralları")]
    public int ExplosionGoal = 20;

    [Tooltip("Bu seviyede ortaya çıkabilecek mermerlerin renk paleti.")]
    public ColorPalette_SO AvailableColors;
    
    [Header("Ödüller")]
    public int Reward = 50;
}