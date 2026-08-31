using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [Header("Bed Cleaning Point")]
    [SerializeField] private Transform bedCleaningPoint;

    [Header("Cleaning UI")]
    public GameObject cleaningUI;
    [SerializeField] private Image cleaningFillImage;
    [SerializeField] private float cleaningDuration = 0.5f;

    [Header("Cleaning VFX")]
    [SerializeField] private GameObject cleaningVFX1;
    [SerializeField] private Transform cleaningVFX;

    [Header("Shared Particle")]
    [SerializeField] private ParticleSystem sharedVFX;

    [Header("Cash Bundle")]
    [SerializeField] private CashBundle cashBundle;
    [SerializeField] private Transform cashBundleArrowTarget;

    [Header("Next Unlock Point")]
    [SerializeField] private GameObject CafeUnlockPointObject;
    [SerializeField] private GameObject CafeUnlockPointCanvas;
    [SerializeField] private Transform CafeUnlockPointArrowTarget;

    [Header("Player")]
    [SerializeField] private CharacterMovement characterMovement;
    [SerializeField] private PlayerAnimationController playerAnim;
    [SerializeField] private GameObject mob;

    public Transform BedCleaningPoint => bedCleaningPoint;
    public bool BedCleaned { get; private set; }

    private bool _cleaningInProgress;
    private static readonly WaitForSeconds WaitCleaningDelay = new WaitForSeconds(0.2f);
    private static readonly Quaternion CleaningRotation = Quaternion.Euler(0f, 180f, 0f);
    private float _invDur;
    private Transform _playerTransform;

    private void Awake()
    {
        Instance = this;
        _invDur = cleaningDuration > 0f ? 1f / cleaningDuration : 2f;
        if (cleaningFillImage != null) cleaningFillImage.fillAmount = 0f;
        if (characterMovement != null) _playerTransform = characterMovement.transform;
    }

    public void BeginCleaning()
    {
        if (BedCleaned || _cleaningInProgress) return;
        if (RoomPatientController.Instance == null || !RoomPatientController.Instance.IsBedMessy) return;

        _cleaningInProgress = true;
        StartCoroutine(CleaningSequence());
    }

    private void PlayVFXAt(Transform point)
    {
        if (sharedVFX == null || point == null) return;
        sharedVFX.transform.position = point.position;
        sharedVFX.Play();
    }

    private IEnumerator CleaningSequence()
    {
        if (characterMovement != null) characterMovement.canMove = false;
        if (playerAnim != null) playerAnim.ForceCleaningState();

        if (mob != null) mob.SetActive(true);
        if (cleaningFillImage != null) cleaningFillImage.fillAmount = 0f;

        if (_playerTransform == null && characterMovement != null)
            _playerTransform = characterMovement.transform;

        float elapsed = 0f;
        while (elapsed < cleaningDuration)
        {
            elapsed += Time.deltaTime;
            if (_playerTransform != null) _playerTransform.rotation = CleaningRotation;
            if (cleaningFillImage != null)
                cleaningFillImage.fillAmount = Mathf.Clamp01(elapsed * _invDur);
            yield return null;
        }

        if (_playerTransform != null) _playerTransform.rotation = CleaningRotation;
        if (cleaningFillImage != null) cleaningFillImage.fillAmount = 1f;

        yield return WaitCleaningDelay;

        if (mob != null) mob.SetActive(false);
        if (cleaningUI != null) cleaningUI.SetActive(false);

        if (cleaningVFX1 != null) cleaningVFX1.SetActive(true);
        PlayVFXAt(cleaningVFX);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCleaningSound();
            AudioManager.Instance.PlayUnlockSound();
        }

        if (playerAnim != null) playerAnim.StopCleaningState();
        if (characterMovement != null) characterMovement.canMove = true;

        BedCleaned = true;
        _cleaningInProgress = false;

        RoomPatientController.Instance?.OnBedCleaned();

        if (cashBundle != null) cashBundle.Activate();

        if (ArrowManager.Instance != null && cashBundleArrowTarget != null)
            ArrowManager.Instance.PointArrowTowards(cashBundleArrowTarget);
    }

    public void OnCashCollected()
    {
        if (ArrowManager.Instance != null && CafeUnlockPointArrowTarget != null)
            ArrowManager.Instance.PointArrowTowards(CafeUnlockPointArrowTarget);

        if (CafeUnlockPointObject != null) CafeUnlockPointObject.SetActive(true);
        if (CafeUnlockPointCanvas != null) CafeUnlockPointCanvas.SetActive(true);

        PlayableSequenceManager.Instance?.ShowQuestText("Unlock Cafe");
        RoomPatientController.Instance?.OnCashCollected();
    }
}