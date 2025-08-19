using System;
using System.Collections;
using UnityEngine;

public class JuiceController : MonoBehaviour
{
    public void PlayPlacementAnimation(float startScaleMultiplier = 1.2f, float duration = 0.2f)
    {
        StartCoroutine(AnimateScaleRoutine(startScaleMultiplier, 1f, duration, null));
    }

    public void PlayConnectionBounce(float bounceScaleMultiplier = 1.2f, float duration = 0.2f)
    {
        StartCoroutine(BounceEffect(bounceScaleMultiplier, duration, null));
    }

    public void PlayPreExplosionAnimation(float growScaleMultiplier = 1.5f, float duration = 0.3f, Action onComplete = null)
    {
        StartCoroutine(AnimateScaleRoutine(1f, growScaleMultiplier, duration, onComplete));
    }

    private IEnumerator AnimateScaleRoutine(float fromScaleMultiplier, float toScaleMultiplier, float duration, Action onComplete)
    {
        float timer = 0f;
        Vector3 originalScale = transform.localScale;
        Vector3 fromScale = originalScale * fromScaleMultiplier;
        Vector3 toScale = originalScale * toScaleMultiplier;

        while (timer < duration)
        {
            transform.localScale = Vector3.Lerp(fromScale, toScale, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.localScale = toScale;
        onComplete?.Invoke();
    }
    
    private IEnumerator BounceEffect(float scaleMultiplier, float duration, Action onComplete)
    {
        float timer = 0f;
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * scaleMultiplier;
        float halfDuration = duration / 2f;

        // Büyüme
        while (timer < halfDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / halfDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        // Küçülme
        timer = 0f;
        while (timer < halfDuration)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, timer / halfDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
        onComplete?.Invoke();
    }
}