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
    [SerializeField] private Transform chairPoint;
    [SerializeField] private Transform afterSitPoint1;
    [SerializeField] private Transform afterSitPoint2;
    [SerializeField] private Transform afterSitPoint3;

    [Header("Arrow Targets")]
    [SerializeField] private Transform chairArrowTarget;
    [SerializeField] private Transform tableArrowTarget;

    public bool IsPatientAtCounter { get; private set; }
    public bool IsTableDirty { get; private set; }

    private static readonly WaitForSeconds WaitSit = new WaitForSeconds(1.5f);
    private static readonly WaitForSeconds WaitGetUp = new WaitForSeconds(0.5f);

    private void Awake()
    {
        Instance = this;
        IsTableDirty = false;
    }

    public void StartPatientFlow()
    {
        IsTableDirty = false;
        StartCoroutine(PatientFlow());
    }

    private IEnumerator PatientFlow()
    {
        PatientAnimationController.Instance.SetWalk();
        IsPatientAtCounter = false;

        if (counterPoint == null || patient == null) yield break;

        yield return MovePatient(counterPoint);

        ForceRotation(0f, 180f, 0f);
        IsPatientAtCounter = true;
        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetIdle();

        yield return new WaitUntil(() => CafeManager.Instance != null && CafeManager.Instance.CounterServed);
        IsPatientAtCounter = false;

        if (patientHandTray != null) patientHandTray.SetActive(true);

        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetWalkWithHolding();

        if (ArrowManager.Instance != null && chairArrowTarget != null)
            ArrowManager.Instance.PointArrowTowards(chairArrowTarget);

        yield return MovePatientWithHolding(chairPoint);

        ForceRotation(0f, 90f, 0f);

        if (patientHandTray != null) patientHandTray.SetActive(false);
        if (chairTray != null) chairTray.SetActive(true);

        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetSitting();

        if (PlayableSequenceManager.Instance != null)
            PlayableSequenceManager.Instance.HideQuestText();

        yield return WaitSit;

        if (sitVFX != null) sitVFX.SetActive(true);
        yield return WaitGetUp;
        if (sitVFX != null) sitVFX.SetActive(false);

        if (chairTray != null) chairTray.SetActive(false);
        if (dirtyBurgerPlate != null) dirtyBurgerPlate.SetActive(true);
        if (messTableUI != null) messTableUI.SetActive(true);
        IsTableDirty = true;

        if (CafeManager.Instance != null) CafeManager.Instance.OnTableBecameDirty();

        if (ArrowManager.Instance != null && tableArrowTarget != null)
            ArrowManager.Instance.PointArrowTowards(tableArrowTarget);

        yield return MovePatient(afterSitPoint1);
        yield return MovePatient(afterSitPoint2);
        yield return MovePatient(afterSitPoint3);

        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetIdle();
    }

    public void OnTableCleaned()
    {
        if (!IsTableDirty) return;
        IsTableDirty = false;

        if (messTableUI != null) messTableUI.SetActive(false);
        if (dirtyBurgerPlate != null) dirtyBurgerPlate.SetActive(false);
        if (chairTray != null) chairTray.SetActive(false);

        if (PlayableSequenceManager.Instance != null)
            PlayableSequenceManager.Instance.HideQuestText();

        if (afterCleaningVFX != null)
        {
            afterCleaningVFX.SetActive(false);
            afterCleaningVFX.SetActive(true);
        }

        if (cashBundle != null) cashBundle.Activate();

        if (CafeManager.Instance != null)
            CafeManager.Instance.OnTableCleaned();
    }

    private void ForceRotation(float x, float y, float z)
    {
        if (patient == null) return;
        Quaternion q = Quaternion.Euler(x, y, z);
        patient.transform.rotation = q;
        patient.transform.localRotation = q;
    }

    private IEnumerator MovePatient(Transform target, bool lockRotation = false)
    {
        if (target == null || patient == null) yield break;
        Vector3 targetPos = target.position;
        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetWalk();
        yield return MoveTo(targetPos, lockRotation);
    }

    private IEnumerator MovePatientWithHolding(Transform target)
    {
        if (target == null || patient == null) yield break;
        Vector3 targetPos = target.position;
        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetWalkWithHolding();
        yield return MoveTo(targetPos, false);
    }

    private IEnumerator MoveTo(Vector3 targetPos, bool lockRotation)
    {
        while (true)
        {
            Vector3 diff = targetPos - patient.transform.position;
            diff.y = 0f;
            if (diff.sqrMagnitude <= 0.01f) break;

            patient.transform.position = Vector3.MoveTowards(
                patient.transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (!lockRotation && diff.sqrMagnitude > 0.0001f)
                patient.transform.rotation = Quaternion.Slerp(
                    patient.transform.rotation,
                    Quaternion.LookRotation(diff),
                    rotationSpeed * Time.deltaTime);

            yield return null;
        }
        patient.transform.position = targetPos;
    }
}