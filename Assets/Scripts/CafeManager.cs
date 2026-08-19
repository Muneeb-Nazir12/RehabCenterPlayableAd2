using UnityEngine;
using UnityEngine.UI;

public class CafeManager : MonoBehaviour
{
    public static CafeManager Instance;

    [Header("Counter Arrow")]
    [SerializeField] private Transform counterArrowTarget;

    [Header("Cash Arrow")]
    [SerializeField] private Transform cashArrowTarget;

    [Header("Next Unlock")]
    [SerializeField] private GameObject nextUnlockPointObject;
    [SerializeField] private Transform nextUnlockArrowTarget;

    [Header("Reception Serving UI")]
    [SerializeField] private GameObject receptionServingPanel;

    public bool CounterServed { get; private set; }

    private void Awake() => Instance = this;

    public void OnCafeUnlocked()
    {
        CounterServed = false;

        if (receptionServingPanel != null) receptionServingPanel.SetActive(true);

        if (PlayableSequenceManager.Instance != null)
            PlayableSequenceManager.Instance.ActivateHeader(
                PlayableSequenceManager.Instance.GetHeaderKitchen());

        if (ArrowManager.Instance != null && counterArrowTarget != null)
            ArrowManager.Instance.PointArrowTowards(counterArrowTarget);

        CafePatientController.Instance.StartPatientFlow();
    }

    public void OnPlayerServedCounter()
    {
        CounterServed = true;

        if (PlayableSequenceManager.Instance != null)
            PlayableSequenceManager.Instance.ActivateHeader(null);

        if (PlayableSequenceManager.Instance != null)
            PlayableSequenceManager.Instance.ShowQuestText("Wait for patient eat food");
    }

    public void OnTableBecameDirty()
    {
        if (PlayableSequenceManager.Instance != null)
            PlayableSequenceManager.Instance.ShowQuestText("Clean the table");
    }

    public void OnTableCleaned()
    {
        if (ArrowManager.Instance != null && cashArrowTarget != null)
            ArrowManager.Instance.PointArrowTowards(cashArrowTarget);

        if (PlayableSequenceManager.Instance != null)
            PlayableSequenceManager.Instance.ShowQuestText("Pick up cash");
    }

    public void OnCashCollected()
    {
        if (nextUnlockPointObject != null) nextUnlockPointObject.SetActive(true);

        if (ArrowManager.Instance != null && nextUnlockArrowTarget != null)
            ArrowManager.Instance.PointArrowTowards(nextUnlockArrowTarget);

        if (PlayableSequenceManager.Instance != null)
            PlayableSequenceManager.Instance.ShowQuestText("Unlock Shower");
    }
}