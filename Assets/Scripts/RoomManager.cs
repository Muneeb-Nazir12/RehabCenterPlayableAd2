using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [Header("Bed Cleaning Point")]
    [SerializeField] private Transform bedCleaningPoint;

    [Header("Cleaning UI")]
    [SerializeField] private GameObject cleaningUI;
    [SerializeField] private Image cleaningFillImage;
    [SerializeField] private float cleaningDuration = 0.5f;

    [Header("Cleaning VFX")]
    [SerializeField] private GameObject cleaningVFX1;
    [SerializeField] private GameObject cleaningVFX2;

    [Header("Cash Bundle")]
    [SerializeField] private CashBundle cashBundle;
    [SerializeField] private Transform cashBundleArrowTarget;

    [Header("Next Unlock Point")]
    [SerializeField] private GameObject nextUnlockPointObject;
    [SerializeField] private Transform nextUnlockPointArrowTarget;

    [Header("Player")]
    [SerializeField] private CharacterMovement characterMovement;
    [SerializeField] private PlayerAnimationController playerAnim;
    [SerializeField] private GameObject mob;

    public Transform BedCleaningPoint => bedCleaningPoint;
    public bool BedCleaned { get; private set; }

    private void Awake()
    {
        Instance = this;
        if (cleaningFillImage != null) cleaningFillImage.fillAmount = 0f;
    }

    public void OnBuildingUnlocked() { }

    public void BeginCleaning() => StartCoroutine(CleaningSequence());

    private IEnumerator CleaningSequence()
    {
        if (characterMovement != null) characterMovement.canMove = false;
        if (playerAnim        != null) playerAnim.ForceCleaningState();

        if (mob        != null) mob.SetActive(true);
        if (cleaningUI != null) cleaningUI.SetActive(true);
        if (cleaningFillImage != null) cleaningFillImage.fillAmount = 0f;

        float elapsed  = 0f;
        float invDur   = cleaningDuration > 0f ? 1f / cleaningDuration : 2f;

        while (elapsed < cleaningDuration)
        {
            elapsed += Time.deltaTime;
            if (cleaningFillImage != null)
                cleaningFillImage.fillAmount = Mathf.Clamp01(elapsed * invDur);
            yield return null;
        }

        if (cleaningFillImage != null) cleaningFillImage.fillAmount = 1f;
        yield return new WaitForSeconds(0.2f);

        if (mob        != null) mob.SetActive(false);
        if (cleaningUI != null) cleaningUI.SetActive(false);

        if (cleaningVFX1 != null) cleaningVFX1.SetActive(true);
        if (cleaningVFX2 != null) cleaningVFX2.SetActive(true);

        if (AudioManager.Instance != null) AudioManager.Instance.PlayUnlockSound();

        if (playerAnim        != null) playerAnim.StopCleaningState();
        if (characterMovement != null) characterMovement.canMove = true;

        BedCleaned = true;
        if (PatientController.Instance != null) PatientController.Instance.OnBedCleaned();

        yield return null;

        if (cashBundle != null) cashBundle.Activate();
        if (ArrowManager.Instance != null && cashBundleArrowTarget != null)
            ArrowManager.Instance.PointArrowTowards(cashBundleArrowTarget);

        if (nextUnlockPointObject != null) nextUnlockPointObject.SetActive(true);
    }

    public void OnCashCollected()
    {
        if (ArrowManager.Instance != null && nextUnlockPointArrowTarget != null)
            ArrowManager.Instance.PointArrowTowards(nextUnlockPointArrowTarget);

        if (PlayableSequenceManager.Instance != null)
            PlayableSequenceManager.Instance.ShowQuestText("Unlock Cafe");

        if (PatientController.Instance != null)
            PatientController.Instance.OnCashCollected();
    }
}
