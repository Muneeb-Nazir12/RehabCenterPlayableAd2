using System.Collections;
using UnityEngine;

public class PatientController : MonoBehaviour
{
    public static PatientController Instance;

    [Header("Patient")]
    [SerializeField] private Transform patient;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Path Points — Room Lobby")]
    [SerializeField] private Transform patientStandingPointBeforeRoomEnter;
    [SerializeField] private Transform patientStandingPointBeforeRoomEnter1;

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

    [Header("Bed Objects")]
    [SerializeField] private GameObject bedObject;
    [SerializeField] private GameObject bedMess;

    [Header("Arrow Targets")]
    [SerializeField] private Transform lobbyArrowTarget;
    [SerializeField] private Transform buildingUnlockArrowTarget;
    [SerializeField] private Transform bedArrowTarget;
    [SerializeField] private Transform bedMessArrowTarget;
    [SerializeField] private Transform cashArrowTarget;
    [SerializeField] private Transform cafeUnlockArrowTarget;

    [Header("Cash")]
    [SerializeField] private GameObject cashBundleObject;

    [Header("Patient Head UI — Post-bed")]
    [SerializeField] private GameObject postBedHeadUI;

    private const float SleepDuration = 1.5f;

    private static readonly Quaternion Rot_0_Neg90_0 = Quaternion.Euler(0f, -90f, 0f);

    public static bool IsFlowComplete { get; private set; } = false;
    public bool IsBedMessy            { get; private set; } = false;

    private void Awake()
    {
        Instance = this;
        IsFlowComplete = false;
        IsBedMessy = false;
    }

    private void Start() => StartCoroutine(PatientFlow());

    private IEnumerator PatientFlow()
    {
        PlayableSequenceManager.Instance?.ShowQuestText("Unlock the room");
        ArrowManager.Instance?.PointArrowTowards(lobbyArrowTarget);

        yield return MovePatient(patientStandingPointBeforeRoomEnter);
        yield return MovePatient(patientStandingPointBeforeRoomEnter1);
        PatientAnimationController.Instance?.SetIdle();

        ArrowManager.Instance?.PointArrowTowards(buildingUnlockArrowTarget);

        yield return new WaitUntil(() => BuildingUnlockManager.buildingUnlockCount >= 1);

        PlayableSequenceManager.Instance?.HideQuestText();
        ArrowManager.Instance?.PointArrowTowards(bedArrowTarget);
        PlayableSequenceManager.Instance?.ShowQuestText("See patient taking rest");

        yield return MovePatient(patientMovingTowardsBedPoint1);
        yield return MovePatient(patientMovingTowardsBedPoint2);

        Quaternion layRot = Rot_0_Neg90_0;
        patient.SetPositionAndRotation(patientLayingDownPoint.position, layRot);
        PatientAnimationController.Instance?.SetLayDown();

        if (sleepingVFX != null) sleepingVFX.SetActive(true);

        float elapsed = 0f;
        Vector3 layPos = patientLayingDownPoint.position;
        while (elapsed < SleepDuration)
        {
            patient.SetPositionAndRotation(layPos, layRot);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (sleepingVFX != null) sleepingVFX.SetActive(false);

        if (bedObject   != null) bedObject.SetActive(false);
        if (bedMess     != null) bedMess.SetActive(true);
        if (bedDirtyVFX != null) bedDirtyVFX.SetActive(true);

        IsBedMessy = true;

        PlayableSequenceManager.Instance?.ShowQuestText("Clean the bed");
        ArrowManager.Instance?.PointArrowTowards(bedMessArrowTarget);

        patient.position = patientAfterSleepPoint1.position;

        yield return MovePatient(patientAfterSleepPoint2);
        yield return MovePatient(patientAfterSleepPoint3);

        PatientAnimationController.Instance?.SetIdle();

        if (postBedHeadUI != null) postBedHeadUI.SetActive(true);

        IsFlowComplete = true;
    }

    public void OnBedCleaned()
    {
        IsBedMessy = false;

        if (bedDirtyVFX != null) bedDirtyVFX.SetActive(false);
        if (bedMess     != null) bedMess.SetActive(false);
        if (bedObject   != null) bedObject.SetActive(true);

        AudioManager.Instance?.PlayCleaningSound();

        if (cashBundleObject != null) cashBundleObject.SetActive(true);

        ArrowManager.Instance?.PointArrowTowards(cashArrowTarget);
        PlayableSequenceManager.Instance?.ShowQuestText("Pick up cash");
    }

    public void OnCashCollected()
    {
        PlayableSequenceManager.Instance?.ShowQuestText("Unlock Cafe");
        ArrowManager.Instance?.PointArrowTowards(cafeUnlockArrowTarget);
        if (postBedHeadUI != null) postBedHeadUI.SetActive(false);
    }

    public void HidePostBedHeadUI()
    {
        if (postBedHeadUI != null) postBedHeadUI.SetActive(false);
    }

    private IEnumerator MovePatient(Transform target)
    {

        if (target == null || patient == null)
            yield break;
        PatientAnimationController.Instance.SetWalk();
        while (true)
        {
            Vector3 diff = target.position - patient.position;
            diff.y = 0f;

            if (diff.sqrMagnitude <= 0.01f)
            {
                patient.position = target.position;
                yield break;
            }

            patient.position = Vector3.MoveTowards(
                patient.position,
                target.position,
                moveSpeed * Time.unscaledDeltaTime
            );

            patient.rotation = Quaternion.Slerp(
                patient.rotation,
                Quaternion.LookRotation(diff),
                rotationSpeed * Time.unscaledDeltaTime
            );

            yield return null;
        }
    }
}
