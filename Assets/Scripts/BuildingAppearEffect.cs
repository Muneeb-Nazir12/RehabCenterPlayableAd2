using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingAppearEffect : MonoBehaviour
{
    [Header("Building")]
    [SerializeField] private GameObject building;

    [Tooltip("Place an empty GameObject at the visual center of the building and assign it here.")]
    [SerializeField] private Transform buildingCenterPoint;

    [Header("Animation")]
    [SerializeField] private float duration = 0.6f;

    [SerializeField]
    private AnimationCurve scaleCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Objects That Must NEVER Move")]
    [Tooltip(
        "Add gameplay objects here such as CounterPoint, TablePoint, " +
        "ChairPoint, PatientPoint, CashPoint, etc."
    )]
    [SerializeField]
    private List<Transform> protectedObjects =
        new List<Transform>();

    private Vector3 targetScale;
    private Coroutine showCoroutine;

    private class SavedTransform
    {
        public Transform transform;
        public Transform parent;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    private readonly List<SavedTransform> savedObjects =
        new List<SavedTransform>();

    private void Awake()
    {
        if (building == null)
        {
            Debug.LogError(
                $"BuildingAppearEffect on {gameObject.name}: Building is not assigned."
            );
            return;
        }

        targetScale = building.transform.localScale;
    }

    public void Show()
    {
        if (building == null)
            return;

        if (showCoroutine != null)
            StopCoroutine(showCoroutine);

        showCoroutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        if (building == null)
            yield break;

        SaveAndDetachProtectedObjects();

        Transform buildingTransform = building.transform;
        Vector3 originalPosition = buildingTransform.position;
        Quaternion originalRotation = buildingTransform.rotation;

        Vector3 centerWorld = buildingCenterPoint != null
            ? buildingCenterPoint.position
            : originalPosition;

        Vector3 pivotToCenter = centerWorld - originalPosition;

        building.SetActive(true);
        buildingTransform.localScale = Vector3.zero;
        buildingTransform.position = originalPosition + pivotToCenter;

        float elapsed = 0f;

        if (duration <= 0f)
        {
            buildingTransform.localScale = targetScale;
            buildingTransform.position = originalPosition;
            buildingTransform.rotation = originalRotation;
        }
        else
        {
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                float curveValue = scaleCurve.Evaluate(t);

                buildingTransform.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, curveValue);
                buildingTransform.position = originalPosition + pivotToCenter * (1f - curveValue);
                buildingTransform.rotation = originalRotation;

                yield return null;
            }
        }

        buildingTransform.localScale = targetScale;
        buildingTransform.position = originalPosition;
        buildingTransform.rotation = originalRotation;

        RestoreProtectedObjects();

        showCoroutine = null;
    }

    private void SaveAndDetachProtectedObjects()
    {
        savedObjects.Clear();

        for (int i = 0; i < protectedObjects.Count; i++)
        {
            Transform obj = protectedObjects[i];

            if (obj == null)
                continue;

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
        for (int i = 0; i < savedObjects.Count; i++)
        {
            SavedTransform saved = savedObjects[i];

            if (saved.transform == null)
                continue;

            Transform obj = saved.transform;

            obj.SetParent(saved.parent, true);

            obj.position = saved.position;
            obj.rotation = saved.rotation;
            obj.localScale = saved.scale;
        }

        savedObjects.Clear();
    }

    public bool IsAnimating()
    {
        return showCoroutine != null;
    }
}