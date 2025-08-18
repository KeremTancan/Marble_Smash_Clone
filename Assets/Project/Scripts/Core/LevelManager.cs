using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Tooltip("Oyundaki tüm seviyelerin sıralı listesi. Buraya sürükleyip bırakın.")] [SerializeField]
    private List<LevelData_SO> levels;

    [SerializeField] private int loopStartLevel = 15;

    private int _currentLevelIndex;
    private const string LEVEL_INDEX_KEY = "PlayerLevel";

    private int _displayLevel;
    private const string DISPLAY_LEVEL_KEY = "PlayerDisplayLevel";

    private void Awake()
    {
        _displayLevel = PlayerPrefs.GetInt(DISPLAY_LEVEL_KEY, 1);
    }

    public LevelData_SO GetCurrentLevelData()
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("LevelManager'da hiç seviye tanımlanmamış!");
            return null;
        }

        int actualLevelIndex;
        int totalDesignedLevels = levels.Count;
        int loopStartIndex = loopStartLevel - 1;

        if (_displayLevel <= totalDesignedLevels)
        {
            actualLevelIndex = _displayLevel - 1;
        }
        else
        {
            int loopRange = totalDesignedLevels - loopStartIndex;
            int indexInLoop = (_displayLevel - totalDesignedLevels - 1) % loopRange;
            actualLevelIndex = loopStartIndex + indexInLoop;
        }

        actualLevelIndex = Mathf.Clamp(actualLevelIndex, 0, totalDesignedLevels - 1);

        return levels[actualLevelIndex];
    }

    public int GetCurrentDisplayLevel()
    {
        return _displayLevel;
    }

    public void AdvanceToNextLevel()
    {
        _displayLevel++;
        PlayerPrefs.SetInt(DISPLAY_LEVEL_KEY, _displayLevel);
        PlayerPrefs.Save();
    }
}