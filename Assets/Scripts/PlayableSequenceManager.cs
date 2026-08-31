using UnityEngine;
using UnityEngine.UI;

public class PlayableSequenceManager : MonoBehaviour
{
    public static PlayableSequenceManager Instance { get; private set; }

    [Header("Portrait Panels")]
    [SerializeField] private GameObject portraitQuestPanel;
    [SerializeField] private Text portraitQuestText;

    [Header("Landscape Panels")]
    [SerializeField] private GameObject landscapeQuestPanel;
    [SerializeField] private Text landscapeQuestText;

    [Header("He Needs Shower — Portrait / Landscape")]
    [SerializeField] private GameObject heNeedsShowerPortrait;
    [SerializeField] private GameObject heNeedsShowerLandscape;

    [Header("Gym Final Recovery — Portrait / Landscape")]
    [SerializeField] private GameObject gymFinalRecoveryPortrait;
    [SerializeField] private GameObject gymFinalRecoveryLandscape;

    [Header("Kitchen Feed Him — Portrait / Landscape")]
    [SerializeField] private GameObject kitchenFeedHimPortrait;
    [SerializeField] private GameObject kitchenFeedHimLandscape;

    private string _currentQuestText = "";
    private bool _isShowing = false;
    private int _lastWidth, _lastHeight;
    private bool _isLandscape;

    private void Awake()
    {
        Instance = this;
        _lastWidth = Screen.width;
        _lastHeight = Screen.height;
        _isLandscape = _lastWidth > _lastHeight;
    }

    private void Update()
    {
        int sw = Screen.width;
        int sh = Screen.height;
        if (sw == _lastWidth && sh == _lastHeight) return;

        _lastWidth = sw;
        _lastHeight = sh;
        _isLandscape = sw > sh;

        if (_isShowing) RefreshQuestPanels();
        RefreshDualObjects();
    }

    private static void SetGoActive(GameObject go, bool state)
    {
        if (go != null && go.activeSelf != state) go.SetActive(state);
    }

    private void RefreshQuestPanels()
    {
        if (_isLandscape)
        {
            SetGoActive(portraitQuestPanel, false);
            if (landscapeQuestText != null) landscapeQuestText.text = _currentQuestText;
            SetGoActive(landscapeQuestPanel, true);
        }
        else
        {
            SetGoActive(landscapeQuestPanel, false);
            if (portraitQuestText != null) portraitQuestText.text = _currentQuestText;
            SetGoActive(portraitQuestPanel, true);
        }
    }

    public void ShowQuestText(string text)
    {
        _currentQuestText = text;
        _isShowing = true;
        RefreshQuestPanels();
    }

    public void HideQuestText()
    {
        _isShowing = false;
        _currentQuestText = "";
        SetGoActive(portraitQuestPanel, false);
        SetGoActive(landscapeQuestPanel, false);
    }

    private void RefreshDualObjects()
    {
        SyncDualObject(heNeedsShowerPortrait, heNeedsShowerLandscape);
        SyncDualObject(gymFinalRecoveryPortrait, gymFinalRecoveryLandscape);
        SyncDualObject(kitchenFeedHimPortrait, kitchenFeedHimLandscape);
    }

    private void SyncDualObject(GameObject portrait, GameObject landscape)
    {
        if (portrait == null || landscape == null) return;
        bool isActive = portrait.activeSelf || landscape.activeSelf;
        SetGoActive(portrait, isActive && !_isLandscape);
        SetGoActive(landscape, isActive && _isLandscape);
    }

    public void ShowHeNeedsShower()
    {
        SetGoActive(heNeedsShowerPortrait, !_isLandscape);
        SetGoActive(heNeedsShowerLandscape, _isLandscape);
    }

    public void HideHeNeedsShower()
    {
        SetGoActive(heNeedsShowerPortrait, false);
        SetGoActive(heNeedsShowerLandscape, false);
    }

    public void ShowGymFinalRecovery()
    {
        SetGoActive(gymFinalRecoveryPortrait, !_isLandscape);
        SetGoActive(gymFinalRecoveryLandscape, _isLandscape);
    }

    public void HideGymFinalRecovery()
    {
        SetGoActive(gymFinalRecoveryPortrait, false);
        SetGoActive(gymFinalRecoveryLandscape, false);
    }

    public void ShowKitchenFeedHim()
    {
        SetGoActive(kitchenFeedHimPortrait, !_isLandscape);
        SetGoActive(kitchenFeedHimLandscape, _isLandscape);
    }

    public void HideKitchenFeedHim()
    {
        SetGoActive(kitchenFeedHimPortrait, false);
        SetGoActive(kitchenFeedHimLandscape, false);
    }
}