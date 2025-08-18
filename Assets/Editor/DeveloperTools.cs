using UnityEditor;
using UnityEngine;

public class DeveloperTools : Editor
{
    [MenuItem("Geliştirici/Oyuncu Verilerini Sil")]
    public static void ClearPlayerData()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("<color=orange>Tüm oyuncu verileri (PlayerPrefs) temizlendi.</color>");
    }

    [MenuItem("Geliştirici/Seviyeyi 50 Yap")]
    public static void SetLevelTo50()
    {
        PlayerPrefs.SetInt("PlayerDisplayLevel", 50);
        PlayerPrefs.Save();
        Debug.Log("<color=cyan>Oyuncu seviyesi 50 olarak ayarlandı. Oyunu başlatın.</color>");
    }
}