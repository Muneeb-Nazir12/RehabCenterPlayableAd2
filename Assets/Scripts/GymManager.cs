using System.Collections;
using UnityEngine;

/// <summary>
/// Gym sequence manager.
/// Uses PatientAnimationController (same animator as the rest of the game — no separate animator).
/// AnimStat values: 0=Idle, 1=Walk, 5=TreadmillRun, 6=BicepCurl, 7=HappyWalk
/// </summary>
public class GymManager : MonoBehaviour
{
    public static GymManager Instance { get; private set; }

    [Header("Patient — same patient used throughout the game")]
    [SerializeField] private Transform patient;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Gym Header UI")]
    [SerializeField] private GameObject gymFinalRecoveryHeader;

    [Header("Treadmill")]
    [SerializeField] private Transform treadmillPoint;
    [SerializeField] private GameObject treadmillMessObject;
    [SerializeField] private GameObject treadmillCleaningLogo;

    [Header("Bicep Machine")]
    [SerializeField] private Transform bicepPoint;
    [SerializeField] private GameObject dumbbellInHand;
    [SerializeField] private GameObject bicepMessObject;
    [SerializeField] private GameObject bicepCleaningLogo;

    [Header("Patient Exit Points")]
    [SerializeField] private Transform exitPoint1;
    [SerializeField] private Transform exitPoint2;
    [SerializeField] private Transform exitPoint3;

    [Header("Arrow Targets")]
    [SerializeField] private Transform treadmillArrowTarget;
    [SerializeField] private Transform bicepArrowTarget;
    [SerializeField] private Transform treadmillCleanArrowTarget;
    [SerializeField] private Transform bicepCleanArrowTarget;

    private bool _treadmillCleaned;
    private bool _bicepCleaned;

    private static readonly WaitForSeconds WaitAnim = new WaitForSeconds(1f);

    private void Awake() => Instance = this;

    public void OnGymUnlocked()
    {
        if (gymFinalRecoveryHeader != null) gymFinalRecoveryHeader.SetActive(true);

        if (PlayableSequenceManager.Instance != null)
            PlayableSequenceManager.Instance.HideQuestText();

        StartCoroutine(GymSequence());
    }

    private IEnumerator GymSequence()
    {
        // ── Walk to treadmill ──────────────────────────────────────────
        if (ArrowManager.Instance != null && treadmillArrowTarget != null)
            ArrowManager.Instance.PointArrowTowards(treadmillArrowTarget);

        yield return MovePatient(treadmillPoint);

        // ── Treadmill run animation (AnimStat = 5) for 1 second ───────
        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetTreadmill();

        yield return WaitAnim;

        // ── Treadmill done — mess appears ──────────────────────────────
        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetIdle();

        if (treadmillMessObject != null) treadmillMessObject.SetActive(true);
        if (treadmillCleaningLogo != null) treadmillCleaningLogo.SetActive(true);

        // ── Walk to bicep machine ──────────────────────────────────────
        if (ArrowManager.Instance != null && bicepArrowTarget != null)
            ArrowManager.Instance.PointArrowTowards(bicepArrowTarget);

        yield return MovePatient(bicepPoint);

        // ── Bicep curl animation (AnimStat = 6) for 1 second ──────────
        if (dumbbellInHand != null) dumbbellInHand.SetActive(true);

        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetBicep();

        yield return WaitAnim;

        // ── Bicep done — mess appears ──────────────────────────────────
        if (dumbbellInHand != null) dumbbellInHand.SetActive(false);

        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetIdle();

        if (bicepMessObject != null) bicepMessObject.SetActive(true);
        if (bicepCleaningLogo != null) bicepCleaningLogo.SetActive(true);

        // ── Player cleans treadmill first ──────────────────────────────
        if (ArrowManager.Instance != null && treadmillCleanArrowTarget != null)
            ArrowManager.Instance.PointArrowTowards(treadmillCleanArrowTarget);

        if (PlayableSequenceManager.Instance != null)
            PlayableSequenceManager.Instance.ShowQuestText("Clean the treadmill area");

        yield return new WaitUntil(() => _treadmillCleaned);

        // ── Player cleans bicep area ───────────────────────────────────
        if (ArrowManager.Instance != null && bicepCleanArrowTarget != null)
            ArrowManager.Instance.PointArrowTowards(bicepCleanArrowTarget);

        if (PlayableSequenceManager.Instance != null)
            PlayableSequenceManager.Instance.ShowQuestText("Clean the bicep area");

        yield return new WaitUntil(() => _bicepCleaned);

        // ── Patient exits with happy walk (AnimStat = 7) ───────────────
        if (PlayableSequenceManager.Instance != null)
            PlayableSequenceManager.Instance.HideQuestText();

        if (gymFinalRecoveryHeader != null) gymFinalRecoveryHeader.SetActive(false);

        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetHappyWalk();

        yield return MovePatientNoAnimChange(exitPoint1);
        yield return MovePatientNoAnimChange(exitPoint2);
        yield return MovePatientNoAnimChange(exitPoint3);

        // Hide patient and trigger completion
        if (patient != null) patient.gameObject.SetActive(false);

        if (GameCompletionManager.Instance != null)
            GameCompletionManager.Instance.TriggerCompletion();
    }

    public void OnTreadmillCleaned()
    {
        if (_treadmillCleaned) return;
        _treadmillCleaned = true;
        if (treadmillMessObject != null) treadmillMessObject.SetActive(false);
        if (treadmillCleaningLogo != null) treadmillCleaningLogo.SetActive(false);
    }

    public void OnBicepCleaned()
    {
        if (_bicepCleaned) return;
        _bicepCleaned = true;
        if (bicepMessObject != null) bicepMessObject.SetActive(false);
        if (bicepCleaningLogo != null) bicepCleaningLogo.SetActive(false);
    }

    // Moves patient and sets Walk animation (AnimStat = 1)
    private IEnumerator MovePatient(Transform target)
    {
        if (target == null || patient == null) yield break;

        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetWalk();

        yield return MoveTo(target.position);
    }

    // Moves patient WITHOUT changing animation (used during happy walk exit)
    private IEnumerator MovePatientNoAnimChange(Transform target)
    {
        if (target == null || patient == null) yield break;
        yield return MoveTo(target.position);
    }

    private IEnumerator MoveTo(Vector3 targetPos)
    {
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