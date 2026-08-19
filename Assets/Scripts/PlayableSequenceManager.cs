using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayableSequenceManager : MonoBehaviour
{
    public static PlayableSequenceManager Instance { get; private set; }

    public enum SequenceStep
    {
        IntroHook        = 1,
        Bedroom          = 2,
        Kitchen          = 3,
        Shower           = 4,
        Gym              = 5,
        FullyRecovered   = 6,
        AutomationEndCard = 7
    }

    [Header("Headers")]
    [SerializeField] private GameObject headerHelpHimRecover;
    [SerializeField] private GameObject headerKitchenFeedHim;
    [SerializeField] private GameObject headerHeNeedsShower;
    [SerializeField] private GameObject headerGymFinalRecovery;
    [SerializeField] private GameObject headerFullyRecovered;
    [SerializeField] private GameObject headerHireStaff;

    [Header("Need Icons")]
    [SerializeField] private GameObject needsIconsContainer;
    [SerializeField] private GameObject sleepNeedIcon;
    [SerializeField] private GameObject foodNeedIcon;
    [SerializeField] private GameObject showerNeedIcon;
    [SerializeField] private GameObject gymNeedIcon;

    [Header("Quest Panel")]
    [SerializeField] private GameObject questPanel;
    [SerializeField] private Text       questText;

    [Header("Intro")]
    [SerializeField] private CameraFollower cameraFollower;
    [SerializeField] private Transform      introCameraTarget;
    [SerializeField] private GameObject     introImage;
    [SerializeField] private float          introDuration = 1f;

    [Header("End Card")]
    [SerializeField] private GameObject staffAutomationPanel;
    [SerializeField] private GameObject endCardPanel;
    [SerializeField] private Button     continueButton;

    public SequenceStep CurrentStep { get; private set; } = SequenceStep.IntroHook;

    public GameObject GetHeaderKitchen() => headerKitchenFeedHim;

    private void Awake() => Instance = this;

    private void Start()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueClicked);
        }
        StartCoroutine(IntroHookSequence());
    }

    private IEnumerator IntroHookSequence()
    {
        CurrentStep = SequenceStep.IntroHook;
        ActivateHeader(headerHelpHimRecover);
        if (introImage != null) introImage.SetActive(true);

        if (needsIconsContainer != null) needsIconsContainer.SetActive(true);
        if (sleepNeedIcon  != null) sleepNeedIcon.SetActive(true);
        if (foodNeedIcon   != null) foodNeedIcon.SetActive(true);
        if (showerNeedIcon != null) showerNeedIcon.SetActive(true);
        if (gymNeedIcon    != null) gymNeedIcon.SetActive(true);

        if (CharacterMovement.Instance != null) CharacterMovement.Instance.canMove = false;

        if (cameraFollower != null && introCameraTarget != null)
            cameraFollower.FocusOnBuilding(introCameraTarget);

        yield return new WaitForSeconds(introDuration);

        if (introImage != null) introImage.SetActive(false);
        if (CharacterMovement.Instance != null) CharacterMovement.Instance.canMove = true;

        CurrentStep = SequenceStep.Bedroom;
        ActivateHeader(null);
    }

    public void SetStep(SequenceStep step)
    {
        CurrentStep = step;

        switch (step)
        {
            case SequenceStep.Bedroom:
                ActivateHeader(null);
                ShowQuestText("Unlock the room");
                break;

            case SequenceStep.Kitchen:
                // Show kitchen feed him header; message panel is hidden until needed
                ActivateHeader(headerKitchenFeedHim);
                if (sleepNeedIcon != null) sleepNeedIcon.SetActive(false);
                if (foodNeedIcon  != null) foodNeedIcon.SetActive(true);
                HideQuestText();
                break;

            case SequenceStep.Shower:
                // Show shower header; hide message panel
                ActivateHeader(headerHeNeedsShower);
                if (foodNeedIcon   != null) foodNeedIcon.SetActive(false);
                if (showerNeedIcon != null) showerNeedIcon.SetActive(true);
                HideQuestText();
                break;

            case SequenceStep.Gym:
                // Gym: activate gym final recovery header; hide message panel
                ActivateHeader(headerGymFinalRecovery);
                if (showerNeedIcon != null) showerNeedIcon.SetActive(false);
                if (gymNeedIcon    != null) gymNeedIcon.SetActive(true);
                HideQuestText();
                break;

            case SequenceStep.FullyRecovered:
                ActivateHeader(headerFullyRecovered);
                if (gymNeedIcon         != null) gymNeedIcon.SetActive(false);
                if (needsIconsContainer != null) needsIconsContainer.SetActive(false);
                ShowQuestText("Fully Recovered!");
                break;

            case SequenceStep.AutomationEndCard:
                ActivateHeader(headerHireStaff);
                if (staffAutomationPanel != null) staffAutomationPanel.SetActive(true);
                ShowQuestText("Hire staff. Automate everything.");
                break;
        }
    }

    public void ActivateHeader(GameObject activeHeader)
    {
        if (headerHelpHimRecover   != null) headerHelpHimRecover  .SetActive(headerHelpHimRecover   == activeHeader);
        if (headerKitchenFeedHim   != null) headerKitchenFeedHim  .SetActive(headerKitchenFeedHim   == activeHeader);
        if (headerHeNeedsShower    != null) headerHeNeedsShower   .SetActive(headerHeNeedsShower    == activeHeader);
        if (headerGymFinalRecovery != null) headerGymFinalRecovery.SetActive(headerGymFinalRecovery == activeHeader);
        if (headerFullyRecovered   != null) headerFullyRecovered  .SetActive(headerFullyRecovered   == activeHeader);
        if (headerHireStaff        != null) headerHireStaff       .SetActive(headerHireStaff        == activeHeader);
    }

    public void ShowQuestText(string text)
    {
        if (questPanel != null) questPanel.SetActive(true);
        if (questText  != null) questText.text = text;
    }

    public void HideQuestText()
    {
        if (questPanel != null) questPanel.SetActive(false);
    }

    public void TriggerEndCard()
    {
        if (endCardPanel != null) endCardPanel.SetActive(true);
        if (GameCompletionManager.Instance != null) GameCompletionManager.Instance.TriggerCompletion();
    }

    private void OnContinueClicked() => SetStep(SequenceStep.AutomationEndCard);
}
