using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    [SerializeField] private Material validPlacementMaterial;
    [SerializeField] private Material invalidPlacementMaterial;

    private List<GameObject> _ghostParts = new List<GameObject>();

    public void Initialize(Shape shapeToCopy)
    {
        foreach (Transform child in transform) Destroy(child.gameObject);
        _ghostParts.Clear();

        foreach (var originalMarble in shapeToCopy.GetMarbles())
        {
            GameObject ghostPart = Instantiate(originalMarble.gameObject, transform);
            // Kopyanın ölçeğini, orijinalin yerel ölçeğiyle eşitle.
            ghostPart.transform.localScale = originalMarble.transform.localScale;
            ghostPart.GetComponent<Collider>().enabled = false;
            _ghostParts.Add(ghostPart);
        }
    }

    public void UpdateGhostPositions(List<GridNode> targetNodes)
    {
        // Her hayalet parçasını, ilgili hedef noktanın pozisyonuna taşı.
        for (int i = 0; i < _ghostParts.Count; i++)
        {
            if (i < targetNodes.Count && targetNodes[i] != null)
            {
                _ghostParts[i].SetActive(true);
                _ghostParts[i].transform.position = targetNodes[i].transform.position;
            }
            else
            {
                // Eğer şekil ızgaranın dışına taşıyorsa,
                // ilgili hayalet parçalarını gizle.
                _ghostParts[i].SetActive(false);
            }
        }
    }

    public void SetState(bool isValid)
    {
        Material materialToApply = isValid ? validPlacementMaterial : invalidPlacementMaterial;
        foreach (var part in _ghostParts)
        {
            part.GetComponent<MeshRenderer>().material = materialToApply;
        }
    }
}