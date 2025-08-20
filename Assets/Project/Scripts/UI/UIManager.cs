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
    [SerializeField] private Button restartButtonS;
    
    [Header("Ayarlar Paneli")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private Button soundToggleButton;
    [SerializeField] private Button vibrationToggleButton;
    
    [Header("Açık/Kapalı Görselleri")]
    [SerializeField] private Sprite toggleOnSprite;
    [SerializeField] private Sprite toggleOffSprite;

    private int _currentCoins;
    private int _currentLevel;
    private bool _isRefreshButtonOnCooldown = false;
    private bool _isFireworkButtonOnCooldown = false;
    private bool _isSettingsButtonOnCooldown = false;
    private readonly WaitForSeconds _buttonCooldown = new WaitForSeconds(0.1f);

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

    void Start()
    {
        if (refreshShapesButton != null) refreshShapesButton.onClick.AddListener(OnRefreshShapesClicked);
        if (fireworkButton != null) fireworkButton.onClick.AddListener(OnFireworkButtonClicked);
        if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (restartButtonS != null) restartButtonS.onClick.AddListener(OnRestartClicked);
        if (cancelFireworkButton != null) cancelFireworkButton.onClick.AddListener(OnCancelFireworkClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(OnSettingsButtonClicked);
        if (soundToggleButton != null) soundToggleButton.onClick.AddListener(ToggleSound);
        if (vibrationToggleButton != null) vibrationToggleButton.onClick.AddListener(ToggleVibration);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        UpdateSettingsUI();
    }
    
    private void OnSettingsButtonClicked()
    {
        if (_isSettingsButtonOnCooldown) return;
        StartCoroutine(SettingsButtonCooldown());
        
        ToggleSettingsPanel();
    }
    private IEnumerator SettingsButtonCooldown()
    {
        _isSettingsButtonOnCooldown = true;
        settingsButton.interactable = false;
        if(closeSettingsButton != null) closeSettingsButton.interactable = false;

        yield return _buttonCooldown; 

        _isSettingsButtonOnCooldown = false;
        settingsButton.interactable = true;
        if(closeSettingsButton != null) closeSettingsButton.interactable = true;
    }
    private void ToggleSettingsPanel()
    {
        AudioManager.Instance.PlayButtonClickSound();
        if (settingsPanel != null)
        {
            bool isPanelActive = !settingsPanel.activeSelf;
            settingsPanel.SetActive(isPanelActive);

            if (inputManager != null)
            {
                if (isPanelActive)
                {
                    inputManager.enabled = false;
                }
                else
                {
                    inputManager.enabled = true;
                }
            }
        }
    }

    private void ToggleSound()
    {
        AudioManager.Instance.PlayButtonClickSound();
        bool newSoundState = !AudioManager.Instance.IsSoundEnabled;
        AudioManager.Instance.ToggleSound(newSoundState);
        UpdateSettingsUI();
    }

    private void ToggleVibration()
    {
        AudioManager.Instance.PlayButtonClickSound();
        bool newVibrationState = !AudioManager.Instance.IsVibrationEnabled;
        AudioManager.Instance.ToggleVibration(newVibrationState);
        UpdateSettingsUI();
    }

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
    
    private void OnRefreshShapesClicked()
    {
        AudioManager.Instance.PlayButtonClickSound();
        if (_isRefreshButtonOnCooldown) return;
        StartCoroutine(RefreshButtonCooldown());

        if (powerUpManager.TryUsePowerUp(refreshPowerUpData))
        {
            shapeManager.RefreshShapeQueue();
        }
        else
        {
            UpdateAllPowerUpButtons();
        }
    }
    private void OnFireworkButtonClicked()
    {
        AudioManager.Instance.PlayButtonClickSound();
        if (_isFireworkButtonOnCooldown) return;
        powerUpManager.ActivateFireworkMode();
        StartCoroutine(FireworkButtonCooldown());
    }

    private IEnumerator RefreshButtonCooldown()
    {
        _isRefreshButtonOnCooldown = true;
        refreshShapesButton.interactable = false; 
        
        yield return _buttonCooldown; 
        
        _isRefreshButtonOnCooldown = false;
        UpdateAllPowerUpButtons();
    }

    private IEnumerator FireworkButtonCooldown()
    {
        _isFireworkButtonOnCooldown = true;
        fireworkButton.interactable = false;

        yield return _buttonCooldown; 

        _isFireworkButtonOnCooldown = false;
        if(!powerUpManager.IsFireworkModeActive)
        {
            UpdateAllPowerUpButtons();
        }
    }

    #region Değişmeyen Kodlar
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
        if (!_isRefreshButtonOnCooldown)
        {
            UpdateButtonState(refreshPowerUpData, refreshShapesButton, refreshLockParent, refreshLockText, refreshCountParent, refreshCountText, refreshCostParent, refreshCostText);
        }
        if (!_isFireworkButtonOnCooldown)
        {
            UpdateButtonState(fireworkPowerUpData, fireworkButton, fireworkLockParent, fireworkLockText, fireworkCountParent, fireworkCountText, fireworkCostParent, fireworkCostText);
        }
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

    private void OnNextLevelClicked()
    {
        AudioManager.Instance.PlayButtonClickSound();
         gameManager.LoadNextLevel();
    }
    private void OnRestartClicked()
    {
        AudioManager.Instance.PlayButtonClickSound();
        gameManager.RestartLevel();
    }

    private void OnCancelFireworkClicked()
    {
        AudioManager.Instance.PlayButtonClickSound();
        powerUpManager.DeactivateFireworkMode();
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