using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Yöneticiler")]
    [SerializeField] private GameManager gameManager;

    [Header("Oyun İçi UI Elemanları")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Slider scoreSlider;
    
    [Header("Oyun Sonu Panelleri")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject levelFailedPanel;

    [Header("Butonlar")]
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button restartButton;


    private void OnEnable()
    {
        EventManager.OnScoreUpdated += UpdateScoreUI;
        EventManager.OnLevelCompleted += ShowLevelCompletePanel;
        EventManager.OnLevelFailed += ShowLevelFailedPanel;
    }

    private void OnDisable()
    {
        EventManager.OnScoreUpdated -= UpdateScoreUI;
        EventManager.OnLevelCompleted -= ShowLevelCompletePanel;
        EventManager.OnLevelFailed -= ShowLevelFailedPanel;
    }

    void Start()
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (levelFailedPanel != null) levelFailedPanel.SetActive(false);
        

        if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
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
    
    private void OnNextLevelClicked() { gameManager.LoadNextLevel(); }
    private void OnRestartClicked() { gameManager.RestartLevel(); }
    private void UpdateScoreUI(int currentScore, int goal) {
        int displayScore = Mathf.Min(currentScore, goal);
        if (scoreText != null) scoreText.text = $"{displayScore} / {goal}";
        if (scoreSlider != null) {
            scoreSlider.maxValue = goal;
            scoreSlider.value = displayScore;
        }
    }
}