using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))] 
public class Marble : MonoBehaviour
{
    public Color MarbleColor { get; private set; }
    public GridNode ParentNode { get; set; }
    private MeshRenderer _meshRenderer;
    private JuiceController _juiceController;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        _juiceController = GetComponent<JuiceController>();
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
    
    public void PlayExplosionAnimation(Action onAnimationComplete)
    {
        if (_juiceController != null)
        {
            _juiceController.PlayPreExplosionAnimation(1.5f, 0.3f, onAnimationComplete);
        }
        else
        {
            onAnimationComplete?.Invoke();
        }
    }
}