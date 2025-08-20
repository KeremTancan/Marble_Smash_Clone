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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayMarblePlaceSound() => PlaySound(marblePlaceClip);
    public void PlayExplosionSound() => PlaySound(explosionClip);
    public void PlayButtonClickSound() => PlaySound(buttonClickClip);
    public void PlayLevelCompleteSound() => PlaySound(levelCompleteClip);
    public void PlayLevelFailedSound() => PlaySound(levelFailedClip);   
    
    public void PlayHoldMarbleSound() => PlaySound(holdMarbleClip);
    
    public void TriggerHaptics()
    {
        // Handheld.Vibrate(); // Basit titreşim için bu satır kullanılabilir.
        // Daha gelişmiş titreşimler için Unity Asset Store'dan "Nice Vibrations" gibi ücretsiz asset'ler önerilir.
        Debug.Log("Haptic feedback triggered!");
    }
}