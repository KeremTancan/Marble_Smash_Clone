using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; } // Diğer script'lerin kolayca erişmesi için

    private int _currentCoins = 0;
    private const string COINS_KEY = "PlayerCoins"; // Kayıt için kullanılacak anahtar

    private void Awake()
    {
        // Singleton pattern: Sahnede sadece bir tane CurrencyManager olmasını sağlar
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Cihaz hafızasından parayı yükle, kayıt yoksa 0'dan başla
        _currentCoins = PlayerPrefs.GetInt(COINS_KEY, 0);
    }

    private void Start()
    {
        // Oyun başında arayüzü güncellemek için olayı tetikle
        EventManager.RaiseOnCurrencyUpdated(_currentCoins);
    }

    public int GetCurrentCoins()
    {
        return _currentCoins;
    }

    public void AddCoins(int amount)
    {
        if (amount < 0) return; // Negatif para eklenmesini engelle

        _currentCoins += amount;
        SaveCoins();
        EventManager.RaiseOnCurrencyUpdated(_currentCoins);
    }

    public bool SpendCoins(int amount)
    {
        if (amount < 0) return false;

        if (_currentCoins >= amount)
        {
            _currentCoins -= amount;
            SaveCoins();
            EventManager.RaiseOnCurrencyUpdated(_currentCoins);
            return true; // Harcama başarılı
        }
        
        Debug.Log("Yetersiz bakiye!");
        return false; // Harcama başarısız
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(COINS_KEY, _currentCoins);
        PlayerPrefs.Save();
    }
}