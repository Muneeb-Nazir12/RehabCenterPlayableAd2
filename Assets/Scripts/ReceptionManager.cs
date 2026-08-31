using UnityEngine;
using UnityEngine.UI;

public class ReceptionManager : MonoBehaviour
{
    public static ReceptionManager Instance;

    [Header("Fill UI")]
    [SerializeField] private Image fillImage;
    [SerializeField] private float servingDuration = 0.5f;
    [SerializeField] private GameObject whiteCircle;
    [SerializeField] private GameObject greenCircle;

    [Header("Player")]
    [SerializeField] private GameObject player;
    [SerializeField] private Collider unlockTrigger;

    private bool _playerInside;
    private bool _serving;
    private bool _alreadyServed;
    private float _elapsed;
    private float _invDuration;

    private void Awake()
    {
        Instance = this;
        _invDuration = servingDuration > 0f ? 1f / servingDuration : 2f;
        if (fillImage != null) fillImage.fillAmount = 0f;
        enabled = false;
    }

    private void Update()
    {
        if (_alreadyServed)
        {
            enabled = false;
            return;
        }
        if (!_playerInside || _serving)
        {
            enabled = false;
            return;
        }
        if (ReceptionPatientController.Instance == null ||
            !ReceptionPatientController.Instance.IsPatientAtReception) return;

        _elapsed += Time.deltaTime;

        if (fillImage != null) fillImage.fillAmount = Mathf.Clamp01(_elapsed * _invDuration);

        if (_elapsed >= servingDuration)
        {
            _serving = true;
            _alreadyServed = true;
            if (fillImage != null) fillImage.fillAmount = 0f;
            enabled = false;
            FinishServing();
        }
    }

    public void OnReceptionUnlocked()
    {
        if (unlockTrigger != null) unlockTrigger.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (player != null && other.gameObject != player) return;
        if (whiteCircle != null && whiteCircle.activeSelf) whiteCircle.SetActive(false);
        if (greenCircle != null && !greenCircle.activeSelf) greenCircle.SetActive(true);
        _playerInside = true;
        if (!_alreadyServed) enabled = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null) return;
        if (player != null && other.gameObject != player) return;
        if (whiteCircle != null && !whiteCircle.activeSelf) whiteCircle.SetActive(true);
        if (greenCircle != null && greenCircle.activeSelf) greenCircle.SetActive(false);
        _playerInside = false;
        enabled = false;
        _elapsed = 0f;
        if (!_alreadyServed && fillImage != null) fillImage.fillAmount = 0f;
    }

    private void FinishServing()
    {
        TargetArrowIndicator.Hide(0);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayServingSound();
        if (ReceptionPatientController.Instance != null) ReceptionPatientController.Instance.OnServed();
    }
}