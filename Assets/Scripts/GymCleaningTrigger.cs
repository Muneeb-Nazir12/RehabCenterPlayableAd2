using UnityEngine;
using UnityEngine.UI;

public class GymCleaningTrigger : MonoBehaviour
{
    public enum GymArea { Treadmill = 0, Bicep = 1 }

    [Header("Area")]
    [SerializeField] private GymArea area;

    [Header("Mess Reference")]
    [SerializeField] private GameObject messObject;
    [SerializeField] private Transform cleaningVFXPoint;

    [Header("Fill UI")]
    [SerializeField] private Image fillImage;
    [SerializeField] private float cleaningDuration = 0.5f;

    [Header("Shared Particle")]
    [SerializeField] private ParticleSystem sharedVFX;

    [Header("Player")]
    [SerializeField] private GameObject player;
    [SerializeField] private CharacterMovement characterMovement;
    [SerializeField] private PlayerAnimationController playerAnim;
    [SerializeField] private GameObject mop;

    private bool _playerInside;
    private bool _cleaned;
    private float _elapsed;
    private float _invDuration;

    private void Awake()
    {
        _invDuration = cleaningDuration > 0f ? 1f / cleaningDuration : 2f;
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

        if (messObject != null && !messObject.activeSelf)
        {
            ResetFill();
            return;
        }

        _elapsed += Time.deltaTime;
        if (fillImage != null) fillImage.fillAmount = Mathf.Clamp01(_elapsed * _invDuration);

        if (_elapsed >= cleaningDuration)
        {
            _cleaned = true;
            enabled = false;
            if (fillImage != null) fillImage.fillAmount = 1f;
            FinishCleaning();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_cleaned || other == null || (player != null && other.gameObject != player)) return;
        if (messObject != null && !messObject.activeSelf) return;

        _playerInside = true;
        enabled = true;
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
        ResetFill();
        if (mop != null) mop.SetActive(false);
        if (characterMovement != null) characterMovement.canMove = true;
        if (playerAnim != null) playerAnim.StopCleaningState();
    }

    private void ResetFill()
    {
        _elapsed = 0f;
        if (fillImage != null) fillImage.fillAmount = 0f;
    }

    private void FinishCleaning()
    {
        if (mop != null) mop.SetActive(false);
        if (characterMovement != null) characterMovement.canMove = true;
        if (playerAnim != null) playerAnim.StopCleaningState();

        PlayVFXAt(cleaningVFXPoint);

        if (messObject != null) messObject.SetActive(false);
        if (GymManager.Instance == null) return;

        if (area == GymArea.Treadmill)
            GymManager.Instance.OnTreadmillCleaned();
        else
            GymManager.Instance.OnBicepCleaned();
    }

    private void PlayVFXAt(Transform point)
    {
        if (sharedVFX == null || point == null) return;
        sharedVFX.transform.position = point.position;
        sharedVFX.Play();
    }
}