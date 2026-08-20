using System.Collections;
using UnityEngine;

public class CafePatientController : MonoBehaviour
{
    public static CafePatientController Instance;

    [Header("Patient")]
    [SerializeField] private GameObject patient;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Patient Objects")]
    [SerializeField] private GameObject patientHandTray;
    [SerializeField] private GameObject chairTray;
    [SerializeField] private GameObject dirtyBurgerPlate;

    [Header("VFX")]
    [SerializeField] private GameObject sitVFX;
    [SerializeField] private GameObject afterCleaningVFX;

    [Header("Mess UI")]
    [SerializeField] private GameObject messTableUI;

    [Header("Cash")]
    [SerializeField] private CashBundle cashBundle;

    [Header("Path Points")]
    [SerializeField] private Transform counterPoint;
    [SerializeField] private Transform counterPoint1;

    [SerializeField] private Transform chairPoint;
    [SerializeField] private Transform chairPoint1;
    [SerializeField] private Transform afterSitPoint1;
    [SerializeField] private Transform afterSitPoint2;
    [SerializeField] private Transform afterSitPoint3;
    [SerializeField] private Transform afterSitPoint4;

    [Header("Arrow Targets")]
    [SerializeField] private Transform chairArrowTarget;
    [SerializeField] private Transform tableArrowTarget;

    [Header("Patient Head UI — Food")]
    [SerializeField] private GameObject foodHeadUI;

    [Header("Patient Head UI — Post-Cafe")]
    [SerializeField] private GameObject postCafeHeadUI;

    public bool IsPatientAtCounter { get; private set; }
    public bool IsTableDirty { get; private set; }

    private static readonly Quaternion Rot_0_90_0 = Quaternion.Euler(0f, 90f, 0f);
    private static readonly Quaternion Rot_0_180_0 = Quaternion.Euler(0f, 180f, 0f);

    private static readonly WaitForSeconds WaitSit = new WaitForSeconds(1.5f);
    private static readonly WaitForSeconds WaitGetUp = new WaitForSeconds(0.5f);

    private Transform _patientT;

    // Track the running coroutine so ShowerManager can stop it cleanly
    private Coroutine _patientFlowCoroutine;

    private void Awake()
    {
        Instance = this;
        IsTableDirty = false;
        if (patient != null) _patientT = patient.transform;
    }

    public void StartPatientFlow()
    {
        IsTableDirty = false;
        _patientFlowCoroutine = StartCoroutine(PatientFlow());
    }

    /// <summary>
    /// Called by ShowerManager before it starts moving the patient,
    /// so both coroutines never fight over the same transform.
    /// </summary>
    public void StopPatientFlow()
    {
        if (_patientFlowCoroutine != null)
        {
            StopCoroutine(_patientFlowCoroutine);
            _patientFlowCoroutine = null;
        }
    }

    private IEnumerator PatientFlow()
    {
        IsPatientAtCounter = false;

        if (counterPoint == null || _patientT == null) yield break;

        yield return new WaitUntil(() => PatientController.IsFlowComplete);

        PatientController.Instance?.HidePostBedHeadUI();

        yield return MovePatient(counterPoint);
        yield return MovePatient(counterPoint1);

        PatientAnimationController.Instance?.SetIdle();
        IsPatientAtCounter = true;

        if (foodHeadUI != null) foodHeadUI.SetActive(true);

        yield return new WaitUntil(() => CafeManager.Instance != null && CafeManager.Instance.CounterServed);

        if (foodHeadUI != null) foodHeadUI.SetActive(false);
        IsPatientAtCounter = false;

        if (patientHandTray != null) patientHandTray.SetActive(true);
        PatientAnimationController.Instance?.SetWalkWithHolding();

        ArrowManager.Instance?.PointArrowTowards(chairArrowTarget);

        yield return MovePatientHolding(chairPoint);
        yield return MovePatientHolding(chairPoint1);

        if (patientHandTray != null) patientHandTray.SetActive(false);
        if (chairTray != null) chairTray.SetActive(true);

        PatientAnimationController.Instance?.SetSitting();
        PlayableSequenceManager.Instance?.HideQuestText();

        yield return WaitSit;

        if (sitVFX != null) sitVFX.SetActive(true);
        yield return WaitGetUp;
        if (sitVFX != null) sitVFX.SetActive(false);

        if (chairTray != null) chairTray.SetActive(false);
        if (dirtyBurgerPlate != null) dirtyBurgerPlate.SetActive(true);
        if (messTableUI != null) messTableUI.SetActive(true);
        IsTableDirty = true;

        CafeManager.Instance?.OnTableBecameDirty();
        ArrowManager.Instance?.PointArrowTowards(tableArrowTarget);

        yield return MovePatient(afterSitPoint1);
        yield return MovePatient(afterSitPoint2);
        yield return MovePatient(afterSitPoint3);
        yield return MovePatient(afterSitPoint4);

        PatientAnimationController.Instance?.SetIdle();

        if (postCafeHeadUI != null) postCafeHeadUI.SetActive(true);

        // Coroutine finished naturally — clear the reference
        _patientFlowCoroutine = null;
    }

    public void OnTableCleaned()
    {
        if (!IsTableDirty) return;
        IsTableDirty = false;

        if (messTableUI != null) messTableUI.SetActive(false);
        if (dirtyBurgerPlate != null) dirtyBurgerPlate.SetActive(false);
        if (chairTray != null) chairTray.SetActive(false);

        PlayableSequenceManager.Instance?.HideQuestText();

        if (afterCleaningVFX != null) { afterCleaningVFX.SetActive(false); afterCleaningVFX.SetActive(true); }

        AudioManager.Instance?.PlayCleaningSound();
        cashBundle?.Activate();
        CafeManager.Instance?.OnTableCleaned();
    }

    public void HidePostCafeHeadUI()
    {
        if (postCafeHeadUI != null) postCafeHeadUI.SetActive(false);
    }

    private IEnumerator MovePatient(Transform target)
    {
        if (target == null || patient == null)
            yield break;
        PatientAnimationController.Instance.SetWalk();
        while (true)
        {
            Vector3 diff = target.position - patient.transform.position;
            diff.y = 0f;

            if (diff.sqrMagnitude <= 0.01f)
            {
                patient.transform.position = target.position;
                yield break;
            }

            patient.transform.position = Vector3.MoveTowards(
                patient.transform.position,
                target.position,
                moveSpeed * Time.unscaledDeltaTime
            );

            patient.transform.rotation = Quaternion.Slerp(
                patient.transform.rotation,
                Quaternion.LookRotation(diff),
                rotationSpeed * Time.unscaledDeltaTime
            );

            yield return null;
        }
    }

    private IEnumerator MovePatientHolding(Transform target)
    {
        if (target == null || _patientT == null) yield break;

        PatientAnimationController.Instance?.SetWalkWithHolding();
        yield return MoveTo(target.position);
    }

    private IEnumerator MoveTo(Vector3 targetPos)
    {
        while (true)
        {
            Vector3 diff = targetPos - _patientT.position;
            diff.y = 0f;
            if (diff.sqrMagnitude <= 0.01f) break;

            _patientT.position = Vector3.MoveTowards(
                _patientT.position, targetPos, moveSpeed * Time.deltaTime);

            if (diff.sqrMagnitude > 0.0001f)
                _patientT.rotation = Quaternion.Slerp(
                    _patientT.rotation,
                    Quaternion.LookRotation(diff),
                    rotationSpeed * Time.deltaTime);

            yield return null;
        }
        _patientT.position = targetPos;
    }
}