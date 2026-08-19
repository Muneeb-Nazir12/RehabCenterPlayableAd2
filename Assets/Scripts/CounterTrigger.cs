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
        _invDuration = 1f / serviceDuration;
        fillImage.fillAmount = 0f;
        greenCircle.SetActive(false);
        whiteCircle.SetActive(true);
    }

    private void Update()
    {
        if (!_playerInside || _served) return;

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
            if (fillImage != null) fillImage.fillAmount = 1f;
            if (greenCircle != null) greenCircle.SetActive(false);
            if (whiteCircle != null) whiteCircle.SetActive(false);
            if (ArrowManager.Instance != null) ArrowManager.Instance.HideArrow();
            if (CafeManager.Instance != null) CafeManager.Instance.OnPlayerServedCounter();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null || (player != null && other.gameObject != player)) return;
        _playerInside = true;
        if (greenCircle != null) greenCircle.SetActive(true);
        if (whiteCircle != null) whiteCircle.SetActive(false);
        if (ArrowManager.Instance != null) ArrowManager.Instance.HideArrow();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null || (player != null && other.gameObject != player)) return;
        _playerInside = false;
        _elapsed = 0f;
        if (fillImage != null) fillImage.fillAmount = 0f;
        if (greenCircle != null) greenCircle.SetActive(false);
        if (whiteCircle != null) whiteCircle.SetActive(true);
    }
}
