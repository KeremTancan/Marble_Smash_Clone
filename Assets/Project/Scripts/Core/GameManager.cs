using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState { Playing, LevelComplete, LevelFailed }
    
    [Header("Yöneticiler")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private ShapeManager shapeManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private ConnectionManager connectionManager; 

    private GameState _currentState;

    private void OnEnable()
    {
        EventManager.OnLevelCompleted += HandleLevelComplete;
        EventManager.OnLevelFailed += HandleLevelFailed;
    }

    private void OnDisable()
    {
        EventManager.OnLevelCompleted -= HandleLevelComplete;
        EventManager.OnLevelFailed -= HandleLevelFailed;
    }

    void Start()
    {
        StartLevel();
    }

    public void StartLevel()
    {
        LevelData_SO currentLevelData = levelManager.GetCurrentLevelData();

        // Kontrol listesine connectionManager'ı da ekledik
        if (gridManager == null || shapeManager == null || scoreManager == null || inputManager == null || connectionManager == null || currentLevelData == null)
        {
            Debug.LogError("GameManager'a gerekli yönetici veya seviye verisi atanmamış!");
            return;
        }

        _currentState = GameState.Playing;
        inputManager.enabled = true;

        scoreManager.PrepareLevel(currentLevelData.ExplosionGoal);
        gridManager.GenerateGrid(currentLevelData);
        connectionManager.UpdateAllConnections();
        shapeManager.PrepareInitialShapes(currentLevelData);
        EventManager.RaiseOnLevelStarted(currentLevelData.LevelID);
    }

    private void HandleLevelComplete()
    {
        if (_currentState != GameState.Playing) return;

        _currentState = GameState.LevelComplete;
        inputManager.enabled = false;
    
        LevelData_SO currentLevelData = levelManager.GetCurrentLevelData();
        if (currentLevelData != null)
        {
            CurrencyManager.Instance.AddCoins(currentLevelData.Reward);
            EventManager.RaiseOnRewardCollected(currentLevelData.Reward);
        }
    }

    private void HandleLevelFailed()
    {
        if (_currentState != GameState.Playing) return;
        _currentState = GameState.LevelFailed;
        inputManager.enabled = false;
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        levelManager.AdvanceToNextLevel();
        RestartLevel();
    }
}