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
    
    [Header("Quest Panel")]
    [SerializeField] private GameObject questPanel;
    [SerializeField] private Text       questText;

    public SequenceStep CurrentStep { get; private set; } = SequenceStep.IntroHook;

    public GameObject GetHeaderKitchen() => headerKitchenFeedHim;

    private void Awake() => Instance = this; 

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
                ActivateHeader(headerKitchenFeedHim);
                HideQuestText();
                break;

            case SequenceStep.Shower:
                ActivateHeader(headerHeNeedsShower);
                HideQuestText();
                break;

            case SequenceStep.Gym:
                ActivateHeader(headerGymFinalRecovery);
                HideQuestText();
                break;

            case SequenceStep.FullyRecovered:
                ActivateHeader(headerFullyRecovered);
                ShowQuestText("Fully Recovered!");
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
}
