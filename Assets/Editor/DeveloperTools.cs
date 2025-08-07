using UnityEditor;
using UnityEngine;

public class DeveloperTools
{
    [MenuItem("Developer/Clear Player Prefs (Tüm Kayıtları Sil)")]
    private static void ClearPlayerPrefs()
    {
        // Cihaz hafızasındaki tüm kayıtlı verileri (seviye, para, güçlendirme hakları vb.) siler.
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        
        Debug.Log("GELİŞTİRİCİ MESAJI: Tüm PlayerPrefs verileri (kayıtlar) başarıyla silindi!");
    }
}