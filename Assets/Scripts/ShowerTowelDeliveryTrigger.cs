using UnityEngine;

public class ShowerTowelDeliveryTrigger : MonoBehaviour
{
    [SerializeField] private GameObject player;

    private bool _delivered;

    private void OnTriggerEnter(Collider other)
    {
        if (_delivered || other == null) return;
        if (player != null && other.gameObject != player) return;
        if (ShowerManager.Instance == null || !ShowerManager.Instance.IsTowelPickedUp) return;

        _delivered = true;

        if (ArrowManager.Instance != null) ArrowManager.Instance.HideArrow();

        if (PlayerAnimationController.Instance != null)
            PlayerAnimationController.Instance.OnTowelDelivered();

        ShowerManager.Instance.OnTowelDelivered();
    }
}