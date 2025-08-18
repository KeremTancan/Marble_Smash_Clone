using UnityEngine;

public class FXManager : MonoBehaviour
{
    [Header("Obje Havuzları")]
    [SerializeField] private ObjectPooler explosionEffectPool;

    public void PlayExplosionEffect(Vector3 position, Color color)
    {
        if (explosionEffectPool == null)
        {
            Debug.LogWarning("FXManager'da explosionEffectPool atanmamış!");
            return;
        }

        GameObject effectObject = explosionEffectPool.GetObjectFromPool();
        Vector3 effectPosition = new Vector3(position.x, position.y, -1f);
        effectObject.transform.position = effectPosition;

        ParticleSystem[] allParticles = effectObject.GetComponentsInChildren<ParticleSystem>();
        Color opaqueColor = new Color(color.r, color.g, color.b, 1f);

        foreach (ParticleSystem ps in allParticles)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var mainModule = ps.main;
            mainModule.startColor = new ParticleSystem.MinMaxGradient(opaqueColor);
        }

        effectObject.SetActive(true);

        ParticleSystem rootParticles = effectObject.GetComponent<ParticleSystem>();
        if (rootParticles != null)
        {
            rootParticles.Play(true);
        }
        else
        {
            foreach (ParticleSystem ps in allParticles)
            {
                ps.Play();
            }
        }
    }
}