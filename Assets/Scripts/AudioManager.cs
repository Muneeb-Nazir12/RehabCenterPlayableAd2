using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioClip bgmClip;
    [SerializeField] private AudioClip cashCollectClip;
    [SerializeField] private AudioClip servingClip;
    [SerializeField] private AudioClip unlockClip;
    [SerializeField] private AudioClip treatmentCompleteClip;

    private void Awake() => Instance = this;
    private void Start() => bgmSource.Play();

    public void PlayCashCollectSound() => sfxSource.PlayOneShot(cashCollectClip);
    public void PlayServingSound() => sfxSource.PlayOneShot(servingClip);
    public void PlayUnlockSound() => sfxSource.PlayOneShot(unlockClip);
    public void PlayTreatmentCompleteSound() => sfxSource.PlayOneShot(treatmentCompleteClip);
    public void PlayCleaningSound() { if (sfxSource != null && servingClip != null) sfxSource.PlayOneShot(servingClip); }
}
