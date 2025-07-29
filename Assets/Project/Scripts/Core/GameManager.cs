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
        if (gridManager == null || shapeManager == null || currentLevelData == null)
        {
            Debug.LogError("GameManager'a gerekli yönetici veya seviye verisi atanmamış!");
            return;
        }
        gridManager.GenerateGrid(currentLevelData);
        shapeManager.PrepareInitialShapes(currentLevelData);
    }
}