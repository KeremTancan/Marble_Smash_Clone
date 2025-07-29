using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Shape_", menuName = "Marble Smash/Shape Data")]
public class ShapeData_SO : ScriptableObject
{
    [Tooltip("Şekli oluşturan mermilerin, (0,0) kabul edilen ilk mermere göre ızgara pozisyonları.")]
    public List<Vector2Int> MarblePositions;
}