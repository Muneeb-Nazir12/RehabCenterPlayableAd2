using System.Collections;
using UnityEngine;

public class ReceptionPatientController : MonoBehaviour
{
    public static ReceptionPatientController Instance;

    [Header("Animation")]
    [SerializeField] private PatientAnimationController patientAnim;

    [Header("Patient Mover")]
    [SerializeField] private PatientMover mover;

    [Header("Path Points")]
    [SerializeField] private Transform counterStandPoint;
    [SerializeField] private Transform afterServedPoint1;
    [SerializeField] private Transform afterServedPoint2;
    [SerializeField] private Transform afterServedPoint3;

    [Header("Arrow Targets")]
    [SerializeField] private Transform receptionArrowTarget;
    [SerializeField] private Transform roomUnlockArrowTarget;

    [Header("Room Unlock")]
    [SerializeField] private GameObject roomUnlockObject;

    [Header("Head Indicator")]
    [SerializeField] private GameObject headIndicatorObjectBeforeReceptionist;
    [SerializeField] private GameObject headIndicatorObjectAFterReceptionist;

    public bool IsPatientAtReception { get; private set; }
    public bool IsServed { get; private set; }
    public bool IsFlowComplete { get; private set; }

    private Transform[] _afterServedPath;

    private void Awake()
    {
        Instance = this;
        _afterServedPath = new Transform[] { afterServedPoint1, afterServedPoint2, afterServedPoint3 };
    }

    private void Start() => StartCoroutine(ReceptionFlow());

    private IEnumerator ReceptionFlow()
    {
        while (!GameIntroManager.IntroComplete) yield return null;

        PlayableSequenceManager.Instance?.ShowQuestText("Serve patient at reception");

        if (headIndicatorObjectBeforeReceptionist != null)
            headIndicatorObjectBeforeReceptionist.SetActive(true);

        if (mover != null && counterStandPoint != null)
        {
            mover.MoveTo(counterStandPoint);
            while (mover.IsMoving) yield return null;
        }

        if (headIndicatorObjectBeforeReceptionist != null)
            headIndicatorObjectBeforeReceptionist.SetActive(false);

        ArrowManager.Instance?.PointArrowTowards(receptionArrowTarget);
        if (patientAnim != null) patientAnim.SetIdle();
        IsPatientAtReception = true;
        while (!IsServed) yield return null;
        IsPatientAtReception = false;

        if (headIndicatorObjectAFterReceptionist != null)
            headIndicatorObjectAFterReceptionist.SetActive(true);

        if (roomUnlockObject != null) roomUnlockObject.SetActive(true);
        OnPatientServed();
        ArrowManager.Instance?.PointArrowTowards(roomUnlockArrowTarget);

        if (mover != null && _afterServedPath != null)
        {
            mover.MovePath(_afterServedPath);
            while (mover.IsMoving) yield return null;
        }

        if (headIndicatorObjectAFterReceptionist != null)
            headIndicatorObjectAFterReceptionist.SetActive(false);

        if (patientAnim != null) patientAnim.SetIdle();
        IsFlowComplete = true;
    }

    public void OnPatientServed()
    {
        PlayableSequenceManager.Instance?.ShowQuestText("Unlock the room");
        TargetArrowIndicator.GoToTarget(1);
    }

    public void OnServed() => IsServed = true;
}