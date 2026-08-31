using System.Collections;
using UnityEngine;

public class PatientSparkleEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem sparkleParticle;

    private Transform _particleTransform;

    private void Awake()
    {
        if (sparkleParticle != null)
            _particleTransform = sparkleParticle.transform;
    }

    public void Play()
    {
        if (sparkleParticle == null) return;
        StartCoroutine(PopIn());
    }

    private IEnumerator PopIn()
    {
        if (_particleTransform == null) _particleTransform = sparkleParticle.transform;
        _particleTransform.localScale = Vector3.zero;
        sparkleParticle.gameObject.SetActive(true);
        sparkleParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        float elapsed = 0f;
        const float duration = 5f;
        const float invDuration = 1f / duration;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed * invDuration;
            _particleTransform.localScale = new Vector3(t, t, t);

            if (t >= 0.1f && !sparkleParticle.isPlaying)
                sparkleParticle.Play();

            yield return null;
        }

        _particleTransform.localScale = Vector3.one;
    }
}