using System.Collections;
using UnityEngine;

public class RoomPatientController : MonoBehaviour
{
    public static RoomPatientController Instance;

    [Header("Animation")]
    [SerializeField] private PatientAnimationController patientAnim;

    [Header("Patient Mover")]
    [SerializeField] private PatientMover mover;
    [SerializeField] private Transform patient;

    [Header("Path Points — To Bed")]
    [SerializeField] private Transform patientMovingTowardsBedPoint1;
    [SerializeField] private Transform patientMovingTowardsBedPoint2;
    [SerializeField] private Transform patientLayingDownPoint;

    [Header("Path Points — After Sleep")]
    [SerializeField] private Transform patientAfterSleepPoint1;
    [SerializeField] private Transform patientAfterSleepPoint2;
    [SerializeField] private Transform patientAfterSleepPoint3;

    [Header("VFX")]
    [SerializeField] private GameObject sleepingVFX;
    [SerializeField] private GameObject bedDirtyVFX;
    [SerializeField] private GameObject cloudVFX;
    [SerializeField] private GameObject afterGetUpVFX;

    [Header("Cloud VFX Settings")]
    [SerializeField] private float cloudReduceDuration = 3f;

    [Header("Bed Objects")]
    [SerializeField] private GameObject bedObject;
    [SerializeField] private GameObject bedMess;

    [Header("Arrow Targets")]
    [SerializeField] private Transform buildingUnlockArrowTarget;
    [SerializeField] private Transform bedArrowTarget;
    [SerializeField] private Transform bedMessArrowTarget;
    [SerializeField] private Transform cashArrowTarget;
    [SerializeField] private Transform cafeUnlockArrowTarget;

    [Header("Cash")]
    [SerializeField] private GameObject cashBundleObject;
    [SerializeField] private GameObject sleepUI;
    public GameObject foodUI;

    private const float SleepDuration = 1.5f;
    private static readonly Quaternion Rot_0_Neg90_0 = Quaternion.Euler(0f, -90f, 0f);

    public static bool IsFlowComplete { get; private set; } = false;
    public bool IsBedMessy { get; private set; } = false;

    private Transform[] _toBedPath;
    private Transform[] _afterSleepPath;
    private ParticleSystem _cloudPS;

    private void Awake()
    {
        Instance = this;
        _toBedPath = new Transform[] { patientMovingTowardsBedPoint1, patientMovingTowardsBedPoint2 };
        _afterSleepPath = new Transform[] { patientAfterSleepPoint2, patientAfterSleepPoint3 };
    }

    private void Start() => StartCoroutine(PatientFlow());

    private void PlayVFX(GameObject vfx)
    {
        if (vfx == null) return;
        vfx.SetActive(false);
        vfx.SetActive(true);
    }

    private IEnumerator ReduceCloudEmission()
    {
        if (_cloudPS == null) yield break;

        var emission = _cloudPS.emission;
        float startRate = emission.rateOverTime.constant;
        float elapsed = 0f;

        while (elapsed < cloudReduceDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / cloudReduceDuration);
            emission.rateOverTime = Mathf.Lerp(startRate, 7f, t);
            yield return null;
        }

        emission.rateOverTime = 7f;
    }

    private IEnumerator PatientFlow()
    {
        while (!GameIntroManager.IntroComplete
            || ReceptionPatientController.Instance == null
            || !ReceptionPatientController.Instance.IsFlowComplete)
            yield return null;

        if (cloudVFX != null) cloudVFX.SetActive(true);
        StartCoroutine(ReduceCloudEmission());

        if (sleepUI != null) sleepUI.SetActive(true);
        PlayableSequenceManager.Instance?.ShowQuestText("Unlock the room");
        if (patientAnim != null) patientAnim.SetIdle();
        ArrowManager.Instance?.PointArrowTowards(buildingUnlockArrowTarget);

        while (BuildingUnlockManager.buildingUnlockCount < 1) yield return null;

        PlayableSequenceManager.Instance?.HideQuestText();
        ArrowManager.Instance?.HideArrow();
        PlayableSequenceManager.Instance?.ShowQuestText("Patient is taking rest");

        if (mover != null && _toBedPath != null)
        {
            mover.MovePath(_toBedPath);
            while (mover.IsMoving) yield return null;
        }

        if (mover != null) mover.Stop();
        Quaternion layRot = Rot_0_Neg90_0;
        Vector3 layPos = patientLayingDownPoint != null ? patientLayingDownPoint.position : patient.position;
        if (patient != null) patient.SetPositionAndRotation(layPos, layRot);
        if (patientAnim != null) patientAnim.SetLayDown();

        if (sleepUI != null) sleepUI.SetActive(false);
        if (sleepingVFX != null) sleepingVFX.SetActive(true);

        float elapsed = 0f;
        while (elapsed < SleepDuration)
        {
            if (patient != null) patient.SetPositionAndRotation(layPos, layRot);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (sleepingVFX != null) sleepingVFX.SetActive(false);
        if (bedObject != null) bedObject.SetActive(false);
        if (bedMess != null) bedMess.SetActive(true);
        if (bedDirtyVFX != null) bedDirtyVFX.SetActive(true);
        if (RoomManager.Instance != null && RoomManager.Instance.cleaningUI != null)
            RoomManager.Instance.cleaningUI.SetActive(true);

        IsBedMessy = true;
        TargetArrowIndicator.GoToTarget(2);
        PlayableSequenceManager.Instance?.ShowQuestText("Clean the bed");
        ArrowManager.Instance?.PointArrowTowards(bedMessArrowTarget);

        if (foodUI != null) foodUI.SetActive(true);
        if (patient != null && patientAfterSleepPoint1 != null)
            patient.position = patientAfterSleepPoint1.position;

        PlayVFX(afterGetUpVFX);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayEffectSound();

        if (mover != null && _afterSleepPath != null)
        {
            mover.MovePath(_afterSleepPath);
            while (mover.IsMoving) yield return null;
        }

        if (patientAnim != null) patientAnim.SetIdle();
        IsFlowComplete = true;
    }

    public void OnBedCleaned()
    {
        IsBedMessy = false;
        if (bedDirtyVFX != null) bedDirtyVFX.SetActive(false);
        if (bedMess != null) bedMess.SetActive(false);
        if (bedObject != null) bedObject.SetActive(true);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCleaningSound();
        if (cashBundleObject != null) cashBundleObject.SetActive(true);
        ArrowManager.Instance?.PointArrowTowards(cashArrowTarget);
        PlayableSequenceManager.Instance?.ShowQuestText("Pick up cash");
    }

    public void OnCashCollected()
    {
        TargetArrowIndicator.GoToTarget(3);
        PlayableSequenceManager.Instance?.ShowQuestText("Unlock Cafe");
        ArrowManager.Instance?.PointArrowTowards(cafeUnlockArrowTarget);
    }
}