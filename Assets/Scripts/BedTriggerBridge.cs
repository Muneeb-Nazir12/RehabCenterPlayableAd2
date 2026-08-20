using UnityEngine;

public class BedTriggerBridge : MonoBehaviour
{
    [SerializeField] private GameObject player;

    private bool _cleaningStarted;

    private void OnTriggerEnter(Collider other)
    {
        if (_cleaningStarted) return;
        if (other.gameObject != player) return;

        if (!PatientController.Instance.IsBedMessy) return;

        _cleaningStarted = true;
        RoomManager.Instance?.BeginCleaning();
    }
}
