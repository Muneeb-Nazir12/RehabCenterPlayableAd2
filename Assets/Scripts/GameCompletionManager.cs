using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameCompletionManager : MonoBehaviour
{
    public static GameCompletionManager Instance { get; private set; }

    [SerializeField] private GameObject completionPanel;
    [SerializeField] private Button installButton;
    [SerializeField] private Button installButton1;

    [Header("Panel Animation")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private float animDuration = 0.35f;

    private bool _gameCompleted;

    private void Awake() => Instance = this;

    private void Start()
    {
        installButton.onClick.RemoveAllListeners();
        installButton.onClick.AddListener(OnInstallClicked);
        installButton1.onClick.RemoveAllListeners();
        installButton1.onClick.AddListener(OnInstallClicked);

        if (panelRect != null)
            panelRect.anchoredPosition = new Vector2(-Screen.width, panelRect.anchoredPosition.y);
    }

    public void TriggerCompletion()
    {
        if (_gameCompleted) return;
        _gameCompleted = true;

        completionPanel.SetActive(true);

        if (panelRect != null)
            StartCoroutine(ScaleIn());
        else
            FinishCompletion();
    }

    private IEnumerator ScaleIn()
    {
        float elapsed = 0f;
        float screenWidth = Screen.width;

        panelRect.anchoredPosition = new Vector2(-screenWidth, panelRect.anchoredPosition.y);

        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);
            float smooth = Mathf.Sin(t * Mathf.PI * 0.5f);
            panelRect.anchoredPosition = new Vector2(
                Mathf.Lerp(-screenWidth, 0f, smooth),
                panelRect.anchoredPosition.y
            );
            yield return null;
        }

        panelRect.anchoredPosition = new Vector2(0f, panelRect.anchoredPosition.y);
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