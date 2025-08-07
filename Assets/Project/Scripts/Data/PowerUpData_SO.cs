using UnityEngine;

[CreateAssetMenu(fileName = "PowerUp_", menuName = "Marble Smash/Power-Up Data")]
public class PowerUpData_SO : ScriptableObject
{
    [Tooltip("Bu güçlendirmenin benzersiz kimliği. Örn: 'refresh', 'firework'")]
    public string PowerUpID; 
    
    [Tooltip("Ücretsiz kullanım hakları bittiğinde istenecek para miktarı")]
    public int Cost; 
    
    [Tooltip("Bu özelliğin oyuncu için kullanılabilir olacağı seviye")]
    public int UnlockLevel; 
    
    [Tooltip("Oyuna başlarken oyuncuya verilecek ücretsiz kullanım hakkı sayısı")]
    public int InitialFreeUses;
}