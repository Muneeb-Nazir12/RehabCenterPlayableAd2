using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingUnlockManager : MonoBehaviour
{
    public static BuildingUnlockManager Instance;

    private static readonly List<GameObject> _sharedCashPool = new List<GameObject>();
    private static bool _poolInitialized = false;

    [Header("Pool Owner — tick TRUE on exactly ONE instance in the scene")]
    [SerializeField] private bool isPoolOwner = false;

    [Header("Building")]
    [SerializeField] private BuildingAppearEffect buildingAppearEffect;
    [SerializeField] private Transform cameraFocusPoint;

    [Header("Unlock Cost")]
    [SerializeField] private int unlockCost = 200;
    [SerializeField] private float drainRatePerSecond = 50f;

    [Header("UI")]
    [SerializeField] private GameObject unlockPanel;
    [SerializeField] private Text unlockCostText;
    [SerializeField] private Image fillImage;

    [Header("Cash Drop (fill only on Pool Owner)")]
    [SerializeField] private GameObject cashDropPrefab;
    [SerializeField] private Transform cashSpawnPoint;
    [SerializeField] private float cashDropSpeed = 2f;
    [SerializeField] private float cashSpawnInterval = 0.25f;
    [SerializeField] private int cashPoolSize = 5;

    [Header("Objects to hide on unlock")]
    [SerializeField] private List<GameObject> objectsToHideOnUnlock;

    [Header("Circles")]
    [SerializeField] private GameObject greenCircle;
    [SerializeField] private GameObject whiteCircle;

    [Header("Camera")]
    [SerializeField] private CameraFollower cameraFollower;

    [Header("Arrow")]
    [SerializeField] private Transform thisUnlockArrowTarget;

    public static int buildingUnlockCount = 0;

    private bool _isUnlocked;
    private bool _isDraining;
    private int _totalDrained;
    private int _lastDisplayedRemaining = -1;

    private Coroutine _drainCoroutine;
    private Coroutine _cashDropCoroutine;

    private WaitForSeconds _waitCashInterval;
    private static readonly WaitForEndOfFrame _waitFrame = new WaitForEndOfFrame();

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        buildingUnlockCount = 0;
        _sharedCashPool.Clear();
        _poolInitialized = false;
    }
#endif

    private void Awake()
    {
        Instance = this;

        if (unlockCostText != null) unlockCostText.text = "";
        if (fillImage != null) fillImage.fillAmount = 0f;

        _waitCashInterval = new WaitForSeconds(cashSpawnInterval);

        if (isPoolOwner && !_poolInitialized)
            InitSharedCashPool();
    }

    private void Start()
    {
        if (unlockCostText != null) unlockCostText.text = string.Format("${0}", unlockCost);
        if (fillImage != null) fillImage.fillAmount = 0f;
    }
    private void InitSharedCashPool()
    {
        if (cashDropPrefab == null || cashSpawnPoint == null) return;
        _sharedCashPool.Clear();
        for (int i = 0; i < cashPoolSize; i++)
        {
            GameObject cash = Instantiate(cashDropPrefab, cashSpawnPoint.position, Quaternion.identity);
            cash.SetActive(false);
            _sharedCashPool.Add(cash);
        }
        _poolInitialized = true;
    }

    private GameObject GetPooledCash()
    {
        for (int i = 0; i < _sharedCashPool.Count; i++)
            if (_sharedCashPool[i] != null && !_sharedCashPool[i].activeSelf)
                return _sharedCashPool[i];
        return null;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (_isUnlocked) return;
        ShowGreenCircle();
        if (ArrowManager.Instance != null) ArrowManager.Instance.HideArrow();
        StartDrain();
    }

    private void OnTriggerExit(Collider other)
    {
        if (_isUnlocked) return;
        ShowWhiteCircle();
        PauseDrain();
        if (ArrowManager.Instance != null && thisUnlockArrowTarget != null)
            ArrowManager.Instance.PointArrowTowards(thisUnlockArrowTarget);
    }
    private void StartDrain()
    {
        if (_isDraining || _isUnlocked) return;
        if (WalletManager.Instance == null || WalletManager.Instance.GetMoney() <= 0) return;

        if (fillImage != null) fillImage.fillAmount = _totalDrained / (float)unlockCost;
        UpdateCostText(_totalDrained);

        _isDraining = true;
        _drainCoroutine = StartCoroutine(DrainCoroutine());
        _cashDropCoroutine = StartCoroutine(CashDropCoroutine());
    }

    private void PauseDrain()
    {
        if (!_isDraining) return;
        _isDraining = false;
        if (_drainCoroutine != null) { StopCoroutine(_drainCoroutine); _drainCoroutine = null; }
        if (_cashDropCoroutine != null) { StopCoroutine(_cashDropCoroutine); _cashDropCoroutine = null; }
        HideAllPooledCash();
    }

    private IEnumerator DrainCoroutine()
    {
        while (_totalDrained < unlockCost)
        {
            int needed = Mathf.Min(
                Mathf.CeilToInt(drainRatePerSecond * Time.deltaTime),
                unlockCost - _totalDrained);

            if (WalletManager.Instance != null && !WalletManager.Instance.AddMoney(-needed))
            {
                PauseDrain();
                yield break;
            }

            _totalDrained += needed;
            if (fillImage != null) fillImage.fillAmount = _totalDrained / (float)unlockCost;
            UpdateCostText(_totalDrained);
            yield return _waitFrame;
        }

        if (fillImage != null) fillImage.fillAmount = 1f;
        _isDraining = false;
        if (_cashDropCoroutine != null) { StopCoroutine(_cashDropCoroutine); _cashDropCoroutine = null; }
        HideAllPooledCash();
        UnlockBuilding();
    }

    private IEnumerator CashDropCoroutine()
    {
        if (!_poolInitialized || cashSpawnPoint == null) yield break;

        while (_isDraining)
        {
            GameObject cash = GetPooledCash();
            if (cash != null)
            {
                Vector3 spawnPos = cashSpawnPoint.position;
                spawnPos.y = 2f;
                cash.transform.position = spawnPos;
                cash.SetActive(true);
                StartCoroutine(MoveCashDown(cash, cashSpawnPoint.position.x, cashSpawnPoint.position.z));
            }
            yield return _waitCashInterval;
        }
    }

    private IEnumerator MoveCashDown(GameObject cash, float spawnX, float spawnZ)
    {
        const float endY = -1f;
        while (cash != null && cash.activeSelf)
        {
            float newY = Mathf.MoveTowards(cash.transform.position.y, endY, cashDropSpeed * Time.deltaTime);
            cash.transform.position = new Vector3(spawnX, newY, spawnZ);
            if (Mathf.Abs(newY - endY) < 0.001f) { cash.SetActive(false); yield break; }
            yield return _waitFrame;
        }
    }

    private void HideAllPooledCash()
    {
        for (int i = 0; i < _sharedCashPool.Count; i++)
            if (_sharedCashPool[i] != null) _sharedCashPool[i].SetActive(false);
    }

    private void UpdateCostText(int drained)
    {
        int remaining = Mathf.Max(0, unlockCost - drained);
        if (remaining == _lastDisplayedRemaining) return;
        _lastDisplayedRemaining = remaining;
        if (unlockCostText != null) unlockCostText.text = string.Format("${0}", remaining);
    }

    private void UnlockBuilding()
    {
        if (_isUnlocked) return;
        _isUnlocked = true;
        buildingUnlockCount += 1;

        AudioManager.Instance.PlayUnlockSound();
        if (unlockPanel != null) unlockPanel.SetActive(false);
        if (buildingAppearEffect != null) buildingAppearEffect.Show();

        if (cameraFollower != null && cameraFocusPoint != null)
            cameraFollower.FocusOnBuilding(cameraFocusPoint);

        for (int i = 0; i < objectsToHideOnUnlock.Count; i++)
            objectsToHideOnUnlock[i]?.SetActive(false);

        ShowWhiteCircle();

        switch (buildingUnlockCount)
        {
            case 1:
                RoomManager.Instance?.OnBuildingUnlocked();
                PlayableSequenceManager.Instance?.SetStep(PlayableSequenceManager.SequenceStep.Bedroom);
                break;
            case 2:
                CafeManager.Instance?.OnCafeUnlocked();
                PlayableSequenceManager.Instance?.SetStep(PlayableSequenceManager.SequenceStep.Kitchen);
                break;
            case 3:
                ShowerManager.Instance?.OnShowerUnlocked();
                PlayableSequenceManager.Instance?.SetStep(PlayableSequenceManager.SequenceStep.Shower);
                break;
            case 4:
                GymManager.Instance?.OnGymUnlocked();
                PlayableSequenceManager.Instance?.SetStep(PlayableSequenceManager.SequenceStep.Gym);
                break;
        }
    }

    private void ShowGreenCircle()
    {
        if (greenCircle != null) greenCircle.SetActive(true);
        if (whiteCircle != null) whiteCircle.SetActive(false);
    }

    private void ShowWhiteCircle()
    {
        if (greenCircle != null) greenCircle.SetActive(false);
        if (whiteCircle != null) whiteCircle.SetActive(true);
    }
}