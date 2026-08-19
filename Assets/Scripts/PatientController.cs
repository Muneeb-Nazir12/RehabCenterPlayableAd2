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

    private const float SleepDuration = 1.5f;

    private void Awake() => Instance = this;
    private void Start() => StartCoroutine(PatientFlow());

    private IEnumerator PatientFlow()
    {
        PlayableSequenceManager.Instance?.ShowQuestText("Unlock the room");
        if (ArrowManager.Instance != null && lobbyArrowTarget != null)
            ArrowManager.Instance.PointArrowTowards(lobbyArrowTarget);

        yield return MovePatient(patientStandingPointBeforeRoomEnter);

        SnapRotationTowards(patientMovingTowardsBedPoint1);
        PatientAnimationController.Instance?.SetIdle();

        ArrowManager.Instance?.PointArrowTowards(buildingUnlockArrowTarget);

        yield return new WaitUntil(() => BuildingUnlockManager.buildingUnlockCount >= 1);

        PlayableSequenceManager.Instance?.HideQuestText();

        ArrowManager.Instance?.PointArrowTowards(bedArrowTarget);
        PlayableSequenceManager.Instance?.ShowQuestText("See patient taking rest");

        yield return MovePatient(patientMovingTowardsBedPoint1);
        yield return MovePatient(patientMovingTowardsBedPoint2);

        patient.position = patientLayingDownPoint.position;
        patient.rotation = Quaternion.Euler(0f, -90f, 0f);
        PatientAnimationController.Instance?.SetLayDown();

        if (sleepingVFX != null) sleepingVFX.SetActive(true);

        float elapsed = 0f;
        Vector3 layPos = patientLayingDownPoint.position;
        Quaternion layRot = Quaternion.Euler(0f, -90f, 0f);
        while (elapsed < SleepDuration)
        {
            patient.position = layPos;
            patient.rotation = layRot;
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (sleepingVFX != null) sleepingVFX.SetActive(false);

        if (bedObject != null) bedObject.SetActive(false);
        if (bedMess != null) bedMess.SetActive(true);
        if (bedDirtyVFX != null) bedDirtyVFX.SetActive(true);

        PlayableSequenceManager.Instance?.ShowQuestText("Clean the bed");
        ArrowManager.Instance?.PointArrowTowards(bedMessArrowTarget);

        // Teleport to point1 first, THEN snap rotation toward point2
        // so the patient faces the correct direction before walking starts
        patient.transform.position = patientAfterSleepPoint1.position;
        SnapRotationTowards(patientAfterSleepPoint2);

        yield return MovePatient(patientAfterSleepPoint2);

        // Snap toward point3 before final move
        SnapRotationTowards(patientAfterSleepPoint3);
        yield return MovePatient(patientAfterSleepPoint3);

        // Face correct idle direction at final rest position
        SnapRotationTowards(patientAfterSleepPoint3);
        PatientAnimationController.Instance?.SetIdle();
    }

    public void OnBedCleaned()
    {
        if (bedDirtyVFX != null) bedDirtyVFX.SetActive(false);
        if (bedMess != null) bedMess.SetActive(false);
        if (bedObject != null) bedObject.SetActive(true);

        if (cashBundleObject != null) cashBundleObject.SetActive(true);

        ArrowManager.Instance?.PointArrowTowards(cashArrowTarget);
        PlayableSequenceManager.Instance?.ShowQuestText("Pick up cash");
    }

    public void OnCashCollected()
    {
        PlayableSequenceManager.Instance?.ShowQuestText("Unlock Cafe");
        ArrowManager.Instance?.PointArrowTowards(cafeUnlockArrowTarget);
    }

    private void ForceRotation(float x, float y, float z)
    {
        if (patient == null) return;
        patient.rotation = Quaternion.Euler(x, y, z);
    }

    private void SnapRotationTowards(Transform target)
    {
        if (patient == null || target == null) return;
        Vector3 dir = target.position - patient.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        patient.rotation = Quaternion.LookRotation(dir.normalized);
    }

    private IEnumerator MovePatient(Transform target)
    {
        if (target == null || patient == null) yield break;

        PatientAnimationController.Instance?.SetWalk();

        Vector3 targetPos = target.position;
        while (true)
        {
            Vector3 diff = targetPos - patient.position;
            diff.y = 0f;
            if (diff.sqrMagnitude <= 0.01f) break;

            patient.position = Vector3.MoveTowards(
                patient.position, targetPos, moveSpeed * Time.deltaTime);

            if (diff.sqrMagnitude > 0.0001f)
                patient.rotation = Quaternion.Slerp(
                    patient.rotation,
                    Quaternion.LookRotation(diff),
                    rotationSpeed * Time.deltaTime);

            yield return null;
        }
        patient.position = targetPos;
    }
}