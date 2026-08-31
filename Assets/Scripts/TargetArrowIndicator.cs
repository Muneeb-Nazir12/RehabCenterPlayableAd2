using System;
using UnityEngine;

public class TargetArrowIndicator : MonoBehaviour
{
    public static TargetArrowIndicator Instance { get; private set; }

    [Serializable]
    public struct ArrowTarget
    {
        public Transform pointA;
        public Transform pointB;
    }

    [Header("Arrow 1 (this GameObject)")]
    public ArrowTarget[] targets;
    public float speed = 2f;

    [Header("Arrow 2 (separate GameObject)")]
    [SerializeField] private GameObject arrow2Object;
    [SerializeField] private ArrowTarget[] arrow2Targets;
    [SerializeField] private float arrow2Speed = 2f;

    private Transform _transform;
    private Vector3 _pointA, _pointB;
    private float _t;
    private int _currentIndex = -1;

    private Transform _arrow2Transform;
    private Vector3 _a2PointA, _a2PointB;
    private float _a2T;
    private int _arrow2CurrentIndex = -1;

    private void Awake()
    {
        Instance = this;
        _transform = transform;

        if (arrow2Object != null)
        {
            _arrow2Transform = arrow2Object.transform;
            arrow2Object.SetActive(false);
        }
    }

    private void Start()
    {
        GoToTarget(0);
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        bool anyActive = false;

        if (_currentIndex >= 0 && _transform != null)
        {
            anyActive = true;
            _t += dt * speed;
            _transform.position = Vector3.Lerp(_pointA, _pointB, Mathf.PingPong(_t, 1f));
        }

        if (_arrow2CurrentIndex >= 0 && _arrow2Transform != null && arrow2Object != null && arrow2Object.activeSelf)
        {
            anyActive = true;
            _a2T += dt * arrow2Speed;
            _arrow2Transform.position = Vector3.Lerp(_a2PointA, _a2PointB, Mathf.PingPong(_a2T, 1f));
        }

        if (!anyActive)
        {
            enabled = false;
        }
    }

    public static void GoToTarget(int index)
    {
        if (Instance == null || Instance.targets == null
            || index < 0 || index >= Instance.targets.Length) return;

        var target = Instance.targets[index];
        if (target.pointA == null || target.pointB == null) return;

        Instance._currentIndex = index;
        Instance._pointA = target.pointA.position;
        Instance._pointB = target.pointB.position;
        Instance._t = 0f;
        Instance.enabled = true;
        if (!Instance.gameObject.activeSelf) Instance.gameObject.SetActive(true);
    }

    public static void Hide(int fromIndex)
    {
        if (Instance == null || Instance._currentIndex != fromIndex) return;
        Instance._currentIndex = -1;
        if (Instance.gameObject.activeSelf) Instance.gameObject.SetActive(false);
        if (Instance._arrow2CurrentIndex < 0) Instance.enabled = false;
    }

    public static void GoToTarget2(int index)
    {
        if (Instance == null || Instance.arrow2Object == null
            || Instance.arrow2Targets == null
            || index < 0 || index >= Instance.arrow2Targets.Length) return;

        var target = Instance.arrow2Targets[index];
        if (target.pointA == null || target.pointB == null) return;

        Instance._arrow2CurrentIndex = index;
        Instance._a2PointA = target.pointA.position;
        Instance._a2PointB = target.pointB.position;
        Instance._a2T = 0f;
        Instance.enabled = true;
        if (!Instance.arrow2Object.activeSelf) Instance.arrow2Object.SetActive(true);
    }

    public static void Hide2(int fromIndex)
    {
        if (Instance == null || Instance._arrow2CurrentIndex != fromIndex) return;
        Instance._arrow2CurrentIndex = -1;
        if (Instance.arrow2Object != null && Instance.arrow2Object.activeSelf)
            Instance.arrow2Object.SetActive(false);
        if (Instance._currentIndex < 0) Instance.enabled = false;
    }
}