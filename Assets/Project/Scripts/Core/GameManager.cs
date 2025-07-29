using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Yöneticiler")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private ShapeManager shapeManager;

    [Header("Seviye Verisi")]
    [SerializeField] private LevelData_SO currentLevelData;

    void Start()
    {
        StartLevel();
    }

    public void StartLevel()
    {
        // Gerekli yöneticilerin ve verilerin atanıp atanmadığını kontrol et
        if (gridManager == null || shapeManager == null || currentLevelData == null)
        {
            Debug.LogError("GameManager'a gerekli yönetici veya seviye verisi atanmamış!");
            return;
        }

        // Diğer yöneticileri, seçilen seviye verisiyle başlat
        gridManager.GenerateGrid(currentLevelData);
        shapeManager.PrepareInitialShapes(currentLevelData);
    }
}