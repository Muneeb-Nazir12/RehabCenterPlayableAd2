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
    [SerializeField] private Transform showerPoint3;

    [Header("Path Points — Exit Washroom")]
    [SerializeField] private Transform exitPoint1;
    [SerializeField] private Transform exitPoint2;
    [SerializeField] private Transform exitPoint3;
    [SerializeField] private Transform exitPoint4;

    [Header("Shower VFX and UI")]
    [SerializeField] private GameObject showerVFX;
    [SerializeField] private GameObject showerUIPanel;
    [SerializeField] private GameObject showerGameObject;

    [Header("Arrow Targets")]
    [SerializeField] private Transform showerRoomArrowTarget;
    [SerializeField] private Transform towelPickupArrowTarget;
    [SerializeField] private Transform showerDeliveryArrowTarget;
    [SerializeField] private Transform cashArrowTarget;
    [SerializeField] private Transform gymUnlockArrowTarget;

    [Header("Player")]
    [SerializeField] private GameObject playerHandTowel;

    [Header("Towel Pickup Trigger")]
    [SerializeField] private GameObject towelPickupTriggerObject;
    [SerializeField] private GameObject towelPickupGreenCircle;

    [Header("Cash and Next Unlock")]
    [SerializeField] private CashBundle cashBundle;
    [SerializeField] private GameObject nextBuildingUnlockPoint;

    [Header("Patient Head UI — Shower")]
    [Tooltip("Shown while the patient walks to the shower. Hidden once the patient is inside showering.")]
    [SerializeField] private GameObject showerHeadUI;

    [Header("Patient Head UI — Post Shower")]
    [Tooltip("Shown once the patient has finished showering and is idle, waiting for the gym to be unlocked.")]
    [SerializeField] private GameObject postShowerHeadUI;

    public bool IsTowelPickedUp { get; private set; }
    public bool IsShowerCompleted { get; private set; }

    private static readonly WaitForSeconds WaitShowerVFX = new WaitForSeconds(2f);

    private void Awake()
    {
        Instance = this;
        if (towelPickupTriggerObject != null) towelPickupTriggerObject.SetActive(false);
        if (towelPickupGreenCircle != null) towelPickupGreenCircle.SetActive(false);
    }

    public void OnShowerUnlocked() => StartCoroutine(ShowerSequence());

    private IEnumerator ShowerSequence()
    {
        // Stop the cafe coroutine FIRST — prevents both coroutines from
        // fighting over patient.position / patient.rotation simultaneously.
        CafePatientController.Instance?.StopPatientFlow();

        IsShowerCompleted = false;
        IsTowelPickedUp = false;

        CafePatientController.Instance?.HidePostCafeHeadUI();

        if (showerHeadUI != null) showerHeadUI.SetActive(true);
        ArrowManager.Instance?.PointArrowTowards(showerRoomArrowTarget);

        yield return MovePatient(showerPoint1);
        yield return MovePatient(showerPoint2);
        yield return MovePatient(showerPoint3);

        PatientAnimationController.Instance?.SetIdle();

        if (showerHeadUI != null) showerHeadUI.SetActive(false);
        PlayableSequenceManager.Instance?.HideQuestText();

        if (showerGameObject != null) showerGameObject.SetActive(false);

        if (showerVFX != null) showerVFX.SetActive(true);
        yield return WaitShowerVFX;
        if (showerVFX != null) showerVFX.SetActive(false);

        PlayableSequenceManager.Instance?.ShowQuestText("Pick up the towel");
        ArrowManager.Instance?.PointArrowTowards(towelPickupArrowTarget);
        if (towelPickupTriggerObject != null) towelPickupTriggerObject.SetActive(true);

        PatientAnimationController.Instance?.SetWalk();

        yield return MovePatient(exitPoint1);
        yield return MovePatient(exitPoint2);
        yield return MovePatient(exitPoint3);
        yield return MovePatient(exitPoint4);

        PatientAnimationController.Instance?.SetIdle();

        // Patient has exited the shower and is idle — show the post-shower head UI
        // so the player knows the patient is waiting for the gym to be unlocked.
        if (postShowerHeadUI != null) postShowerHeadUI.SetActive(true);

        if (nextBuildingUnlockPoint != null) nextBuildingUnlockPoint.SetActive(true);
    }

    /// <summary>
    /// Called by GymManager when the gym is unlocked, hiding the post-shower head UI.
    /// </summary>
    public void HidePostShowerHeadUI()
    {
        if (postShowerHeadUI != null) postShowerHeadUI.SetActive(false);
    }

    public void OnTowelPickedUp()
    {
        if (IsTowelPickedUp) return;
        IsTowelPickedUp = true;

        if (playerHandTowel != null) playerHandTowel.SetActive(true);
        if (towelPickupGreenCircle != null) towelPickupGreenCircle.SetActive(false);
        if (towelPickupTriggerObject != null) towelPickupTriggerObject.SetActive(false);

        PlayableSequenceManager.Instance?.ShowQuestText("Deliver the towel");
        ArrowManager.Instance?.PointArrowTowards(showerDeliveryArrowTarget);
    }

    public void OnTowelDelivered()
    {
        if (IsShowerCompleted) return;
        IsShowerCompleted = true;

        if (playerHandTowel != null) playerHandTowel.SetActive(false);
        if (showerUIPanel != null) showerUIPanel.SetActive(false);

        PlayableSequenceManager.Instance?.HideQuestText();
        AudioManager.Instance?.PlayCleaningSound();

        cashBundle?.Activate();

        ArrowManager.Instance?.PointArrowTowards(cashArrowTarget);
        PlayableSequenceManager.Instance?.ShowQuestText("Pick up cash");
    }

    public void OnShowerCashCollected()
    {
        PlayableSequenceManager.Instance?.ShowQuestText("Unlock Gym");
        ArrowManager.Instance?.PointArrowTowards(gymUnlockArrowTarget);
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