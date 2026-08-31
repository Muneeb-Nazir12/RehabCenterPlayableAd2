using UnityEngine;

public class CafeManager : MonoBehaviour
{
    public static CafeManager Instance;

    [Header("Counter Arrow")]
    [SerializeField] private Transform counterArrowTarget;

    [Header("Cash Arrow")]
    [SerializeField] private Transform cashArrowTarget;

    [Header("Next Unlock")]
    [SerializeField] private GameObject washroomUnlockPointObject;
    [SerializeField] private GameObject washroomUnlockPointCanvas;
    [SerializeField] private Transform washRoomUnlockArrowTarget;

    [Header("Reception Serving UI")]
    [SerializeField] private GameObject receptionServingPanel;

    public bool CounterServed { get; private set; }

    private void Awake() => Instance = this;

    public void OnCafeUnlocked()
    {
        CounterServed = false;
        if (receptionServingPanel != null) receptionServingPanel.SetActive(true);
        if (PlayableSequenceManager.Instance != null)
        {
            PlayableSequenceManager.Instance.HideQuestText();
            PlayableSequenceManager.Instance.ShowKitchenFeedHim();
        }
        TargetArrowIndicator.GoToTarget(4);
        if (ArrowManager.Instance != null) ArrowManager.Instance.PointArrowTowards(counterArrowTarget);
        if (CafePatientController.Instance != null) CafePatientController.Instance.StartPatientFlow();
    }

    public void OnPlayerServedCounter()
    {
        TargetArrowIndicator.Hide(4);
        CounterServed = true;
    }

    public void OnTableBecameDirty()
    {
        if (PlayableSequenceManager.Instance != null)
        {
            PlayableSequenceManager.Instance.HideKitchenFeedHim();
            PlayableSequenceManager.Instance.ShowQuestText("Clean the table");
        }
        TargetArrowIndicator.GoToTarget(8);
    }

    public void OnTableCleaned()
    {
        if (ArrowManager.Instance != null) ArrowManager.Instance.PointArrowTowards(cashArrowTarget);
        if (PlayableSequenceManager.Instance != null) PlayableSequenceManager.Instance.ShowQuestText("Pick up cash");
    }

    public void OnCashCollected()
    {
        if (washroomUnlockPointObject != null) washroomUnlockPointObject.SetActive(true);
        if (washroomUnlockPointCanvas != null) washroomUnlockPointCanvas.SetActive(true);

        TargetArrowIndicator.GoToTarget(5);
        if (ArrowManager.Instance != null) ArrowManager.Instance.PointArrowTowards(washRoomUnlockArrowTarget);
        if (PlayableSequenceManager.Instance != null) PlayableSequenceManager.Instance.ShowQuestText("Unlock Shower");
    }
}