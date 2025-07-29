using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeManager : MonoBehaviour
{
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

    private ColorPalette_SO _currentPalette;
    private int _shapesLeftInQueue; // Kuyrukta kaç şekil kaldığını sayan değişken

    private void OnEnable()
    {
        EventManager.OnShapePlaced += HandleShapePlaced;
    }

    private void OnDisable()
    {
        EventManager.OnShapePlaced -= HandleShapePlaced;
    }

    public void PrepareInitialShapes(LevelData_SO levelData)
    {
        if (!ValidateSettings()) return;

        _currentPalette = levelData.AvailableColors;
        
        // Başlangıçta tüm slotları temizle
        foreach (var slot in queueSlots)
        {
            foreach (Transform child in slot) Destroy(child.gameObject);
        }

        SpawnNewShapeBatch();
    }

    // Bir şekil kullanıldığında bu metot çağrılır
    private void HandleShapePlaced()
    {
        _shapesLeftInQueue--; // Kalan şekil sayısını azalt

        // Eğer kuyrukta hiç şekil kalmadıysa, yeni bir üçlü set oluştur
        if (_shapesLeftInQueue <= 0)
        {
            // Küçük bir gecikme ile yeni şekilleri getirelim ki daha akıcı görünsün
            StartCoroutine(SpawnBatchWithDelay(0.5f));
        }
    }

    // Yeni bir üçlü şekil seti oluşturan ana metot
    private void SpawnNewShapeBatch()
    {
        if (queueSlots.Length == 0) return;

        foreach (var slot in queueSlots)
        {
            SpawnSingleShape(slot);
        }
        _shapesLeftInQueue = queueSlots.Length; // Sayacı yeniden ayarla
    }

    // Tek bir slota yeni bir şekil oluşturan yardımcı metot
    private void SpawnSingleShape(Transform slot)
    {
        if (slot == null || _currentPalette == null) return;
        
        ShapeData_SO randomShapeData = allAvailableShapes[Random.Range(0, allAvailableShapes.Count)];
        Shape newShape = Instantiate(shapePrefab, slot);
        newShape.transform.localPosition = Vector3.zero;
        newShape.Initialize(randomShapeData, _currentPalette, marblePrefab, horizontalSpacing, verticalSpacing);
    }
    
    // Gecikmeli olarak yeni set getirmeyi sağlayan Coroutine
    private IEnumerator SpawnBatchWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnNewShapeBatch();
    }

    private bool ValidateSettings()
    {
        if (allAvailableShapes == null || allAvailableShapes.Count == 0) { Debug.LogError("ShapeManager'da 'All Available Shapes' listesi boş!", this); return false; }
        if (shapePrefab == null || marblePrefab == null) { Debug.LogError("ShapeManager'a 'Shape' veya 'Marble' prefab'ı atanmamış!", this); return false; }
        if (queueSlots == null || queueSlots.Length == 0) { Debug.LogError("ShapeManager'a 'Queue Slots' atanmamış!", this); return false; }
        return true;
    }
}