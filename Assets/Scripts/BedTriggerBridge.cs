using UnityEngine;

public class BedTriggerBridge : MonoBehaviour
{
    [SerializeField] private GameObject player;

    private void OnTriggerEnter(Collider other)
    {
        if (other == null || (player != null && other.gameObject != player)) return;
        if (RoomManager.Instance != null) RoomManager.Instance.BeginCleaning();
    }
}