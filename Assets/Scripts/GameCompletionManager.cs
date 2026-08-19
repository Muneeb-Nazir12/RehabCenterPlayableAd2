using UnityEngine;
using UnityEngine.UI;

public class GameCompletionManager : MonoBehaviour
{
    public static GameCompletionManager Instance { get; private set; }

    [SerializeField] private GameObject completionPanel;
    [SerializeField] private Button installButton;

    private bool _gameCompleted;

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
    }

    public void TriggerCompletion()
    {
        if (_gameCompleted) return;
        _gameCompleted = true;

        if (completionPanel != null) completionPanel.SetActive(true);
        if (PlayableDynamicJoystick.Instance != null) PlayableDynamicJoystick.Instance.OnGameEnd();

        Luna.Unity.LifeCycle.GameEnded();
        Time.timeScale = 0f;
    }

    public void OnInstallClicked()
    {
        Luna.Unity.Playable.InstallFullGame();
    }
}
