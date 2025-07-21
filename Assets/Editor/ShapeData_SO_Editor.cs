using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq; 

/// ShapeData_SO'nun Inspector'daki görünümünü ve işlevselliğini zenginleştirir.
/// Sahnedeki objelerin göreceli pozisyonlarından şekil verisi oluşturmayı sağlar.

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

        var allCalculatedPositions = new List<Vector2Int>();
        foreach (Transform currentMarble in shapeParent.transform)
        {
            Vector3 worldOffset = currentMarble.position - anchorMarble.position;
            allCalculatedPositions.Add(WorldOffsetToGridCoordinate(worldOffset));
        }

        List<Vector2Int> uniquePositions = allCalculatedPositions.Distinct().ToList();

        if (childCount > uniquePositions.Count)
        {
            Debug.LogWarning($"<b>Uyarı:</b> Sahneye {childCount} mermer koydunuz ancak sadece {uniquePositions.Count} benzersiz pozisyon kaydedildi. " +
                             $"Lütfen mermilerin birbirine çok yakın olmadığından emin olun.", shapeParent);
        }

        shapeData.MarblePositions = uniquePositions;
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
