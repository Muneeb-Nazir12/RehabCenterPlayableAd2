using UnityEngine;

public class ArrowManager : MonoBehaviour
{
    public static ArrowManager Instance;

    [SerializeField] private GameObject arrowObject;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float distanceFromPlayer = 1.5f;
    [SerializeField] private float heightOffset = 0.2f;
    [SerializeField] private float modelRotationOffsetY = 0f;
    [SerializeField] private Renderer arrowRenderer;

    private Transform _arrowTransform;
    private Transform _trackedTarget;
    private bool _isVisible;

    private void Awake()
    {
        Instance = this;
        if (arrowObject != null) _arrowTransform = arrowObject.transform;
        HideArrow();
    }

    private void LateUpdate()
    {
        if (!_isVisible || _trackedTarget == null || playerTransform == null || _arrowTransform == null)
        {
            enabled = false;
            return;
        }

        UpdateArrowTransform();
    }

    private void UpdateArrowTransform()
    {
        Vector3 playerPos = playerTransform.position;
        Vector3 targetPos = _trackedTarget.position;

        float diffX = targetPos.x - playerPos.x;
        float diffZ = targetPos.z - playerPos.z;
        float sqrMag = diffX * diffX + diffZ * diffZ;
        if (sqrMag < 0.001f) return;

        float invMag = 1f / Mathf.Sqrt(sqrMag);
        float dirX = diffX * invMag;
        float dirZ = diffZ * invMag;
        float yAngle = Mathf.Atan2(dirX, dirZ) * Mathf.Rad2Deg;

        _arrowTransform.position = new Vector3(
            playerPos.x + dirX * distanceFromPlayer,
            playerPos.y + heightOffset,
            playerPos.z + dirZ * distanceFromPlayer);

        _arrowTransform.rotation = Quaternion.Euler(90f, yAngle + modelRotationOffsetY, 180f);
    }

    public void PointArrowTowards(Transform target)
    {
        if (target == null) return;
        _trackedTarget = target;
        _isVisible = true;
        enabled = true;
        if (arrowObject != null && !arrowObject.activeSelf) arrowObject.SetActive(true);
        if (arrowRenderer != null && !arrowRenderer.enabled) arrowRenderer.enabled = true;
        if (_arrowTransform != null && playerTransform != null)
            UpdateArrowTransform();
    }

    public void HideArrow()
    {
        _isVisible = false;
        _trackedTarget = null;
        enabled = false;
        if (arrowRenderer != null && arrowRenderer.enabled) arrowRenderer.enabled = false;
        if (arrowObject != null && arrowObject.activeSelf) arrowObject.SetActive(false);
    }
}
