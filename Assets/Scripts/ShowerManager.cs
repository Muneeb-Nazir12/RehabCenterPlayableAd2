using System.Collections;
using UnityEngine;

public class ShowerManager : MonoBehaviour
{
    public static ShowerManager Instance { get; private set; }

    [Header("Patient Mover")]
    [SerializeField] private PatientMover mover;
    [SerializeField] private Transform patient;

    [Header("Path Points — To Shower")]
    [SerializeField] private Transform showerPoint1;
    [SerializeField] private Transform showerPoint2;
    [SerializeField] private Transform showerPoint3;

    [Header("Path Points — Exit Washroom")]
    [SerializeField] private Transform exitPoint1;
    [SerializeField] private Transform exitPoint2;
    [SerializeField] private Transform exitPoint3;
    [SerializeField] private Transform exitPoint4;

    [Header("Shower VFX and UI")]
    [SerializeField] private GameObject showerVFX;

    [Header("Arrow Targets")]
    [SerializeField] private Transform showerRoomArrowTarget;
    [SerializeField] private Transform cashArrowTarget;
    [SerializeField] private Transform gymUnlockArrowTarget;

    [Header("Cash and Next Unlock")]
    [SerializeField] private CashBundle cashBundle;

    [Header("Gym Unlock")]
    [SerializeField] private GameObject gymUnlockPointObject;
    [SerializeField] private GameObject gymUnlockObjectCanvas;

    [Header("VFX")]
    [SerializeField] private GameObject cloudVFX;
    [SerializeField] private GameObject afterShowerVFX;

    [Header("Shower Snap Settings")]
    [SerializeField] private float showerSnapY = 0.3f;
    [SerializeField] private float showerVFXDuration = 2f;

    public GameObject gymUI;
    public bool IsShowerCompleted { get; private set; }

    private static readonly Quaternion ShowerSnapRotation = Quaternion.Euler(0f, 0f, 0f);

    private Transform[] _exitPath;

    private void Awake()
    {
        Instance = this;
        _exitPath = new Transform[] { exitPoint1, exitPoint2, exitPoint3, exitPoint4 };
    }

    private void PlayVFX(GameObject vfx)
    {
        if (vfx == null) return;
        vfx.SetActive(false);
        vfx.SetActive(true);
    }

    public void OnShowerUnlocked()
    {
        TargetArrowIndicator.Hide(5);
        StartCoroutine(ShowerSequence());
    }

    private IEnumerator ShowerSequence()
    {
        if (PlayableSequenceManager.Instance != null)
        {
            PlayableSequenceManager.Instance.HideQuestText();
            PlayableSequenceManager.Instance.ShowHeNeedsShower();
        }

        while (CafePatientController.Instance == null
            || !CafePatientController.Instance.IsFlowComplete) yield return null;

        CafePatientController.Instance.StopPatientFlow();
        IsShowerCompleted = false;

        if (mover != null)
        {
            mover.MoveTo(showerPoint1, null);
            while (mover.IsMoving) yield return null;

            mover.MoveTo(showerPoint2, null);
            while (mover.IsMoving) yield return null;

            mover.MoveTo(showerPoint3, null);
            while (mover.IsMoving) yield return null;
        }

        Vector3 lockedPos = Vector3.zero;
        if (patient != null)
        {
            lockedPos = new Vector3(patient.position.x, showerSnapY, patient.position.z);
            patient.SetPositionAndRotation(lockedPos, ShowerSnapRotation);
        }

        if (CafePatientController.Instance != null && CafePatientController.Instance.showerUI != null)
            CafePatientController.Instance.showerUI.SetActive(false);

        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetIdle();

        if (showerVFX != null) showerVFX.SetActive(true);

        float showerElapsed = 0f;
        while (showerElapsed < showerVFXDuration)
        {
            if (patient != null)
                patient.SetPositionAndRotation(lockedPos, ShowerSnapRotation);
            showerElapsed += Time.deltaTime;
            yield return null;
        }

        if (showerVFX != null) showerVFX.SetActive(false);

        if (cloudVFX != null) cloudVFX.SetActive(false);
        PlayVFX(afterShowerVFX);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayEffectSound();

        if (cashBundle != null) cashBundle.Activate();
        ArrowManager.Instance?.PointArrowTowards(cashArrowTarget);
        if (PlayableSequenceManager.Instance != null)
        {
            PlayableSequenceManager.Instance.HideHeNeedsShower();
            PlayableSequenceManager.Instance.ShowQuestText("Collect Cash");
        }

        if (gymUI != null) gymUI.SetActive(true);

        if (mover != null)
        {
            mover.MovePath(_exitPath);
            while (mover.IsMoving) yield return null;
        }

        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetIdle();
        IsShowerCompleted = true;
    }

    public void OnShowerCashCollected()
    {
        if (gymUnlockPointObject != null) gymUnlockPointObject.SetActive(true);
        if (gymUnlockObjectCanvas != null) gymUnlockObjectCanvas.SetActive(true);
        TargetArrowIndicator.GoToTarget(6);
        PlayableSequenceManager.Instance?.ShowQuestText("Unlock Gym");
        ArrowManager.Instance?.PointArrowTowards(gymUnlockArrowTarget);
    }
}