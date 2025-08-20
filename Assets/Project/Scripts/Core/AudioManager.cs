using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip marblePlaceClip;
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip levelCompleteClip;
    [SerializeField] private AudioClip levelFailedClip;
    [SerializeField] private AudioClip holdMarbleClip;
    
    private bool _isSoundEnabled = true;
    private bool _isVibrationEnabled = true;
    private const string SOUND_KEY = "GameSoundEnabled";
    private const string VIBRATION_KEY = "GameVibrationEnabled";
    public bool IsSoundEnabled => _isSoundEnabled;
    public bool IsVibrationEnabled => _isVibrationEnabled;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _isSoundEnabled = PlayerPrefs.GetInt(SOUND_KEY, 1) == 1;
        _isVibrationEnabled = PlayerPrefs.GetInt(VIBRATION_KEY, 1) == 1;
    }

    private void PlaySound(AudioClip clip)
    {
        if (!_isSoundEnabled || clip == null || sfxSource == null) return;
        
        sfxSource.PlayOneShot(clip);
    }

    public void PlayMarblePlaceSound() => PlaySound(marblePlaceClip);
    public void PlayExplosionSound() => PlaySound(explosionClip);
    public void PlayButtonClickSound() => PlaySound(buttonClickClip);
    public void PlayLevelCompleteSound() => PlaySound(levelCompleteClip);
    public void PlayLevelFailedSound() => PlaySound(levelFailedClip);   
    
    public void PlayHoldMarbleSound() => PlaySound(holdMarbleClip);
    
    public void TriggerHaptics()
    {
        if (!_isVibrationEnabled) return;

#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
        Debug.Log("Haptic feedback triggered!");
    }

    // --- YENİ: Ayarları değiştirmek için public metodlar ---
    public void ToggleSound(bool isEnabled)
    {
        _isSoundEnabled = isEnabled;
        PlayerPrefs.SetInt(SOUND_KEY, isEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleVibration(bool isEnabled)
    {
        _isVibrationEnabled = isEnabled;
        PlayerPrefs.SetInt(VIBRATION_KEY, isEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }
}