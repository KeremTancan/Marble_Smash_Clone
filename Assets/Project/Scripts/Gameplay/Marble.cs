using UnityEngine;

[RequireComponent(typeof(MeshRenderer))] 
public class Marble : MonoBehaviour
{
    public Color MarbleColor { get; private set; }
    public GridNode ParentNode { get; set; }
    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }
    public void SetColor(Color newColor)
    {
        MarbleColor = newColor;
        if (_meshRenderer == null)
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }
        _meshRenderer.material.color = newColor;
    }
}