using UnityEngine;

public class BedTriggerBridge : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject whiteCircle;
    [SerializeField] private GameObject greenCircle;

    private bool _cleaningStarted;

    private void OnTriggerEnter(Collider other)
    {
        if (player != null && other.gameObject != player) return;
        TargetArrowIndicator.Hide(2);
        if (greenCircle != null && !greenCircle.activeSelf) greenCircle.SetActive(true);
        if (whiteCircle != null && whiteCircle.activeSelf) whiteCircle.SetActive(false);
        if (_cleaningStarted) return;
        if (RoomPatientController.Instance == null || !RoomPatientController.Instance.IsBedMessy) return;
        _cleaningStarted = true;
        if (RoomManager.Instance != null) RoomManager.Instance.BeginCleaning();
    }

    private void OnTriggerExit(Collider other)
    {
        if (player != null && other.gameObject != player) return;
        if (greenCircle != null && greenCircle.activeSelf) greenCircle.SetActive(false);
        if (whiteCircle != null && !whiteCircle.activeSelf) whiteCircle.SetActive(true);
    }
}