using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioClip cashCollectClip;
    [SerializeField] private AudioClip unlockClip;
    [SerializeField] private AudioClip cleaningClip;
    [SerializeField] private AudioClip servingClip;
    [SerializeField] private AudioClip effectClip;
    [SerializeField] private AudioClip levelCompletionClip;


    private void Awake() => Instance = this;

    private void Start()
    {
        if (bgmSource != null) bgmSource.Play();
    }

    public void PlayCashCollectSound() => PlaySFX(cashCollectClip);
    public void PlayUnlockSound() => PlaySFX(unlockClip);
    public void PlayServingSound() => PlaySFX(servingClip);
    public void PlayCleaningSound() => PlaySFX(cleaningClip);
    public void PlayEffectSound() => PlaySFX(effectClip);
    public void LevelCompletionSound() => PlaySFX(levelCompletionClip);
    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }
}
