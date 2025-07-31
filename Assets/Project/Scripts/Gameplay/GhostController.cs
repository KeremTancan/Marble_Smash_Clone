using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    private List<GameObject> _ghostParts = new List<GameObject>();

    public void Initialize(Shape shapeToCopy)
    {
        foreach (Transform child in transform) Destroy(child.gameObject);
        _ghostParts.Clear();

        foreach (var originalMarble in shapeToCopy.GetMarbles())
        {
            GameObject ghostPart = Instantiate(originalMarble.gameObject, transform);
            ghostPart.GetComponent<Collider>().enabled = false;
            
            MeshRenderer renderer = ghostPart.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material materialInstance = new Material(renderer.material);
                
                materialInstance.SetFloat("_Surface", 1.0f); // Yüzey Tipi -> Transparent
                materialInstance.SetFloat("_Blend", 0.0f);   // Karıştırma Modu -> Alpha
                materialInstance.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                materialInstance.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                materialInstance.SetInt("_ZWrite", 0);
                materialInstance.DisableKeyword("_ALPHATEST_ON");
                materialInstance.EnableKeyword("_ALPHABLEND_ON");
                materialInstance.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                materialInstance.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                
                Color originalColor = originalMarble.MarbleColor;
                float desiredAlpha = 120f / 255f;
                materialInstance.color = new Color(originalColor.r, originalColor.g, originalColor.b, desiredAlpha); 
                renderer.material = materialInstance;
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