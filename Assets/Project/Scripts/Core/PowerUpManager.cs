using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    [Tooltip("Oyundaki tüm güçlendirme verilerini buraya sürükleyin")]
    [SerializeField] private List<PowerUpData_SO> allPowerUps;

    // Her bir güçlendirmenin kalan ücretsiz hakkını saklayan sözlük
    private Dictionary<string, int> _powerUpCounts = new Dictionary<string, int>();
    
    // Roket modu için
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
            // Eğer oyuncu oyunu ilk defa açıyorsa, başlangıç haklarını ver.
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

    // Bir güçlendirmeyi kullanmayı deneyen ana metot
    public bool TryUsePowerUp(PowerUpData_SO powerUpData)
    {
        if (powerUpData == null) return false;

        string id = powerUpData.PowerUpID;
        int count = GetPowerUpCount(id);

        // 1. Ücretsiz kullanım hakkı var mı?
        if (count > 0)
        {
            _powerUpCounts[id]--;
            SavePowerUpCount(id, _powerUpCounts[id]);
            EventManager.RaiseOnPowerUpCountChanged(id, _powerUpCounts[id]); // UI'a haber ver
            return true;
        }
        // 2. Hakkı yoksa, parası yetiyor mu?
        else
        {
            if (CurrencyManager.Instance.SpendCoins(powerUpData.Cost))
            {
                // Para başarıyla harcandı
                return true;
            }
        }
        
        // Hakkı da yok, parası da yetmiyor.
        return false;
    }

    // Sadece Roket modu için olan metotlar
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