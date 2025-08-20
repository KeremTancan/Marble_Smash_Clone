using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Yöneticiler")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ShapeManager shapeManager;
    [SerializeField] private PowerUpManager powerUpManager;
    [SerializeField] private InputManager inputManager;

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
    [SerializeField] private TextMeshProUGUI rewardText; 

    [Header("Butonlar")]
    [SerializeField] private Button refreshShapesButton;
    [SerializeField] private Button fireworkButton;
    [SerializeField] private Button cancelFireworkButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button restartButtonS;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private Button soundToggleButton;
    [SerializeField] private Button vibrationToggleButton;

    [Header("Diğer UI Elemanları")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject refreshCountParent;
    [SerializeField] private TextMeshProUGUI refreshCountText;
    [SerializeField] private GameObject refreshCostParent;
    [SerializeField] private TextMeshProUGUI refreshCostText;
    [SerializeField] private GameObject refreshLockParent;
    [SerializeField] private TextMeshProUGUI refreshLockText;
    [SerializeField] private GameObject fireworkCountParent;
    [SerializeField] private TextMeshProUGUI fireworkCountText;
    [SerializeField] private GameObject fireworkCostParent;
    [SerializeField] private TextMeshProUGUI fireworkCostText;
    [SerializeField] private GameObject fireworkLockParent;
    [SerializeField] private TextMeshProUGUI fireworkLockText;
    [SerializeField] private GameObject fireworkModePanel;
    [SerializeField] private Sprite toggleOnSprite;
    [SerializeField] private Sprite toggleOffSprite;

    private int _currentCoins;
    private int _currentLevel;
    private bool _isCooldownActive = false;
    private readonly WaitForSeconds _buttonCooldown = new WaitForSeconds(0.1f);
    private void OnDisable()
    {
        EventManager.OnLevelStarted -= HandleLevelStart;
        EventManager.OnCurrencyUpdated -= UpdateCurrencyText;
        EventManager.OnPowerUpCountChanged -= OnPowerUpCountChanged;
        EventManager.OnLevelCompleted -= ShowLevelCompletePanel;
        EventManager.OnRewardCollected -= UpdateRewardText;
        EventManager.OnLevelFailed -= ShowLevelFailedPanel;
        EventManager.OnFireworkModeChanged -= ToggleFireworkPanel;
        EventManager.OnScoreUpdated -= UpdateScoreUI;
    }
    private void OnEnable()
    {
        EventManager.OnLevelStarted += HandleLevelStart;
        EventManager.OnCurrencyUpdated += UpdateCurrencyText;
        EventManager.OnPowerUpCountChanged += OnPowerUpCountChanged;
        EventManager.OnLevelCompleted += ShowLevelCompletePanel;
        EventManager.OnRewardCollected += UpdateRewardText;
        EventManager.OnLevelFailed += ShowLevelFailedPanel;
        EventManager.OnFireworkModeChanged += ToggleFireworkPanel;
        EventManager.OnScoreUpdated += UpdateScoreUI;
    }

    void Start()
    {
        AddButtonListener(refreshShapesButton, OnRefreshShapesClicked);
        AddButtonListener(fireworkButton, OnFireworkButtonClicked);
        AddButtonListener(nextLevelButton, OnNextLevelClicked);
        AddButtonListener(restartButton, OnRestartClicked);
        AddButtonListener(restartButtonS, OnRestartClicked);
        AddButtonListener(cancelFireworkButton, OnCancelFireworkClicked);
        AddButtonListener(settingsButton, ToggleSettingsPanel);
        AddButtonListener(closeSettingsButton, ToggleSettingsPanel);
        AddButtonListener(soundToggleButton, ToggleSound);
        AddButtonListener(vibrationToggleButton, ToggleVibration);

        if (settingsPanel != null) settingsPanel.SetActive(false);
        UpdateSettingsUI();
    }
    private void AddButtonListener(Button button, System.Action action)
    {
        if (button != null)
        {
            button.onClick.AddListener(() => HandleButtonClick(action));
        }
    }

    private void HandleButtonClick(System.Action actionToExecute)
    {
        if (_isCooldownActive) return;
        StartCoroutine(StartCooldown());
        AudioManager.Instance.PlayButtonClickSound();
        actionToExecute?.Invoke();
    }

    private IEnumerator StartCooldown()
    {
        _isCooldownActive = true;
        yield return _buttonCooldown; 
        _isCooldownActive = false;
    }
    
    private void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            bool isPanelActive = !settingsPanel.activeSelf;
            settingsPanel.SetActive(isPanelActive);
            inputManager.enabled = !isPanelActive;
        }
    }
    private void ToggleSound()
    {
        bool newSoundState = !AudioManager.Instance.IsSoundEnabled;
        AudioManager.Instance.ToggleSound(newSoundState);
        UpdateSettingsUI();
    }
    private void ToggleVibration()
    {
        bool newVibrationState = !AudioManager.Instance.IsVibrationEnabled;
        AudioManager.Instance.ToggleVibration(newVibrationState);
        UpdateSettingsUI();
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
    private void OnNextLevelClicked()
    {
         gameManager.LoadNextLevel();
    }
    private void OnRestartClicked()
    {
        gameManager.RestartLevel();
    }
    private void OnCancelFireworkClicked()
    {
        powerUpManager.DeactivateFireworkMode();
    }

    #region Değişmeyen Kodlar
    private void UpdateSettingsUI()
    {
        if (soundToggleButton != null && toggleOnSprite != null && toggleOffSprite != null)
        {
            soundToggleButton.image.sprite = AudioManager.Instance.IsSoundEnabled ? toggleOnSprite : toggleOffSprite;
        }
        if (vibrationToggleButton != null && toggleOnSprite != null && toggleOffSprite != null)
        {
            vibrationToggleButton.image.sprite = AudioManager.Instance.IsVibrationEnabled ? toggleOnSprite : toggleOffSprite;
        }
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
    private void ToggleFireworkPanel(bool isActive) 
    {
        fireworkModePanel?.SetActive(isActive);
        if(!isActive)
        {
           UpdateAllPowerUpButtons();
        }
    }
    private void UpdateRewardText(int rewardAmount)
    {
        if (rewardText != null)
        {
            rewardText.text = $"{rewardAmount}";
        }
    }
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
    #endregion
}