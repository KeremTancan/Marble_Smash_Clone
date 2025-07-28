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
            ghostPart.transform.localScale = originalMarble.transform.localScale;
            ghostPart.GetComponent<Collider>().enabled = false;
            _ghostParts.Add(ghostPart);
        }
    }

    public void UpdateGhostPositions(Dictionary<Marble, GridNode> targetPlacement)
    {
        foreach (var part in _ghostParts) part.SetActive(false);
        int i = 0;
        foreach (var pair in targetPlacement)
        {
            if (i < _ghostParts.Count)
            {
                _ghostParts[i].SetActive(true);
                _ghostParts[i].transform.position = pair.Value.transform.position; 
            }
            i++;
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