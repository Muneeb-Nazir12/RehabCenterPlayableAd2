using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingAppearEffect : MonoBehaviour
{
    [Header("Building")]
    [SerializeField] private GameObject building;

    [Tooltip("Place an empty GameObject at the visual center of the building and assign it here.")]
    [SerializeField] private Transform buildingCenterPoint;

    [Header("Camera Focus")]
    [Tooltip("Which room this building is. Camera moves to the matching point assigned on CameraFollower.")]
    [SerializeField] private CameraFollower.RoomType roomType;

    [Tooltip("How long camera holds at the focus point. -1 uses CameraFollower's default focusWaitDuration.")]
    [SerializeField] private float cameraHoldDuration = -1f;

    [Header("Animation")]
    [SerializeField] private float duration = 0.6f;

    [SerializeField]
    private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Objects That Must NEVER Move")]
    [SerializeField] private List<Transform> protectedObjects = new List<Transform>();

    private Vector3 targetScale;
    private Coroutine showCoroutine;
    private Transform _buildingTransform;

    private struct SavedTransform
    {
        public Transform transform;
        public Transform parent;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }
    private readonly List<SavedTransform> savedObjects = new List<SavedTransform>(8);

    private void Awake()
    {
        if (building != null)
        {
            _buildingTransform = building.transform;
            targetScale = _buildingTransform.localScale;
        }
    }

    public void Show()
    {
        if (showCoroutine != null)
            StopCoroutine(showCoroutine);

        showCoroutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        if (building == null || _buildingTransform == null) yield break;

        if (CameraFollower.Instance != null)
            CameraFollower.Instance.FocusOnRoom(roomType, cameraHoldDuration);

        SaveAndDetachProtectedObjects();

        Vector3 originalPosition = _buildingTransform.position;
        Quaternion originalRotation = _buildingTransform.rotation;

        Vector3 centerWorld = buildingCenterPoint != null
            ? buildingCenterPoint.position
            : originalPosition;

        Vector3 pivotToCenter = centerWorld - originalPosition;

        building.SetActive(true);
        _buildingTransform.localScale = Vector3.zero;
        _buildingTransform.position = originalPosition + pivotToCenter;

        float elapsed = 0f;

        if (duration <= 0f)
        {
            _buildingTransform.localScale = targetScale;
            _buildingTransform.position = originalPosition;
            _buildingTransform.rotation = originalRotation;
        }
        else
        {
            float invDur = 1f / duration;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed * invDur);
                float curveValue = scaleCurve != null ? scaleCurve.Evaluate(t) : t;

                _buildingTransform.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, curveValue);
                _buildingTransform.position = originalPosition + pivotToCenter * (1f - curveValue);
                _buildingTransform.rotation = originalRotation;

                yield return null;
            }
        }

        _buildingTransform.localScale = targetScale;
        _buildingTransform.position = originalPosition;
        _buildingTransform.rotation = originalRotation;

        RestoreProtectedObjects();
        showCoroutine = null;
    }

    private void SaveAndDetachProtectedObjects()
    {
        savedObjects.Clear();
        if (protectedObjects == null) return;

        int count = protectedObjects.Count;
        for (int i = 0; i < count; i++)
        {
            Transform obj = protectedObjects[i];
            if (obj == null) continue;

            SavedTransform saved = new SavedTransform
            {
                transform = obj,
                parent = obj.parent,
                position = obj.position,
                rotation = obj.rotation,
                scale = obj.localScale
            };

            savedObjects.Add(saved);
            obj.SetParent(null, true);
            obj.position = saved.position;
            obj.rotation = saved.rotation;
            obj.localScale = saved.scale;
        }
    }

    private void RestoreProtectedObjects()
    {
        int count = savedObjects.Count;
        for (int i = 0; i < count; i++)
        {
            SavedTransform saved = savedObjects[i];
            if (saved.transform == null) continue;

            Transform obj = saved.transform;
            obj.SetParent(saved.parent, true);
            obj.position = saved.position;
            obj.rotation = saved.rotation;
            obj.localScale = saved.scale;
        }

        savedObjects.Clear();
    }
}