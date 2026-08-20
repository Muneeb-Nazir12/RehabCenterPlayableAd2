using UnityEngine;

/// <summary>
/// Gates bed cleaning so the player can only start cleaning when the bed is actually messy.
/// Also prevents duplicate CleaningSequence calls if player exits and re-enters.
/// </summary>
public class BedTriggerBridge : MonoBehaviour
{
    [SerializeField] private GameObject player;

    private bool _cleaningStarted;

    private void OnTriggerEnter(Collider other)
    {
        if (_cleaningStarted) return;
        if (other == null || (player != null && other.gameObject != player)) return;

        // FIX: only begin cleaning when the bed is actually messy
        if (PatientController.Instance == null || !PatientController.Instance.IsBedMessy) return;

        _cleaningStarted = true;
        RoomManager.Instance?.BeginCleaning();
    }
}
