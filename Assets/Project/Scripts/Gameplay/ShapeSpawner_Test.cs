using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Sadece test amaçlıdır. Belirtilen ShapeData listesini sırayla ekranda oluşturur ve belirli bir süre sonra yok eder.

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

    private void Start()
    {
        if (shapesToTest == null || shapesToTest.Count == 0 || marblePrefab == null)
        {
            Debug.LogError("Lütfen 'Shapes To Test' listesini ve 'Marble Prefab'ı Inspector'dan atayın!");
            return;
        }

        StartCoroutine(SpawnShapesRoutine());
    }

    private IEnumerator SpawnShapesRoutine()
    {
        int currentIndex = 0;

        while (true)
        {
            ShapeData_SO currentShapeData = shapesToTest[currentIndex];

            GameObject shapeContainer = new GameObject($"SpawnedShape_{currentShapeData.name}");
            shapeContainer.transform.position = this.transform.position;

            foreach (Vector2Int gridPos in currentShapeData.MarblePositions)
            {
                // Izgara koordinatını, dünya pozisyonuna çevir.
                float worldX = gridPos.x * horizontalSpacing + (gridPos.y % 2 != 0 ? horizontalSpacing / 2f : 0);
                float worldY = gridPos.y * verticalSpacing;
                Vector3 worldPosition = new Vector3(worldX, worldY, 0);

                Instantiate(marblePrefab, shapeContainer.transform.position + worldPosition, Quaternion.identity, shapeContainer.transform);
            }

            yield return new WaitForSeconds(displayDuration);
            Destroy(shapeContainer);
            currentIndex++;
            if (currentIndex >= shapesToTest.Count)
            {
                currentIndex = 0;
            }
        }
    }
}
