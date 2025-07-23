using System.Collections.Generic;
using UnityEngine;

/// Oyuncuya sunulacak şekil haznesini (queue) yönetir.
/// Rastgele şekiller oluşturur ve renk paletini onlara aktarır.

public class ShapeManager : MonoBehaviour
{
    [Header("Seviye Verisi")]
    [SerializeField] private LevelData_SO currentLevelData;

    [Header("Şekil Veritabanı")]
    [SerializeField] private List<ShapeData_SO> allAvailableShapes;

    [Header("Prefabs")]
    [SerializeField] private Shape shapePrefab;
    [SerializeField] private GameObject marblePrefab;

    [Header("Hazne (Queue) Ayarları")]
    [SerializeField] private Transform[] queueSlots;

    [Header("Izgara Ayarları (Görsel için)")]
    [SerializeField] private float horizontalSpacing = 1.0f;
    [SerializeField] private float verticalSpacing = 0.866f;

    private void Start()
    {
        if (!ValidateSettings()) return;
        SpawnNewShapeQueue();
    }

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
            ShapeData_SO randomShapeData = allAvailableShapes[Random.Range(0, allAvailableShapes.Count)];
            Shape newShape = Instantiate(shapePrefab, queueSlots[i]);
            newShape.transform.localPosition = Vector3.zero;

            newShape.Initialize(randomShapeData, palette, marblePrefab, horizontalSpacing, verticalSpacing);
        }
    }

    private bool ValidateSettings()
    {
        if (currentLevelData == null) { Debug.LogError("ShapeManager'a 'Current Level Data' atanmamış!", this); return false; }
        if (allAvailableShapes == null || allAvailableShapes.Count == 0) { Debug.LogError("ShapeManager'da 'All Available Shapes' listesi boş!", this); return false; }
        if (shapePrefab == null || marblePrefab == null) { Debug.LogError("ShapeManager'a 'Shape' veya 'Marble' prefab'ı atanmamış!", this); return false; }
        if (queueSlots == null || queueSlots.Length == 0) { Debug.LogError("ShapeManager'a 'Queue Slots' atanmamış!", this); return false; }
        return true;
    }
}
