using System.Collections;
using UnityEngine;

public class GymManager : MonoBehaviour
{
    public static GymManager Instance { get; private set; }

    [Header("Patient")]
    [SerializeField] private Transform patient;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("UI")]
    [SerializeField] private GameObject gymFinalRecoveryHeader;
    [SerializeField] private GameObject postExerciseHeadUI;

    [Header("Treadmill")]
    [SerializeField] private Transform treadmillPoint;
    [SerializeField] private GameObject treadmillEquipment;
    [SerializeField] private GameObject treadmillMessObject;
    [SerializeField] private GameObject treadmillCleaningLogo;

    [Header("Bicep Machine")]
    [SerializeField] private Transform bicepPoint;
    [SerializeField] private GameObject dumbbellLeftHand;
    [SerializeField] private GameObject dumbbellRightHand;
    [SerializeField] private GameObject bicepEquipment;
    [SerializeField] private GameObject bicepMessObject;
    [SerializeField] private GameObject bicepCleaningLogo;

    [Header("Other")]
    [SerializeField] private GameObject gymFinalRecoveryObject;
    [SerializeField] private Transform exitPoint1, exitPoint2, exitPoint3;

    [Header("Arrow Targets")]
    [SerializeField] private Transform treadmillArrowTarget;
    [SerializeField] private Transform bicepArrowTarget;
    [SerializeField] private Transform treadmillCleanArrowTarget;
    [SerializeField] private Transform bicepCleanArrowTarget;

    private bool _treadmillCleaned, _bicepCleaned, _patientLeaving;
    private static readonly WaitForSeconds WaitAnim = new WaitForSeconds(1f);

    private void Awake() => Instance = this;

    private static void Show(GameObject go, bool state) { if (go) go.SetActive(state); }

    public void OnGymUnlocked()
    {
        _treadmillCleaned = _bicepCleaned = _patientLeaving = false;

        CafePatientController.Instance?.HidePostCafeHeadUI();

        // Hide the post-shower head UI now that the gym phase has begun.
        ShowerManager.Instance?.HidePostShowerHeadUI();

        Show(gymFinalRecoveryHeader, true);
        PlayableSequenceManager.Instance?.HideQuestText();

        StartCoroutine(GymSequence());
    }

    private IEnumerator GymSequence()
    {
        // --- Treadmill ---
        ArrowManager.Instance?.PointArrowTowards(treadmillArrowTarget);
        yield return MovePatient(treadmillPoint);
        PatientAnimationController.Instance?.SetTreadmill();
        yield return WaitAnim;
        PatientAnimationController.Instance?.SetIdle();
        Show(treadmillEquipment, false);
        Show(treadmillMessObject, true);
        Show(treadmillCleaningLogo, true);

        // --- Bicep ---
        ArrowManager.Instance?.PointArrowTowards(bicepArrowTarget);
        yield return MovePatient(bicepPoint);
        Show(dumbbellLeftHand, true);
        Show(dumbbellRightHand, true);
        PatientAnimationController.Instance?.SetBicep();
        yield return WaitAnim;
        Show(dumbbellLeftHand, false);
        Show(dumbbellRightHand, false);
        PatientAnimationController.Instance?.SetIdle();
        Show(gymFinalRecoveryObject, false);
        Show(bicepEquipment, false);
        Show(bicepMessObject, true);
        Show(bicepCleaningLogo, true);
        Show(postExerciseHeadUI, true);

        StartCoroutine(PatientExitSequence());

        // --- Clean treadmill ---
        ArrowManager.Instance?.PointArrowTowards(treadmillCleanArrowTarget);
        PlayableSequenceManager.Instance?.ShowQuestText("Clean the treadmill area");
        yield return new WaitUntil(() => _treadmillCleaned);

        // --- Clean bicep ---
        ArrowManager.Instance?.PointArrowTowards(bicepCleanArrowTarget);
        PlayableSequenceManager.Instance?.ShowQuestText("Clean the bicep area");
        yield return new WaitUntil(() => _bicepCleaned);

        // --- Done ---
        PlayableSequenceManager.Instance?.HideQuestText();
        Show(gymFinalRecoveryHeader, false);
        Show(postExerciseHeadUI, false);
        GameCompletionManager.Instance?.TriggerCompletion();
    }

    private IEnumerator PatientExitSequence()
    {
        if (_patientLeaving) yield break;
        _patientLeaving = true;

        Show(gymFinalRecoveryHeader, false);
        Show(postExerciseHeadUI, false);
        PatientAnimationController.Instance?.SetHappyWalk();

        yield return MovePatient(exitPoint1);
        yield return MovePatient(exitPoint2);
        yield return MovePatient(exitPoint3);

        if (patient) patient.gameObject.SetActive(false);
    }

    public void OnTreadmillCleaned()
    {
        if (_treadmillCleaned) return;
        _treadmillCleaned = true;
        Show(treadmillMessObject, false);
        Show(treadmillCleaningLogo, false);
        Show(treadmillEquipment, true);
        AudioManager.Instance?.PlayCleaningSound();
    }

    public void OnBicepCleaned()
    {
        if (_bicepCleaned) return;
        _bicepCleaned = true;
        Show(bicepMessObject, false);
        Show(bicepCleaningLogo, false);
        Show(bicepEquipment, true);
        AudioManager.Instance?.PlayCleaningSound();
    }

    private IEnumerator MovePatient(Transform target, Quaternion? finalRotation = null)
    {
        if (target == null || patient == null) yield break;

        Vector3 targetPos = target.position;
        Vector3 startDiff = targetPos - patient.position;
        startDiff.y = 0f;

        if (startDiff.sqrMagnitude <= 0.01f)
        {
            if (finalRotation.HasValue)
                patient.rotation = finalRotation.Value;
            yield break;
        }

        PatientAnimationController.Instance?.SetWalk();

        while (true)
        {
            Vector3 diff = targetPos - patient.position;
            diff.y = 0f;

            if (diff.sqrMagnitude <= 0.01f) break;

            patient.position = Vector3.MoveTowards(
                patient.position, targetPos, moveSpeed * Time.unscaledDeltaTime);

            if (diff.sqrMagnitude > 0.0001f)
                patient.rotation = Quaternion.Slerp(
                    patient.rotation,
                    Quaternion.LookRotation(diff),
                    rotationSpeed * Time.unscaledDeltaTime);

            yield return null;
        }

        if (finalRotation.HasValue)
            patient.SetPositionAndRotation(targetPos, finalRotation.Value);
        else
            patient.position = targetPos;
    }
}