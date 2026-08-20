using UnityEngine;

public class ShowerTowelDeliveryTrigger : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private bool _delivered;
    private void OnTriggerEnter(Collider other)
    {
        if (other == null || (player != null && other.gameObject != player)) return;

        if (_delivered) return;
        if (ShowerManager.Instance == null || !ShowerManager.Instance.IsTowelPickedUp) return;

        _delivered = true;

        ArrowManager.Instance?.HideArrow();
        PlayerAnimationController.Instance?.OnTowelDelivered();
        ShowerManager.Instance.OnTowelDelivered();
    }
}
