// Dosya: Editor/ShapeData_SO_Editor.cs
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq; // LINQ kütüphanesini ekliyoruz.

/// <summary>
/// ShapeData_SO'nun Inspector'daki görünümünü ve işlevselliğini zenginleştirir.
/// Sahnedeki objelerin göreceli pozisyonlarından şekil verisi oluşturmayı sağlar.
/// </summary>
[CustomEditor(typeof(ShapeData_SO))]
public class ShapeData_SO_Editor : Editor
{
    private GameObject shapeParent;

    private const float HORIZONTAL_SPACING = 1.0f;
    private const float VERTICAL_SPACING = 0.866f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Create From Scene", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox(
            "1. Sahnede boş bir GameObject oluşturun.\n" +
            "2. Şekli oluşturacak mermer objelerini bu GameObject'in altına (child) yerleştirin.\n" +
            "3. Hiyerarşideki İLK mermer, şeklin (0,0) merkezi (çapası) olarak kabul edilecektir.\n" +
            "4. Ana GameObject'i aşağıdaki alana sürükleyin ve butona basın.",
            MessageType.Info);

        shapeParent = (GameObject)EditorGUILayout.ObjectField("Shape Parent Object", shapeParent, typeof(GameObject), true);

        if (GUILayout.Button("Generate Positions from Parent") && shapeParent != null)
        {
            GenerateShapeData();
        }
    }

    private void GenerateShapeData()
    {
        int childCount = shapeParent.transform.childCount;
        if (childCount == 0)
        {
            Debug.LogWarning("Shape Parent Object'in altında hiç çocuk obje bulunamadı!", shapeParent);
            return;
        }

        ShapeData_SO shapeData = (ShapeData_SO)target;
        Transform anchorMarble = shapeParent.transform.GetChild(0);

        // --- YENİ VE SAĞLAM MANTIK ---
        // 1. Önce tüm pozisyonları, mükerrer olsalar bile bir listeye alalım.
        var allCalculatedPositions = new List<Vector2Int>();
        foreach (Transform currentMarble in shapeParent.transform)
        {
            Vector3 worldOffset = currentMarble.position - anchorMarble.position;
            allCalculatedPositions.Add(WorldOffsetToGridCoordinate(worldOffset));
        }

        // 2. LINQ kullanarak listedeki mükerrer kayıtları temizleyelim.
        List<Vector2Int> uniquePositions = allCalculatedPositions.Distinct().ToList();

        // 3. Kullanıcıyı uyaralım (en önemli kısım).
        // Eğer sahnedeki mermi sayısı ile kaydedilen benzersiz pozisyon sayısı farklıysa,
        // bu, bazı mermilerin üst üste bindiği anlamına gelir.
        if (childCount > uniquePositions.Count)
        {
            Debug.LogWarning($"<b>Uyarı:</b> Sahneye {childCount} mermer koydunuz ancak sadece {uniquePositions.Count} benzersiz pozisyon kaydedildi. " +
                             $"Lütfen mermilerin birbirine çok yakın olmadığından emin olun.", shapeParent);
        }

        // 4. ScriptableObject'in listesini nihai, temizlenmiş liste ile güncelle.
        shapeData.MarblePositions = uniquePositions;

        // Değişiklikleri kaydet.
        EditorUtility.SetDirty(shapeData);
        AssetDatabase.SaveAssets();

        Debug.Log($"'{shapeData.name}' başarıyla güncellendi! Şekil, '{anchorMarble.name}' merkez alınarak {shapeData.MarblePositions.Count} merminin göreceli pozisyonu ile oluşturuldu.", shapeData);
    }

    private Vector2Int WorldOffsetToGridCoordinate(Vector3 worldOffset)
    {
        int gridY = Mathf.RoundToInt(worldOffset.y / VERTICAL_SPACING);
        float xShift = (gridY % 2 != 0) ? HORIZONTAL_SPACING / 2f : 0;
        int gridX = Mathf.RoundToInt((worldOffset.x - xShift) / HORIZONTAL_SPACING);
        return new Vector2Int(gridX, gridY);
    }
}
