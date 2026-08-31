using UnityEngine;

public class CashBundle : MonoBehaviour
{
    [SerializeField] private int cashValue = 200;
    [SerializeField] private GameObject cash;
    [SerializeField] private GameObject player;

    [Header("Next Building Unlock")]
    [SerializeField] private GameObject nextBuildingUnlockPoint;

    private bool _collected;
    private bool _isActive;

    public void Activate()
    {
        _collected = false;
        _isActive = true;
        if (cash != null && !cash.activeSelf) cash.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isActive || _collected) return;
        if (other == null || (player != null && other.gameObject != player)) return;

        _collected = true;
        _isActive = false;

        if (cash != null && cash.activeSelf) cash.SetActive(false);

        if (AudioManager.Instance != null) AudioManager.Instance.PlayCashCollectSound();
        if (WalletManager.Instance != null) WalletManager.Instance.AddMoney(cashValue);

        int unlockCount = BuildingUnlockManager.buildingUnlockCount;

        if (unlockCount == 0 && ReceptionPatientController.Instance != null)
        {
            ReceptionPatientController.Instance.OnPatientServed();
        }
        else if (unlockCount == 3 && ShowerManager.Instance != null)
        {
            ShowerManager.Instance.OnShowerCashCollected();
        }
        else if (unlockCount == 2 && CafeManager.Instance != null)
        {
            CafeManager.Instance.OnCashCollected();
        }
        else if (RoomManager.Instance != null)
        {
            RoomManager.Instance.OnCashCollected();
        }
    }
}