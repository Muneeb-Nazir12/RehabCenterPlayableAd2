using UnityEngine;

public class TowelPickupTrigger : MonoBehaviour
{
    [Header("Circles")]
    [SerializeField] private GameObject greenCircle;
    [SerializeField] private GameObject whiteCircle;
    [SerializeField] private GameObject player;

    private bool _pickedUp;

    private void OnEnable()
    {
        _pickedUp = false;
        if (greenCircle != null) greenCircle.SetActive(false);
        if (whiteCircle != null) whiteCircle.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_pickedUp || other == null) return;
        if (player != null && other.gameObject != player) return;
        if (ShowerManager.Instance != null && ShowerManager.Instance.IsTowelPickedUp) return;

        _pickedUp = true;

        if (greenCircle != null) greenCircle.SetActive(true);
        if (whiteCircle != null) whiteCircle.SetActive(false);

        if (ArrowManager.Instance != null) ArrowManager.Instance.HideArrow();

        if (PlayerAnimationController.Instance != null)
            PlayerAnimationController.Instance.OnTowelPickedUp();

        if (ShowerManager.Instance != null) ShowerManager.Instance.OnTowelPickedUp();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == null || (player != null && other.gameObject != player)) return;
        if (_pickedUp) return;
        if (PlayerAnimationController.Instance != null)
            PlayerAnimationController.Instance.SetAtTowel(false);

        if (greenCircle != null) greenCircle.SetActive(false);
        if (whiteCircle != null) whiteCircle.SetActive(true);
    }
}