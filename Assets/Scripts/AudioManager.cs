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

    private void Awake() => Instance = this;

    private void Start()
    {
        bgmSource.Play();
    }
    public void PlayCashCollectSound()       => PlaySFX(cashCollectClip);
    public void PlayUnlockSound()            => PlaySFX(unlockClip);

    public void PlayCleaningSound()
    {
        PlaySFX(cleaningClip);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
