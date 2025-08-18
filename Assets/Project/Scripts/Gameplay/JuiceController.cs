using System;
using System.Collections;
using UnityEngine;

public class JuiceController : MonoBehaviour
{
    // Bağlantı kurulduğunda oynatılan animasyon (Hızlı Büyü-Küçül)
    public void PlayConnectionBounce()
    {
        StartCoroutine(BounceEffect(1.2f, 0.2f, null));
    }

    // Patlama öncesi oynatılan animasyon
    public void PlayGrowAndShrink(float targetScaleMultiplier, float duration, Action onComplete)
    {
        StartCoroutine(BounceEffect(targetScaleMultiplier, duration, onComplete));
    }

    // YENİ EKLENEN FONKSİYON: Yerleştirme animasyonu (Büyükten Küçüğe)
    public void PlayPlacementAnimation(float startScaleMultiplier, float duration)
    {
        StartCoroutine(ShrinkToPlaceEffect(startScaleMultiplier, duration));
    }

    private IEnumerator ShrinkToPlaceEffect(float startMultiplier, float duration)
    {
        float timer = 0f;
        Vector3 originalScale = transform.localScale;
        Vector3 startScale = originalScale * startMultiplier;

        // Animasyon başlangıcında ölçeği büyüt
        transform.localScale = startScale;

        // Yavaşça orijinal boyutuna geri dön
        while (timer < duration)
        {
            transform.localScale = Vector3.Lerp(startScale, originalScale, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        // Animasyon sonunda ölçeğin tam olarak doğru olduğundan emin ol
        transform.localScale = originalScale;
    }
    
    // BounceEffect Coroutine'i (Değişiklik yok)
    private IEnumerator BounceEffect(float scaleMultiplier, float duration, Action onComplete)
    {
        float timer = 0f;
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * scaleMultiplier;

        // Büyüme fazı
        while (timer < duration / 2)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / (duration / 2));
            timer += Time.deltaTime;
            yield return null;
        }

        // Küçülme/Geri Dönme fazı
        timer = 0f;
        while (timer < duration / 2)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, timer / (duration / 2));
            timer += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
        
        onComplete?.Invoke();
    }
}