using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private GridManager gridManager; 
    public int _currentScore = 0;
    private int _scoreGoal = 0;
    public void PrepareLevel(int goal)
    {
        _currentScore = 0;
        _scoreGoal = goal;
        EventManager.RaiseOnScoreUpdated(_currentScore, _scoreGoal);
    }

    private void OnEnable()
    {
        EventManager.OnMarblesExploded += AddScore;
    }

    private void OnDisable()
    {
        EventManager.OnMarblesExploded -= AddScore;
    }
    
    private void AddScore(int amount)
    {
        _currentScore += amount;
        EventManager.RaiseOnScoreUpdated(_currentScore, _scoreGoal);
        
        CheckForUnlocks();

        if (_currentScore >= _scoreGoal)
        {
            EventManager.RaiseOnLevelCompleted();
        }
    }

    private void CheckForUnlocks()
    {
        if (gridManager == null) return;

        foreach (var node in gridManager.GetGrid().Values)
        {
            if (node.IsLocked && _currentScore >= node.MarblesToUnlock)
            {
                node.Unlock();
            }
        }
    }
}