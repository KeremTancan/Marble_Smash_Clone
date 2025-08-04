using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Tooltip("Oyundaki tüm seviyelerin sıralı listesi. Buraya sürükleyip bırakın.")]
    [SerializeField] private List<LevelData_SO> levels;

    private int _currentLevelIndex;
    private const string LEVEL_INDEX_KEY = "PlayerLevel";

    private void Awake()
    {
        // Oyuncunun kaldığı seviyeyi cihaz hafızasından yükle.
        // Eğer daha önce hiç oynanmamışsa, 0'dan başla.
        _currentLevelIndex = PlayerPrefs.GetInt(LEVEL_INDEX_KEY, 0);

        if (_currentLevelIndex >= levels.Count)
        {
            _currentLevelIndex = levels.Count - 1;
        }
    }

    public LevelData_SO GetCurrentLevelData()
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("LevelManager'da hiç seviye tanımlanmamış!");
            return null;
        }
        return levels[_currentLevelIndex];
    }

    public void AdvanceToNextLevel()
    {
        _currentLevelIndex++;
        
        // Eğer oyuncu son seviyeyi de geçtiyse, son seviyede kalmaya devam etsin.
        if (_currentLevelIndex >= levels.Count)
        {
            _currentLevelIndex = levels.Count - 1;
        }

        // Yeni seviye numarasını cihaz hafızasına kaydet.
        PlayerPrefs.SetInt(LEVEL_INDEX_KEY, _currentLevelIndex);
        PlayerPrefs.Save();
    }
}