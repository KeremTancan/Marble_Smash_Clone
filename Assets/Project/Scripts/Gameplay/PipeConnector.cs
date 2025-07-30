using System.Collections;
using UnityEngine;

public class PipeConnector : MonoBehaviour
{
    [Header("Görsel Referans")]
    [SerializeField] private Transform pipeVisual; 

    [Header("Ayarlar")]
    [SerializeField] private float marbleRadius = 0.15f;

    private void Awake()
    {
        if (pipeVisual == null && transform.childCount > 0)
        {
            pipeVisual = transform.GetChild(0);
        }
    }
    
    public void AnimateConnection(Vector3 startPoint, Vector3 endPoint, Color color)
    {
        StartCoroutine(AnimatePipeRoutine(startPoint, endPoint, color));
    }

    private IEnumerator AnimatePipeRoutine(Vector3 initialStartPoint, Vector3 initialEndPoint, Color color)
    {
        MeshRenderer renderer = pipeVisual.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_BaseColor", color);
            renderer.SetPropertyBlock(propBlock);
        }

        Vector3 direction = (initialEndPoint - initialStartPoint).normalized;
        Vector3 startPoint = initialStartPoint + direction * marbleRadius;
        Vector3 endPoint = initialEndPoint - direction * marbleRadius;

        transform.position = startPoint;
        transform.LookAt(endPoint);

        float pipeLength = Vector3.Distance(startPoint, endPoint);
        if (pipeLength < 0) pipeLength = 0;

        Vector3 finalScale = new Vector3(pipeVisual.localScale.x, pipeLength / 2f, pipeVisual.localScale.z);
        Vector3 initialScale = new Vector3(finalScale.x, 0, finalScale.z);

        float duration = 0.1f;
        float timer = 0f;

        while (timer < duration)
        {
            float t = timer / duration;
            pipeVisual.localScale = Vector3.Lerp(initialScale, finalScale, t);
            transform.position = Vector3.Lerp(startPoint, (startPoint + endPoint) / 2f, t);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = (startPoint + endPoint) / 2f;
        pipeVisual.localScale = finalScale;
    }
}