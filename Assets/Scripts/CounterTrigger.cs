using UnityEngine;
using UnityEngine.UI;

public class CounterTrigger : MonoBehaviour
{
    [Header("Circles")]
    [SerializeField] private GameObject greenCircle;
    [SerializeField] private GameObject whiteCircle;

    [Header("Fill")]
    [SerializeField] private Image fillImage;
    [SerializeField] private float serviceDuration = 0.5f;
    [SerializeField] private GameObject player;

    private bool _playerInside;
    private bool _served;
    private float _elapsed;
    private float _invDuration;

    private void Awake()
    {
        _invDuration = serviceDuration > 0f ? 1f / serviceDuration : 2f;
        if (fillImage != null) fillImage.fillAmount = 0f;
        if (greenCircle != null) greenCircle.SetActive(false);
        if (whiteCircle != null) whiteCircle.SetActive(true);
        enabled = false;
    }

    private void Update()
    {
        if (!_playerInside || _served)
        {
            enabled = false;
            return;
        }

        bool isPatientReady = CafePatientController.Instance != null && CafePatientController.Instance.IsPatientAtCounter;
        if (!isPatientReady)
        {
            if (_elapsed > 0f)
            {
                _elapsed = 0f;
                if (fillImage != null) fillImage.fillAmount = 0f;
            }
            return;
        }

        _elapsed += Time.deltaTime;
        if (fillImage != null) fillImage.fillAmount = Mathf.Clamp01(_elapsed * _invDuration);

        if (_elapsed >= serviceDuration)
        {
            _served = true;
            enabled = false;
            if (fillImage != null) fillImage.fillAmount = 0f;
            if (greenCircle != null && greenCircle.activeSelf) greenCircle.SetActive(false);
            if (whiteCircle != null && !whiteCircle.activeSelf) whiteCircle.SetActive(true);
            if (ArrowManager.Instance != null) ArrowManager.Instance.HideArrow();
            if (CafeManager.Instance != null) CafeManager.Instance.OnPlayerServedCounter();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_served || other == null || (player != null && other.gameObject != player)) return;
        _playerInside = true;
        enabled = true;
        if (greenCircle != null && !greenCircle.activeSelf) greenCircle.SetActive(true);
        if (whiteCircle != null && whiteCircle.activeSelf) whiteCircle.SetActive(false);
        if (ArrowManager.Instance != null) ArrowManager.Instance.HideArrow();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null || (player != null && other.gameObject != player)) return;
        _playerInside = false;
        enabled = false;
        _elapsed = 0f;
        if (fillImage != null) fillImage.fillAmount = 0f;
        if (greenCircle != null && greenCircle.activeSelf) greenCircle.SetActive(false);
        if (whiteCircle != null && !whiteCircle.activeSelf) whiteCircle.SetActive(true);
    }
}
