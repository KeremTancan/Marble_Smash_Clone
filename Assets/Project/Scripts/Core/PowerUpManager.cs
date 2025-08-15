using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    [Tooltip("Oyundaki tüm güçlendirme verilerini buraya sürükleyin")]
    [SerializeField] private List<PowerUpData_SO> allPowerUps;
    private Dictionary<string, int> _powerUpCounts = new Dictionary<string, int>();
    private bool _isFireworkModeActive = false; 
    public bool IsFireworkModeActive => _isFireworkModeActive;

    private void Awake()
    {
        LoadPowerUpCounts();
    }

    private void LoadPowerUpCounts()
    {
        foreach (var powerUp in allPowerUps)
        {
            string key = $"PowerUpCount_{powerUp.PowerUpID}";
            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetInt(key, powerUp.InitialFreeUses);
            }
            _powerUpCounts[powerUp.PowerUpID] = PlayerPrefs.GetInt(key);
        }
    }

    private void SavePowerUpCount(string powerUpID, int count)
    {
        string key = $"PowerUpCount_{powerUpID}";
        PlayerPrefs.SetInt(key, count);
        PlayerPrefs.Save();
    }

    public int GetPowerUpCount(string powerUpID)
    {
        _powerUpCounts.TryGetValue(powerUpID, out int count);
        return count;
    }
    public bool TryUsePowerUp(PowerUpData_SO powerUpData)
    {
        if (powerUpData == null) return false;

        string id = powerUpData.PowerUpID;
        int count = GetPowerUpCount(id);

        if (count > 0)
        {
            _powerUpCounts[id]--;
            SavePowerUpCount(id, _powerUpCounts[id]);
            EventManager.RaiseOnPowerUpCountChanged(id, _powerUpCounts[id]); 
            return true;
        }
        else
        {
            if (CurrencyManager.Instance.SpendCoins(powerUpData.Cost))
            {
                return true;
            }
        }
        return false;
    }
    public void ActivateFireworkMode()
    {
        if (_isFireworkModeActive) return;
        _isFireworkModeActive = true;
        EventManager.RaiseOnFireworkModeChanged(true);
    }

    public void DeactivateFireworkMode()
    {
        if (!_isFireworkModeActive) return;
        _isFireworkModeActive = false;
        EventManager.RaiseOnFireworkModeChanged(false);
    }
}