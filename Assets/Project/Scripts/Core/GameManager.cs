using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Yöneticiler")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private ShapeManager shapeManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private UIManager uiManager;

    [Header("Seviye Verisi")]
    [SerializeField] private LevelData_SO currentLevelData;

    void Start()
    {
        StartLevel();
    }

    public void StartLevel()
    {
        if (gridManager == null || shapeManager == null || scoreManager == null || currentLevelData == null)
        {
            Debug.LogError("GameManager'a gerekli yönetici veya seviye verisi atanmamış!");
            return;
        }

        scoreManager.PrepareLevel(currentLevelData.ExplosionGoal); 
        gridManager.GenerateGrid(currentLevelData);
        shapeManager.PrepareInitialShapes(currentLevelData);
    }
}