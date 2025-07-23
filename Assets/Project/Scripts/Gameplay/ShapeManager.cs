using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Oyuncuya sunulacak şekil haznesini (queue) yönetir.
/// Rastgele şekil ve renklerde 3 adet Shape objesi oluşturur.
/// </summary>
public class ShapeManager : MonoBehaviour
{
    [Header("Seviye Verisi")]
    [Tooltip("ShapeManager, renk paletini bu seviye verisinden alacak.")]
    [SerializeField] private LevelData_SO currentLevelData;

    [Header("Şekil Veritabanı")]
    [Tooltip("Oyunda kullanılabilecek TÜM şekillerin listesi.")]
    [SerializeField] private List<ShapeData_SO> allAvailableShapes;

    [Header("Prefabs")]
    [SerializeField] private Shape shapePrefab; // PF_Shape prefab'ı
    [SerializeField] private GameObject marblePrefab; // PF_Marble prefab'ı

    [Header("Hazne (Queue) Ayarları")]
    [Tooltip("Şekillerin oluşturulacağı 3 adet pozisyon (boş GameObject'ler).")]
    [SerializeField] private Transform[] queueSlots;

    [Header("Izgara Ayarları (Görsel için)")]
    [SerializeField] private float horizontalSpacing = 1.0f;
    [SerializeField] private float verticalSpacing = 0.866f;

    // Sadece test için, oyun başlar başlamaz yeni şekilleri oluşturalım.
    private void Start()
    {
        if (!ValidateSettings()) return;
        
        SpawnNewShapeQueue();
    }

    /// <summary>
    /// Hazneye 3 adet yeni, rastgele şekil oluşturur.
    /// </summary>
    public void SpawnNewShapeQueue()
    {
        if (queueSlots.Length != 3)
        {
            Debug.LogError("ShapeManager'da tam olarak 3 adet Queue Slot atanmalıdır!", this);
            return;
        }

        ColorPalette_SO palette = currentLevelData.AvailableColors;

        for (int i = 0; i < queueSlots.Length; i++)
        {
            // 1. Rastgele bir şekil verisi seç.
            ShapeData_SO randomShapeData = allAvailableShapes[Random.Range(0, allAvailableShapes.Count)];
            
            // 2. Rastgele bir renk seç.
            Color randomColor = palette.Colors[Random.Range(0, palette.Colors.Count)];

            // 3. Yeni bir Shape objesi oluştur.
            Shape newShape = Instantiate(shapePrefab, queueSlots[i]);
            newShape.transform.localPosition = Vector3.zero; // Slot'un tam merkezine yerleştir.

            // 4. Shape'i verilerle kur.
            newShape.Initialize(randomShapeData, randomColor, marblePrefab, horizontalSpacing, verticalSpacing);
        }
    }

    /// <summary>
    /// Inspector'da gerekli tüm atamaların yapılıp yapılmadığını kontrol eder.
    /// </summary>
    private bool ValidateSettings()
    {
        if (currentLevelData == null)
        {
            Debug.LogError("ShapeManager'a 'Current Level Data' atanmamış!", this);
            return false;
        }
        if (allAvailableShapes == null || allAvailableShapes.Count == 0)
        {
            Debug.LogError("ShapeManager'da 'All Available Shapes' listesi boş!", this);
            return false;
        }
        if (shapePrefab == null || marblePrefab == null)
        {
            Debug.LogError("ShapeManager'a 'Shape' veya 'Marble' prefab'ı atanmamış!", this);
            return false;
        }
        if (queueSlots == null || queueSlots.Length == 0)
        {
            Debug.LogError("ShapeManager'a 'Queue Slots' atanmamış!", this);
            return false;
        }
        return true;
    }
}
