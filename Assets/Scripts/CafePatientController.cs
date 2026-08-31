using System.Collections;
using UnityEngine;

public class CafePatientController : MonoBehaviour
{
    public static CafePatientController Instance;

    [Header("Patient Mover")]
    [SerializeField] private PatientMover mover;

    [Header("Patient")]
    [SerializeField] private GameObject patient;

    [Header("Patient Objects")]
    [SerializeField] private GameObject patientHandTray;
    [SerializeField] private GameObject chairTray;
    [SerializeField] private GameObject dirtyBurgerPlate;

    [Header("VFX")]
    [SerializeField] private GameObject sitVFX;
    [SerializeField] private GameObject afterEatingVFX;

    [Header("Mess UI")]
    [SerializeField] private GameObject messTableUI;

    [Header("Cash")]
    [SerializeField] private CashBundle cashBundle;

    [Header("Path Points")]
    [SerializeField] private Transform counterPoint;
    [SerializeField] private Transform counterPoint1;
    [SerializeField] private Transform chairPoint;
    [SerializeField] private Transform afterSitPoint1;
    [SerializeField] private Transform afterSitPoint2;
    [SerializeField] private Transform afterSitPoint3;
    [SerializeField] private Transform afterSitPoint4;

    [Header("Arrow Targets")]
    [SerializeField] private Transform chairArrowTarget;
    [SerializeField] private Transform tableArrowTarget;

    [Header("Shared Particle")]
    [SerializeField] private ParticleSystem sharedVFX;
    [SerializeField] private Transform cleaningVFXPoint;

    public bool IsPatientAtCounter { get; private set; }
    public bool IsTableDirty { get; private set; }
    public bool IsFlowComplete { get; private set; }

    public GameObject showerUI;

    private static readonly Quaternion Rot_0_90_0 = Quaternion.Euler(0f, 90f, 0f);

    private Transform _patientT;
    private Coroutine _patientFlowCoroutine;
    private Transform[] _toCounterPath;
    private Transform[] _afterSitPath;

    private void Awake()
    {
        Instance = this;
        IsTableDirty = false;
        IsFlowComplete = false;
        if (patient != null) _patientT = patient.transform;
        _toCounterPath = new Transform[] { counterPoint, counterPoint1 };
        _afterSitPath = new Transform[] { afterSitPoint1, afterSitPoint2, afterSitPoint3, afterSitPoint4 };
    }

    private void PlayVFX(GameObject vfx)
    {
        if (vfx == null) return;
        vfx.SetActive(false);
        vfx.SetActive(true);
    }

    public void StartPatientFlow()
    {
        IsFlowComplete = false;
        IsTableDirty = false;
        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.ResetAnimator();

        if (_patientFlowCoroutine != null)
        {
            StopCoroutine(_patientFlowCoroutine);
            _patientFlowCoroutine = null;
        }
        _patientFlowCoroutine = StartCoroutine(PatientFlow());
    }

    public void StopPatientFlow()
    {
        if (_patientFlowCoroutine != null)
        {
            StopCoroutine(_patientFlowCoroutine);
            _patientFlowCoroutine = null;
        }
        if (mover != null) mover.Stop();
    }

    private IEnumerator PatientFlow()
    {
        IsPatientAtCounter = false;
        IsFlowComplete = false;

        if (counterPoint == null || _patientT == null) yield break;

        while (!RoomPatientController.IsFlowComplete) yield return null;

        if (mover != null && _toCounterPath != null)
        {
            mover.MovePath(_toCounterPath);
            while (mover.IsMoving) yield return null;
        }

        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetIdle();
        IsPatientAtCounter = true;

        while (CafeManager.Instance == null || !CafeManager.Instance.CounterServed)
            yield return null;

        IsPatientAtCounter = false;
        if (RoomPatientController.Instance != null && RoomPatientController.Instance.foodUI != null)
            RoomPatientController.Instance.foodUI.SetActive(false);
        if (patientHandTray != null) patientHandTray.SetActive(true);


        if (mover != null && chairPoint != null)
        {
            mover.MoveTo(chairPoint, holdingTray: true);
            while (mover.IsMoving) yield return null;
        }

        if (patientHandTray != null) patientHandTray.SetActive(false);
        if (chairTray != null) chairTray.SetActive(true);

        Vector3 sitPos = chairPoint.position;
        Quaternion sitRot = Rot_0_90_0;

        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetSitting();
        PlayableSequenceManager.Instance?.HideQuestText();

        float sitElapsed = 0f;
        const float sitDuration = 1.5f;
        while (sitElapsed < sitDuration)
        {
            _patientT.SetPositionAndRotation(sitPos, sitRot);
            sitElapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        ArrowManager.Instance?.PointArrowTowards(chairArrowTarget);

        if (sitVFX != null) sitVFX.SetActive(true);
        float getUpElapsed = 0f;
        const float getUpDuration = 0.5f;
        while (getUpElapsed < getUpDuration)
        {
            _patientT.SetPositionAndRotation(sitPos, sitRot);
            getUpElapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        if (sitVFX != null) sitVFX.SetActive(false);

        if (chairTray != null) chairTray.SetActive(false);
        if (dirtyBurgerPlate != null) dirtyBurgerPlate.SetActive(true);
        if (messTableUI != null) messTableUI.SetActive(true);
        IsTableDirty = true;

        PlayVFX(afterEatingVFX);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayEffectSound();

        if (CafeManager.Instance != null) CafeManager.Instance.OnTableBecameDirty();
        ArrowManager.Instance?.PointArrowTowards(tableArrowTarget);
        if (showerUI != null) showerUI.SetActive(true);

        if (mover != null && _afterSitPath != null)
        {
            mover.MovePath(_afterSitPath);
            while (mover.IsMoving) yield return null;
        }

        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetIdle();

        IsFlowComplete = true;
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
        PlayVFXAt(cleaningVFXPoint);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCleaningSound();
        if (cashBundle != null) cashBundle.Activate();
        if (CafeManager.Instance != null) CafeManager.Instance.OnTableCleaned();
    }

    private void PlayVFXAt(Transform point)
    {
        if (sharedVFX == null || point == null) return;
        sharedVFX.transform.position = point.position;
        sharedVFX.Play();
    }
}