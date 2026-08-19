using System.Collections;
using UnityEngine;

public class ShowerManager : MonoBehaviour
{
    public static ShowerManager Instance { get; private set; }

    [Header("Patient")]
    [SerializeField] private Transform patient;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Path Points — Lobby Wait Before Shower Unlock")] 
    [SerializeField] private Transform lobbyWaitPoint;

    [Header("Path Points — To Shower")]
    [SerializeField] private Transform showerPoint1;
    [SerializeField] private Transform showerPoint2;

    [Header("Path Points — To Gym Unlock")]
    [SerializeField] private Transform exitPoint1;
    [SerializeField] private Transform exitPoint2;

    [Header("Shower VFX and UI")]
    [SerializeField] private GameObject showerVFX;
    [SerializeField] private GameObject showerUIPanel;   

    [Header("Arrow Targets")]
    [SerializeField] private Transform towelPickupArrowTarget;
    [SerializeField] private Transform showerDeliveryArrowTarget;

    [Header("Player")]
    [SerializeField] private GameObject playerHandTowel;

    [Header("Towel Pickup Trigger")]
    [SerializeField] private GameObject towelPickupTriggerObject;

    [Header("Cash and Next Unlock")]
    [SerializeField] private CashBundle cashBundle;         
    [SerializeField] private Transform  cashArrowTarget;
    [SerializeField] private GameObject nextBuildingUnlockPoint;

    public bool IsTowelPickedUp   { get; private set; }
    public bool IsShowerCompleted { get; private set; }

    private static readonly WaitForSeconds WaitShowerVFX = new WaitForSeconds(1f);

    private void Awake()
    {
        Instance = this;
        // Make sure towel trigger starts disabled
        if (towelPickupTriggerObject != null) towelPickupTriggerObject.SetActive(false);
    }

    public void OnShowerUnlocked() => StartCoroutine(ShowerSequence());

    private IEnumerator ShowerSequence()
    {
        IsShowerCompleted = false;
        IsTowelPickedUp   = false;

        // If patient should wait at lobby until shower unlocks, move there and rotate
        if (lobbyWaitPoint != null)
        {
            yield return MovePatient(lobbyWaitPoint);
            // FIX: Force rotate to 0,90,0 while waiting at lobby for shower unlock
            ForceRotation(0f, 90f, 0f);
            if (PatientAnimationController.Instance != null)
                PatientAnimationController.Instance.SetIdle();
        }

        yield return MovePatient(showerPoint1);
        yield return MovePatient(showerPoint2);

        // FIX: Force rotate to 0,0,0 while taking shower
        ForceRotation(0f, 0f, 0f);

        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetIdle();

        if (showerVFX != null) showerVFX.SetActive(true);
        yield return WaitShowerVFX;
        if (showerVFX != null) showerVFX.SetActive(false);

        // Show shower UI header
        if (showerUIPanel != null) showerUIPanel.SetActive(true);

        if (PlayableSequenceManager.Instance != null)
            PlayableSequenceManager.Instance.ShowQuestText("Pick up the towel");

        if (ArrowManager.Instance != null && towelPickupArrowTarget != null)
            ArrowManager.Instance.PointArrowTowards(towelPickupArrowTarget);

        // Enable the towel pickup trigger only now
        if (towelPickupTriggerObject != null) towelPickupTriggerObject.SetActive(true);
    }

    public void OnTowelPickedUp()
    {
        if (IsTowelPickedUp) return;
        IsTowelPickedUp = true;

        // FIX: Activate player hand towel gameobject
        if (playerHandTowel != null) playerHandTowel.SetActive(true);

        if (PlayableSequenceManager.Instance != null)
            PlayableSequenceManager.Instance.ShowQuestText("Deliver the towel");

        if (ArrowManager.Instance != null && showerDeliveryArrowTarget != null)
            ArrowManager.Instance.PointArrowTowards(showerDeliveryArrowTarget);
    }

    public void OnTowelDelivered()
    {
        if (IsShowerCompleted) return;
        IsShowerCompleted = true;

        if (playerHandTowel != null) playerHandTowel.SetActive(false);

        if (showerUIPanel != null) showerUIPanel.SetActive(false);

        if (PlayableSequenceManager.Instance != null)
            PlayableSequenceManager.Instance.HideQuestText();

        cashBundle.Activate();

        if (ArrowManager.Instance != null && cashArrowTarget != null)
            ArrowManager.Instance.PointArrowTowards(cashArrowTarget);

        if (PlayableSequenceManager.Instance != null)
            PlayableSequenceManager.Instance.ShowQuestText("Pick up cash");

        StartCoroutine(PatientExitSequence());
    }

    private IEnumerator PatientExitSequence()
    {
        yield return MovePatient(exitPoint1);
        yield return MovePatient(exitPoint2);

        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetIdle();

        // Show next unlock point (gym)
        if (nextBuildingUnlockPoint != null) nextBuildingUnlockPoint.SetActive(true);
    }

    // Called by CashBundle when shower cash is collected
    public void OnShowerCashCollected()
    {
        if (PlayableSequenceManager.Instance != null)
            PlayableSequenceManager.Instance.ShowQuestText("Unlock Gym");
    }

    private void ForceRotation(float x, float y, float z)
    {
        if (patient == null) return;
        Quaternion q = Quaternion.Euler(x, y, z);
        patient.rotation = q;
        patient.localRotation = q;
    }

    private IEnumerator MovePatient(Transform target)
    {
        if (target == null || patient == null) yield break;

        Vector3 targetPos = target.position;

        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetWalk();

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
