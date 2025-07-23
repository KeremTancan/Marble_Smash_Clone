using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
    public ShapeData_SO ShapeData { get; private set; }
    private readonly List<Marble> _marbles = new List<Marble>();

    public bool IsPlaced { get; private set; }

    public void Initialize(ShapeData_SO shapeData, Color color, GameObject marblePrefab, float hSpacing, float vSpacing)
    {
        this.ShapeData = shapeData;
        gameObject.name = $"Shape_{shapeData.name}";

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        _marbles.Clear();
        var localPositions = new List<Vector3>();

        foreach (var gridPos in shapeData.MarblePositions)
        {
            float worldX = gridPos.x * hSpacing + (gridPos.y % 2 != 0 ? hSpacing / 2f : 0);
            float worldY = gridPos.y * vSpacing;
            localPositions.Add(new Vector3(worldX, worldY, 0));
        }

        Vector3 centerOffset = Vector3.zero;
        foreach (var pos in localPositions)
        {
            centerOffset += pos;
        }
        centerOffset /= localPositions.Count;

        foreach (var pos in localPositions)
        {
            GameObject marbleObj = Instantiate(marblePrefab, this.transform);
            marbleObj.transform.localPosition = pos - centerOffset;

            Marble newMarble = marbleObj.GetComponent<Marble>();
            newMarble.SetColor(color);
            _marbles.Add(newMarble);
        }
    }
}
