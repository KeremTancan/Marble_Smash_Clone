using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Yöneticiler")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ShapeManager shapeManager;
    [SerializeField] private PowerUpManager powerUpManager;

    [Header("Güçlendirme Verileri")]
    [SerializeField] private PowerUpData_SO refreshPowerUpData;
    [SerializeField] private PowerUpData_SO fireworkPowerUpData;

    [Header("Oyun İçi UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Slider scoreSlider;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private TextMeshProUGUI levelText; 
    
    [Header("Oyun Sonu Panelleri")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject levelFailedPanel;

    [Header("Yenileme Butonu Elemanları")]
    [SerializeField] private Button refreshShapesButton;
    [SerializeField] private GameObject refreshCountParent;
    [SerializeField] private TextMeshProUGUI refreshCountText;
    [SerializeField] private GameObject refreshCostParent;
    [SerializeField] private TextMeshProUGUI refreshCostText;
    [SerializeField] private GameObject refreshLockParent;
    [SerializeField] private TextMeshProUGUI refreshLockText;

    [Header("Roket Butonu Elemanları")]
    [SerializeField] private Button fireworkButton;
    [SerializeField] private GameObject fireworkCountParent;
    [SerializeField] private TextMeshProUGUI fireworkCountText;
    [SerializeField] private GameObject fireworkCostParent;
    [SerializeField] private TextMeshProUGUI fireworkCostText;
    [SerializeField] private GameObject fireworkLockParent;
    [SerializeField] private TextMeshProUGUI fireworkLockText;
    
    [Header("Roket Modu UI")]
    [SerializeField] private Button cancelFireworkButton;
    [SerializeField] private GameObject fireworkModePanel;

    [Header("Oyun Sonu Butonları")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button restartButton;

    private int _currentCoins;
    private int _currentLevel;

    private void OnEnable()
    {
        EventManager.OnLevelStarted += HandleLevelStart;
        EventManager.OnCurrencyUpdated += UpdateCurrencyText;
        EventManager.OnPowerUpCountChanged += OnPowerUpCountChanged;
        EventManager.OnLevelCompleted += ShowLevelCompletePanel;
        EventManager.OnLevelFailed += ShowLevelFailedPanel;
        EventManager.OnFireworkModeChanged += ToggleFireworkPanel;
        EventManager.OnScoreUpdated += UpdateScoreUI;
    }

    private void OnDisable()
    {
        EventManager.OnLevelStarted -= HandleLevelStart;
        EventManager.OnCurrencyUpdated -= UpdateCurrencyText;
        EventManager.OnPowerUpCountChanged -= OnPowerUpCountChanged;
        EventManager.OnLevelCompleted -= ShowLevelCompletePanel;
        EventManager.OnLevelFailed -= ShowLevelFailedPanel;
        EventManager.OnFireworkModeChanged -= ToggleFireworkPanel;
        EventManager.OnScoreUpdated -= UpdateScoreUI;
    }

    void Start()
    {
        if (refreshShapesButton != null) refreshShapesButton.onClick.AddListener(OnRefreshShapesClicked);
        if (fireworkButton != null) fireworkButton.onClick.AddListener(OnFireworkButtonClicked);
        if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (cancelFireworkButton != null) cancelFireworkButton.onClick.AddListener(OnCancelFireworkClicked);
    }

    private void HandleLevelStart(int levelID)
    {
        _currentLevel = levelID;
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (levelFailedPanel != null) levelFailedPanel.SetActive(false);
        if (fireworkModePanel != null) fireworkModePanel.SetActive(false);
        if (levelText != null)
        {
            levelText.text = $"LEVEL {levelID}";
        }
        UpdateAllPowerUpButtons();
    }
    
    private void OnPowerUpCountChanged(string powerUpID, int newCount) => UpdateAllPowerUpButtons();
    private void UpdateCurrencyText(int newAmount)
    {
        _currentCoins = newAmount;
        if(currencyText != null) currencyText.text = newAmount.ToString();
        UpdateAllPowerUpButtons();
    }
    
    private void UpdateAllPowerUpButtons()
    {
        UpdateButtonState(refreshPowerUpData, refreshShapesButton, refreshLockParent, refreshLockText, refreshCountParent, refreshCountText, refreshCostParent, refreshCostText);
        UpdateButtonState(fireworkPowerUpData, fireworkButton, fireworkLockParent, fireworkLockText, fireworkCountParent, fireworkCountText, fireworkCostParent, fireworkCostText);
    }
    
    private void UpdateButtonState(PowerUpData_SO data, Button button, GameObject lockParent, TextMeshProUGUI lockText, GameObject countParent, TextMeshProUGUI countText, GameObject costParent, TextMeshProUGUI costText)
    {
        if (data == null || button == null) return;

        if (_currentLevel < data.UnlockLevel)
        {
            lockParent.SetActive(true);
            countParent.SetActive(false);
            costParent.SetActive(false);
            button.interactable = false;
            if (lockText != null) lockText.text = $"LV {data.UnlockLevel}";
            return;
        }
        
        lockParent.SetActive(false);
        int count = powerUpManager.GetPowerUpCount(data.PowerUpID);

        if (count > 0)
        {
            countParent.SetActive(true);
            costParent.SetActive(false);
            if (countText != null) countText.text = count.ToString();
            button.interactable = true;
        }
        else
        {
            countParent.SetActive(false);
            costParent.SetActive(true);
            if (costText != null) costText.text = data.Cost.ToString();
            button.interactable = (_currentCoins >= data.Cost);
        }
    }

    private void OnRefreshShapesClicked()
    {
        if (powerUpManager.TryUsePowerUp(refreshPowerUpData))
        {
            shapeManager.RefreshShapeQueue();
        }
    }

    private void OnFireworkButtonClicked()
    {
        powerUpManager.ActivateFireworkMode();
    }
    
    private void OnNextLevelClicked() => gameManager.LoadNextLevel();
    private void OnRestartClicked() => gameManager.RestartLevel();
    private void OnCancelFireworkClicked() => powerUpManager.DeactivateFireworkMode();
    private void ToggleFireworkPanel(bool isActive) => fireworkModePanel?.SetActive(isActive);
    private void UpdateScoreUI(int currentScore, int goal)
    {
        int displayScore = Mathf.Min(currentScore, goal);
        if (scoreText != null)
        {
            scoreText.text = $"{displayScore} / {goal}";
        }
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