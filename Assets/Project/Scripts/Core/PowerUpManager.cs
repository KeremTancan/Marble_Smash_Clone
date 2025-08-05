using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    private bool _isFireworkModeActive = false;

    public bool IsFireworkModeActive => _isFireworkModeActive;

    public void ActivateFireworkMode()
    {
        if (_isFireworkModeActive) return;
        
        _isFireworkModeActive = true;
        Debug.Log("Havai Fişek Modu Aktif!");
        EventManager.RaiseOnFireworkModeChanged(true);
    }

    public void DeactivateFireworkMode()
    {
        if (!_isFireworkModeActive) return;
        
        _isFireworkModeActive = false;
        Debug.Log("Havai Fişek Modu Kapatıldı.");
        EventManager.RaiseOnFireworkModeChanged(false);
    }

    public void ConsumeFirework()
    {
        // TODO: Roket sayısını azaltma veya para eksiltme mantığı buraya eklenecek.
        DeactivateFireworkMode();
    }
}