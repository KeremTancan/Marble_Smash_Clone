using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sadece test amaçlıdır. Belirtilen ShapeData listesini
/// sırayla ekranda oluşturur ve belirli bir süre sonra yok eder.
/// </summary>
public class ShapeSpawner_Test : MonoBehaviour
{
    [Header("Test Ayarları")]
    [Tooltip("Sırayla spawn edilecek şekillerin listesi.")]
    [SerializeField] private List<ShapeData_SO> shapesToTest;

    [Tooltip("Mermiyi temsil edecek prefab.")]
    [SerializeField] private GameObject marblePrefab;

    [Tooltip("Her şeklin ekranda kalma süresi (saniye).")]
    [SerializeField] private float displayDuration = 5.0f;

    [Header("Görsel Ayarlar (GridManager ile aynı olmalı)")]
    [SerializeField] private float horizontalSpacing = 1.0f;
    [SerializeField] private float verticalSpacing = 0.866f;

    // Oyunu başlatınca test döngüsünü başlatalım.
    private void Start()
    {
        // Gerekli atamalar yapılmamışsa hata ver ve dur.
        if (shapesToTest == null || shapesToTest.Count == 0 || marblePrefab == null)
        {
            Debug.LogError("Lütfen 'Shapes To Test' listesini ve 'Marble Prefab'ı Inspector'dan atayın!");
            return; // Script'in çalışmasını engelle.
        }

        // Test döngüsünü başlatan Coroutine'i çağır.
        StartCoroutine(SpawnShapesRoutine());
    }

    /// <summary>
    /// Şekilleri sırayla spawn eden, bekleyen ve yok eden ana döngü.
    /// </summary>
    private IEnumerator SpawnShapesRoutine()
    {
        int currentIndex = 0;

        // Oyun çalıştığı sürece sonsuz bir döngüde kal.
        while (true)
        {
            // Mevcut şekil verisini al.
            ShapeData_SO currentShapeData = shapesToTest[currentIndex];

            // Oluşturulan mermileri bir arada tutmak için geçici bir parent oluşturalım.
            // Bu, onları daha sonra kolayca yok etmemizi sağlar.
            GameObject shapeContainer = new GameObject($"SpawnedShape_{currentShapeData.name}");
            shapeContainer.transform.position = this.transform.position;

            // Şekil verisindeki her bir göreceli pozisyon için bir mermer oluştur.
            foreach (Vector2Int gridPos in currentShapeData.MarblePositions)
            {
                // Izgara koordinatını, dünya pozisyonuna çevir.
                float worldX = gridPos.x * horizontalSpacing + (gridPos.y % 2 != 0 ? horizontalSpacing / 2f : 0);
                float worldY = gridPos.y * verticalSpacing;
                Vector3 worldPosition = new Vector3(worldX, worldY, 0);

                // Prefab'ı bu pozisyonda, container'ın altında oluştur.
                Instantiate(marblePrefab, shapeContainer.transform.position + worldPosition, Quaternion.identity, shapeContainer.transform);
            }

            // Belirtilen süre kadar bekle.
            yield return new WaitForSeconds(displayDuration);

            // Süre dolunca, oluşturduğumuz container'ı ve içindeki tüm mermileri yok et.
            Destroy(shapeContainer);

            // Bir sonraki şekle geç.
            currentIndex++;

            // Eğer listenin sonuna geldiysek, başa dön.
            if (currentIndex >= shapesToTest.Count)
            {
                currentIndex = 0;
            }
        }
    }
}
