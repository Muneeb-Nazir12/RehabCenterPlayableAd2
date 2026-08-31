using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class BuildingUnlockManager : MonoBehaviour
{
    public static BuildingUnlockManager Instance;

    private static readonly List<GameObject> _sharedCashPool = new List<GameObject>();
    private static readonly List<Transform> _sharedCashTransforms = new List<Transform>();
    private static bool _poolInitialized = false;

    [Header("Pool Owner — tick TRUE on exactly ONE instance in the scene")]
    [SerializeField] private bool isPoolOwner = false;

    [Header("Building")]
    [SerializeField] private BuildingAppearEffect buildingAppearEffect;

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

    [Header("Arrow")]
    [SerializeField] private Transform thisUnlockArrowTarget;

    [Header("Arrow Index this trigger owns")]
    [SerializeField] private int arrowIndex = 1;
    public static int buildingUnlockCount = 0;

    private bool _isUnlocked;
    private bool _isDraining;
    private int _totalDrained;
    private float _drainAccumulator;
    private int _lastDisplayedRemaining = -1;
    private readonly StringBuilder _sb = new StringBuilder(8);

    private Coroutine _drainCoroutine;
    private Coroutine _cashDropCoroutine;

    private void Awake()
    {
        Instance = this;
        if (unlockCostText != null) unlockCostText.text = "";
        if (fillImage != null) fillImage.fillAmount = 0f;
        if (isPoolOwner && !_poolInitialized) InitSharedCashPool();
    }

    private void Start()
    {
        if (unlockCostText != null)
        {
            _sb.Clear();
            _sb.Append('$').Append(unlockCost);
            unlockCostText.text = _sb.ToString();
        }
        if (fillImage != null) fillImage.fillAmount = 0f;
    }

    private void InitSharedCashPool()
    {
        if (cashDropPrefab == null || cashSpawnPoint == null) return;
        _sharedCashPool.Clear();
        _sharedCashTransforms.Clear();
        for (int i = 0; i < cashPoolSize; i++)
        {
            GameObject cash = Instantiate(cashDropPrefab, cashSpawnPoint.position, Quaternion.identity);
            cash.SetActive(false);
            _sharedCashPool.Add(cash);
            _sharedCashTransforms.Add(cash.transform);
        }
        _poolInitialized = true;
    }

    private int GetPooledCashIndex()
    {
        for (int i = 0; i < _sharedCashPool.Count; i++)
            if (_sharedCashPool[i] != null && !_sharedCashPool[i].activeSelf)
                return i;
        return -1;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isUnlocked) return;
        ShowGreenCircle();
        TargetArrowIndicator.Hide(arrowIndex);
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
        float invCost = unlockCost > 0 ? 1f / unlockCost : 1f;
        if (fillImage != null) fillImage.fillAmount = _totalDrained * invCost;
        UpdateCostText(_totalDrained);
        _isDraining = true;
        _drainAccumulator = 0f;
        _drainCoroutine = StartCoroutine(DrainCoroutine(invCost));
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

    private IEnumerator DrainCoroutine(float invCost)
    {
        while (_totalDrained < unlockCost)
        {
            _drainAccumulator += drainRatePerSecond * Time.deltaTime;
            int needed = (int)_drainAccumulator;
            if (needed > 0)
            {
                _drainAccumulator -= needed;
                needed = Mathf.Min(needed, unlockCost - _totalDrained);

                if (WalletManager.Instance != null && !WalletManager.Instance.AddMoney(-needed))
                {
                    PauseDrain();
                    yield break;
                }

                _totalDrained += needed;
                if (fillImage != null) fillImage.fillAmount = _totalDrained * invCost;
                UpdateCostText(_totalDrained);
            }
            yield return null;
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

        float spawnTimer = 0f;
        const float endY = -1f;
        Vector3 spawnPos = cashSpawnPoint.position;
        float spawnX = spawnPos.x;
        float spawnZ = spawnPos.z;

        while (_isDraining)
        {
            float dt = Time.deltaTime;
            spawnTimer += dt;
            if (spawnTimer >= cashSpawnInterval)
            {
                spawnTimer = 0f;
                int index = GetPooledCashIndex();
                if (index >= 0)
                {
                    Transform cashT = _sharedCashTransforms[index];
                    cashT.position = new Vector3(spawnX, 2f, spawnZ);
                    _sharedCashPool[index].SetActive(true);
                }
            }

            for (int i = 0; i < _sharedCashPool.Count; i++)
            {
                if (_sharedCashPool[i] != null && _sharedCashPool[i].activeSelf)
                {
                    Transform cashT = _sharedCashTransforms[i];
                    Vector3 pos = cashT.position;
                    float newY = Mathf.MoveTowards(pos.y, endY, cashDropSpeed * dt);
                    cashT.position = new Vector3(spawnX, newY, spawnZ);
                    if (newY <= endY + 0.01f)
                    {
                        _sharedCashPool[i].SetActive(false);
                    }
                }
            }

            yield return null;
        }
    }

    private void HideAllPooledCash()
    {
        for (int i = 0; i < _sharedCashPool.Count; i++)
            if (_sharedCashPool[i] != null && _sharedCashPool[i].activeSelf)
                _sharedCashPool[i].SetActive(false);
    }

    private void UpdateCostText(int drained)
    {
        int remaining = Mathf.Max(0, unlockCost - drained);
        if (remaining == _lastDisplayedRemaining) return;
        _lastDisplayedRemaining = remaining;
        if (unlockCostText != null)
        {
            _sb.Clear();
            _sb.Append('$').Append(remaining);
            unlockCostText.text = _sb.ToString();
        }
    }

    private void UnlockBuilding()
    {
        if (_isUnlocked) return;
        _isUnlocked = true;
        buildingUnlockCount += 1;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayUnlockSound();
        if (unlockPanel != null) unlockPanel.SetActive(false);
        if (buildingAppearEffect != null) buildingAppearEffect.Show();

        if (objectsToHideOnUnlock != null)
        {
            for (int i = 0; i < objectsToHideOnUnlock.Count; i++)
            {
                if (objectsToHideOnUnlock[i] != null)
                    objectsToHideOnUnlock[i].SetActive(false);
            }
        }

        ShowWhiteCircle();

        switch (buildingUnlockCount)
        {
            case 1: ReceptionManager.Instance?.OnReceptionUnlocked(); break;
            case 2: CafeManager.Instance?.OnCafeUnlocked(); break;
            case 3: ShowerManager.Instance?.OnShowerUnlocked(); break;
            case 4: GymManager.Instance?.OnGymUnlocked(); break;
        }
    }

    private void ShowGreenCircle()
    {
        if (greenCircle != null && !greenCircle.activeSelf) greenCircle.SetActive(true);
        if (whiteCircle != null && whiteCircle.activeSelf) whiteCircle.SetActive(false);
    }

    private void ShowWhiteCircle()
    {
        if (greenCircle != null && greenCircle.activeSelf) greenCircle.SetActive(false);
        if (whiteCircle != null && !whiteCircle.activeSelf) whiteCircle.SetActive(true);
    }
}