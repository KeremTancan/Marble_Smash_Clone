using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    [SerializeField] private Material sharedGhostMaterial;
    private List<GameObject> _ghostParts = new List<GameObject>();
    private MaterialPropertyBlock _propertyBlock;

    public void Initialize(Shape shapeToCopy)
    {
        _propertyBlock = new MaterialPropertyBlock();

        foreach (Transform child in transform) Destroy(child.gameObject);
        _ghostParts.Clear();

        foreach (var originalMarble in shapeToCopy.GetMarbles())
        {
            GameObject ghostPart = Instantiate(originalMarble.gameObject, transform);
            ghostPart.GetComponent<Collider>().enabled = false;
            
            MeshRenderer renderer = ghostPart.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = sharedGhostMaterial;
                Color originalColor = originalMarble.MarbleColor;
                float desiredAlpha = 150f / 255f;
                Color finalColor = new Color(originalColor.r, originalColor.g, originalColor.b, desiredAlpha);
                _propertyBlock.SetColor("_BaseColor", finalColor);
                renderer.SetPropertyBlock(_propertyBlock);
            }
            _ghostParts.Add(ghostPart);
        }
    }
    
    public void UpdatePositions(Dictionary<Marble, GridNode> targetPlacement)
    {
        foreach (var part in _ghostParts)
        {
            part.SetActive(false);
        }

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
}