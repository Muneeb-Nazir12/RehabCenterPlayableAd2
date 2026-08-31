using UnityEngine;
using System;

public class PatientMover : MonoBehaviour
{
    public static PatientMover Instance { get; private set; }
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private PatientAnimationController _anim;

    public bool IsMoving => _isMoving;

    private Transform _transform;
    private Transform _target;
    private bool _isMoving;
    private Action _onArrived;
    private bool _holdingTray;
    private bool _wasPausedByCamera;

    private Transform[] _path;
    private int _pathIndex;
    private int _pathLength;

    private void Awake()
    {
        _transform = transform;
        enabled = false;
        Instance = this;
    }

    private void Update()
    {
        if (!_isMoving || _target == null)
        {
            enabled = false;
            return;
        }

        bool cameraFocusing = CameraFollower.Instance != null && CameraFollower.Instance.IsFocusing;
        if (cameraFocusing)
        {
            if (!_wasPausedByCamera)
            {
                _wasPausedByCamera = true;
                if (_anim != null) _anim.SetIdle();
            }
            return;
        }
        if (_wasPausedByCamera)
        {
            _wasPausedByCamera = false;
            SetWalkAnim();
        }

        float dt = Time.unscaledDeltaTime;
        Vector3 currentPos = _transform.position;
        Vector3 targetPos = _target.position;
        targetPos.y = currentPos.y;

        float diffX = targetPos.x - currentPos.x;
        float diffZ = targetPos.z - currentPos.z;
        float distSqr = diffX * diffX + diffZ * diffZ;

        Vector3 newPos = Vector3.MoveTowards(currentPos, targetPos, moveSpeed * dt);
        _transform.position = newPos;

        if (distSqr > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(new Vector3(diffX, 0f, diffZ));
            _transform.rotation = Quaternion.RotateTowards(_transform.rotation, targetRot, rotationSpeed * 50f * dt);
        }

        float remX = targetPos.x - newPos.x;
        float remZ = targetPos.z - newPos.z;
        if (remX * remX + remZ * remZ <= 0.0001f)
        {
            OnWaypointReached();
        }
    }

    public void MoveTo(Transform target, bool holdingTray = false)
    {
        if (target == null)
        {
            _isMoving = false;
            enabled = false;
            return;
        }

        _path = null;
        _pathIndex = 0;
        _pathLength = 0;
        _target = target;
        _onArrived = null;
        _holdingTray = holdingTray;
        _isMoving = true;
        _wasPausedByCamera = false;
        SetWalkAnim();
        enabled = true;
    }

    public void MoveTo(Transform target, Action onArrived, bool holdingTray = false)
    {
        if (target == null)
        {
            _isMoving = false;
            enabled = false;
            onArrived?.Invoke();
            return;
        }

        _path = null;
        _pathIndex = 0;
        _pathLength = 0;
        _target = target;
        _onArrived = onArrived;
        _holdingTray = holdingTray;
        _isMoving = true;
        _wasPausedByCamera = false;
        SetWalkAnim();
        enabled = true;
    }

    public void MovePath(Transform[] path, bool holdingTray = false)
    {
        if (path == null || path.Length == 0)
        {
            _isMoving = false;
            enabled = false;
            return;
        }

        _path = path;
        _pathIndex = 0;
        _pathLength = path.Length;
        _target = path[0];
        _onArrived = null;
        _holdingTray = holdingTray;
        _isMoving = true;
        _wasPausedByCamera = false;
        SetWalkAnim();
        enabled = true;
    }

    public void Stop()
    {
        _isMoving = false;
        _onArrived = null;
        _path = null;
        _target = null;
        if (_anim != null) _anim.SetIdle();
        enabled = false;
    }

    private void OnWaypointReached()
    {
        if (_path != null && _pathIndex < _pathLength - 1)
        {
            _pathIndex++;
            _target = _path[_pathIndex];
            return;
        }

        _isMoving = false;
        enabled = false;
        if (_anim != null) _anim.SetIdle();

        var cb = _onArrived;
        _onArrived = null;
        cb?.Invoke();
    }

    private void SetWalkAnim()
    {
        if (_anim == null) return;
        if (_holdingTray) _anim.SetWalkWithHolding();
        else _anim.SetWalk();
    }
}