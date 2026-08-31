using UnityEngine;
using UnityEngine.UI;

public class TableCleaningTrigger : MonoBehaviour
{
    [Header("Fill")]
    [SerializeField] private Image fillImage;
    [SerializeField] private float cleaningDuration = 0.5f;

    [Header("Player")]
    [SerializeField] private PlayerAnimationController playerAnim;
    [SerializeField] private CharacterMovement characterMovement;
    [SerializeField] private GameObject mop;
    [SerializeField] private GameObject player;

    private bool _playerInside;
    private bool _cleaned;
    private float _elapsed;
    private float _invDuration;

    private void Awake()
    {
        _invDuration = cleaningDuration > 0f ? (1f / cleaningDuration) : 2f;
        if (fillImage != null) fillImage.fillAmount = 0f;
        enabled = false;
    }

    private void Update()
    {
        if (!_playerInside || _cleaned)
        {
            enabled = false;
            return;
        }

        if (CafePatientController.Instance != null && !CafePatientController.Instance.IsTableDirty)
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

        if (_elapsed >= cleaningDuration)
        {
            _cleaned = true;
            enabled = false;
            if (fillImage != null) fillImage.fillAmount = 1f;
            OnCleaningDone();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_cleaned || other == null || (player != null && other.gameObject != player)) return;
        TargetArrowIndicator.Hide(8);
        _playerInside = true;
        enabled = true;

        bool isDirty = CafePatientController.Instance != null && CafePatientController.Instance.IsTableDirty;
        if (!isDirty) return;

        if (mop != null) mop.SetActive(true);
        if (characterMovement != null) characterMovement.canMove = false;
        if (playerAnim != null) playerAnim.ForceCleaningState();
        if (ArrowManager.Instance != null) ArrowManager.Instance.HideArrow();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null || (player != null && other.gameObject != player)) return;
        _playerInside = false;
        enabled = false;
        if (mop != null) mop.SetActive(false);
        if (characterMovement != null) characterMovement.canMove = true;
        if (playerAnim != null) playerAnim.StopCleaningState();
        _elapsed = 0f;
        if (fillImage != null) fillImage.fillAmount = 0f;
    }

    private void OnCleaningDone()
    {
        if (mop != null) mop.SetActive(false);
        if (characterMovement != null) characterMovement.canMove = true;
        if (playerAnim != null) playerAnim.StopCleaningState();
        if (CafePatientController.Instance != null)
            CafePatientController.Instance.OnTableCleaned();
    }
}