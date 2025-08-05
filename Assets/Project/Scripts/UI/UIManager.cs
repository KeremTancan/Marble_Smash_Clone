using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Yöneticiler")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ShapeManager shapeManager;
    [SerializeField] private PowerUpManager powerUpManager; 

    [Header("Oyun İçi UI Elemanları")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Slider scoreSlider;
    
    [Header("Oyun Sonu Panelleri")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject levelFailedPanel;

    [Header("Butonlar")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button refreshShapesButton;
    [SerializeField] private Button fireworkButton; 
    [SerializeField] private Button cancelFireworkButton;

    [Header("Power-Up Paneli")]
    [SerializeField] private GameObject fireworkModePanel; 

    private void OnEnable()
    {
        EventManager.OnScoreUpdated += UpdateScoreUI;
        EventManager.OnLevelCompleted += ShowLevelCompletePanel;
        EventManager.OnLevelFailed += ShowLevelFailedPanel;
        EventManager.OnFireworkModeChanged += ToggleFireworkPanel; 
    }

    private void OnDisable()
    {
        EventManager.OnScoreUpdated -= UpdateScoreUI;
        EventManager.OnLevelCompleted -= ShowLevelCompletePanel;
        EventManager.OnLevelFailed -= ShowLevelFailedPanel;
        EventManager.OnFireworkModeChanged -= ToggleFireworkPanel;
    }

    void Start()
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (levelFailedPanel != null) levelFailedPanel.SetActive(false);
        if (fireworkModePanel != null) fireworkModePanel.SetActive(false);

        if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (refreshShapesButton != null) refreshShapesButton.onClick.AddListener(OnRefreshShapesClicked);
        if (fireworkButton != null) fireworkButton.onClick.AddListener(OnFireworkButtonClicked);
        if (cancelFireworkButton != null) cancelFireworkButton.onClick.AddListener(OnCancelFireworkClicked);
    }

    private void OnFireworkButtonClicked() => powerUpManager.ActivateFireworkMode();
    private void OnCancelFireworkClicked() => powerUpManager.DeactivateFireworkMode();
    private void ToggleFireworkPanel(bool isActive)
    {
        if (fireworkModePanel != null) fireworkModePanel.SetActive(isActive);
    }

    private void OnNextLevelClicked() { gameManager.LoadNextLevel(); }
    private void OnRestartClicked() { gameManager.RestartLevel(); }
    
    private void OnRefreshShapesClicked()
    {
        if (shapeManager != null)
        {
            shapeManager.RefreshShapeQueue();
        }
    }

    private void UpdateScoreUI(int currentScore, int goal)
    {
        int displayScore = Mathf.Min(currentScore, goal);
        if (scoreText != null) scoreText.text = $"{displayScore} / {goal}";
        if (scoreSlider != null)
        {
            scoreSlider.maxValue = goal;
            scoreSlider.value = displayScore;
        }
    }

    private void ShowLevelCompletePanel()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            Canvas.ForceUpdateCanvases();
        }
    }

    private void ShowLevelFailedPanel()
    {
        if (levelFailedPanel != null)
        {
            levelFailedPanel.SetActive(true);
            Canvas.ForceUpdateCanvases();
        }
    }
}