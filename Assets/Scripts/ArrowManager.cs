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

    private static readonly Vector3 Up = Vector3.up;

    private void Awake()
    {
        Instance = this;
        if (arrowObject != null) _arrowTransform = arrowObject.transform;
        HideArrow();
    }

    private void LateUpdate()
    {
        if (!_isVisible || _trackedTarget == null || playerTransform == null || _arrowTransform == null) return;

        Vector3 playerPos = playerTransform.position;
        Vector3 direction = _trackedTarget.position - playerPos;
        direction.y = 0f;

        float sqrMag = direction.sqrMagnitude;
        if (sqrMag < 0.001f) return;

        float invMag = 1f / Mathf.Sqrt(sqrMag);
        Vector3 dir = direction * invMag;
        float yAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

        _arrowTransform.position = playerPos + dir * distanceFromPlayer + Up * heightOffset;
        _arrowTransform.rotation = Quaternion.Euler(90f, yAngle + modelRotationOffsetY, 180f);
    }

    public void PointArrowTowards(Transform target)
    {
        if (target == null) return;
        _trackedTarget = target;
        _isVisible = true;
        if (arrowObject != null && !arrowObject.activeSelf) arrowObject.SetActive(true);
        if (arrowRenderer != null) arrowRenderer.enabled = true;
    }

    public void HideArrow()
    {
        _isVisible = false;
        _trackedTarget = null;
        if (arrowRenderer != null) arrowRenderer.enabled = false;
    }
}
