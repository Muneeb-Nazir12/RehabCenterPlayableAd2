using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameCompletionManager : MonoBehaviour
{
    public static GameCompletionManager Instance { get; private set; }

    [SerializeField] private GameObject completionPanel;
    [SerializeField] private Button installButton;
    [SerializeField] private Button installButton1;

    [Header("Portrait / Landscape Images")]
    [SerializeField] private GameObject portraitImage;
    [SerializeField] private GameObject landscapeImage;

    [Header("Panel Animation")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private float animDuration = 2f;

    private bool _gameCompleted;

    private float _panelY;
    private float _screenWidth;
    private int _lastWidth = -1;
    private int _lastHeight = -1;
    private int _lastIsPortrait = -1;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (installButton != null)
        {
            installButton.onClick.RemoveAllListeners();
            installButton.onClick.AddListener(OnInstallClicked);
        }

        if (installButton1 != null)
        {
            installButton1.onClick.RemoveAllListeners();
            installButton1.onClick.AddListener(OnInstallClicked);
        }

        UpdateOrientationImage(true);

        if (panelRect != null)
        {
            _panelY = panelRect.anchoredPosition.y;
            _screenWidth = Screen.width;

            panelRect.anchoredPosition = new Vector2(_screenWidth, _panelY);
        }
    }

    private void Update()
    {
        int sw = Screen.width;
        int sh = Screen.height;

        if (sw != _lastWidth || sh != _lastHeight)
        {
            _lastWidth = sw;
            _lastHeight = sh;

            UpdateOrientationImage(false);
        }
    }

    private void UpdateOrientationImage(bool force)
    {
        bool isPortrait = Screen.height > Screen.width;
        int portInt = isPortrait ? 1 : 0;

        if (!force && portInt == _lastIsPortrait)
            return;

        _lastIsPortrait = portInt;

        if (portraitImage != null && portraitImage.activeSelf != isPortrait)
            portraitImage.SetActive(isPortrait);

        if (landscapeImage != null && landscapeImage.activeSelf == isPortrait)
            landscapeImage.SetActive(!isPortrait);
    }

    public void TriggerCompletion()
    {
        if (_gameCompleted)
            return;

        _gameCompleted = true;

        if (completionPanel != null)
            completionPanel.SetActive(true);

        UpdateOrientationImage(true);

        if (panelRect != null)
            StartCoroutine(ScaleIn());
        else
            FinishCompletion();
    }

    private IEnumerator ScaleIn()
    {
        float elapsed = 0f;

        _screenWidth = Screen.width;

        panelRect.anchoredPosition = new Vector2(_screenWidth, _panelY);

        float invDur = animDuration > 0f ? 1f / animDuration : 1f;

        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed * invDur);

            float smooth = Mathf.Sin(t * Mathf.PI * 0.5f);

            panelRect.anchoredPosition = new Vector2(
                Mathf.Lerp(_screenWidth, 0f, smooth),
                _panelY
            );

            yield return null;
        }

        panelRect.anchoredPosition = new Vector2(0f, _panelY);

        FinishCompletion();
    }

    private void FinishCompletion()
    {
        if (PlayableDynamicJoystick.Instance != null)
            PlayableDynamicJoystick.Instance.OnGameEnd();

        Luna.Unity.LifeCycle.GameEnded();

        Time.timeScale = 0f;
    }

    public void OnInstallClicked()
    {
        Luna.Unity.Playable.InstallFullGame();
    }
}