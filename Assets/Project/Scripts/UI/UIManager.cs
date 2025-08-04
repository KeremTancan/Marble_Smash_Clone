using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elemanları")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Slider scoreSlider;

    private void OnEnable()
    {
        EventManager.OnScoreUpdated += UpdateScoreUI;
    }

    private void OnDisable()
    {
        EventManager.OnScoreUpdated -= UpdateScoreUI;
    }

    private void UpdateScoreUI(int currentScore, int goal)
    {
        int displayScore = Mathf.Min(currentScore, goal);
        
        if (scoreText != null)
        {
            scoreText.text = $"{displayScore}/{goal}";
        }

        if (scoreSlider != null)
        {
            scoreSlider.maxValue = goal;
            scoreSlider.value = displayScore;
        }
    }
}