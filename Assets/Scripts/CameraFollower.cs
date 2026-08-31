using System.Collections;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public static CameraFollower Instance { get; private set; }

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private Transform character;

    [Header("Portrait Settings")]
    [SerializeField] private Vector3 portraitPositionOffset;
    [SerializeField] private Vector3 portraitRotationEuler;
    [SerializeField] private Transform portraitIntroPoint;

    [Header("Landscape Settings")]
    [SerializeField] private Vector3 landscapePositionOffset;
    [SerializeField] private Vector3 landscapeRotationEuler;
    [SerializeField] private Transform landscapeIntroPoint;

    [Header("Focus Durations")]
    [SerializeField] private float focusMoveDuration = 1f;
    [SerializeField] private float focusWaitDuration = 2f;
    [SerializeField] private float returnMoveDuration = 1f;

    [Header("Post-Intro Rotation")]
    [SerializeField] private Vector3 postIntroRotationEuler = new Vector3(60f, 180f, 0f);
    [SerializeField] private float postIntroRotateDuration = 0.4f;

    [Header("Room Unlock Focus Points")]
    [SerializeField] private Transform roomPoint;
    [SerializeField] private Transform cafePoint;
    [SerializeField] private Transform washroomPoint;
    [SerializeField] private Transform gymPoint;

    public enum RoomType { Room, Cafe, Washroom, Gym }

    public bool IsFocusing { get; private set; }

    private Coroutine _focusCoroutine;
    private Transform _transform;
    private Vector3 _savedPosition;
    private bool _isLandscape;
    private int _lastWidth = -1;
    private int _lastHeight = -1;

    private Quaternion _portraitRot;
    private Quaternion _landscapeRot;
    private Quaternion _postIntroRot;

    private Vector3 _activeOffset;
    private Quaternion _activeRotation;
    private Transform _activeIntroPoint;

    public Transform ActiveIntroPoint => _activeIntroPoint;

    private void Awake()
    {
        Instance = this;
        _transform = transform;
        _portraitRot = Quaternion.Euler(portraitRotationEuler);
        _landscapeRot = Quaternion.Euler(landscapeRotationEuler);
        _postIntroRot = Quaternion.Euler(postIntroRotationEuler);
        UpdateScreenDimensions();
    }

    private void UpdateScreenDimensions()
    {
        int sw = Screen.width;
        int sh = Screen.height;
        if (sw != _lastWidth || sh != _lastHeight)
        {
            _lastWidth = sw;
            _lastHeight = sh;
            _isLandscape = sw > sh;

            _activeOffset = _isLandscape ? landscapePositionOffset : portraitPositionOffset;
            _activeRotation = _isLandscape ? _landscapeRot : _portraitRot;
            _activeIntroPoint = _isLandscape ? landscapeIntroPoint : portraitIntroPoint;
        }
    }

    private void LateUpdate()
    {
        UpdateScreenDimensions();

        if (IsFocusing || character == null) return;

        float dt = Time.deltaTime * followSpeed;
        _transform.position = Vector3.Lerp(_transform.position, character.position + _activeOffset, dt);
        _transform.rotation = Quaternion.Lerp(_transform.rotation, _activeRotation, dt);
    }

    public void FocusOnRoom(RoomType room, float waitOverride = -1f)
    {
        Transform point = null;
        switch (room)
        {
            case RoomType.Room: point = roomPoint; break;
            case RoomType.Cafe: point = cafePoint; break;
            case RoomType.Washroom: point = washroomPoint; break;
            case RoomType.Gym: point = gymPoint; break;
        }

        if (point == null) return;
        if (_focusCoroutine != null) StopCoroutine(_focusCoroutine);
        _focusCoroutine = StartCoroutine(FocusOnRoomRoutine(point, waitOverride));
    }

    private IEnumerator FocusOnRoomRoutine(Transform focusPoint, float waitOverride)
    {
        if (focusPoint == null) yield break;

        IsFocusing = true;
        _savedPosition = _transform.position;

        Vector3 focusPos = focusPoint.position;

        float elapsed = 0f;
        float invMove = focusMoveDuration > 0f ? 1f / focusMoveDuration : 1f;
        while (elapsed < focusMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed * invMove);
            _transform.position = Vector3.Lerp(_savedPosition, focusPos, t);
            yield return null;
        }
        _transform.position = focusPos;

        float holdTime = waitOverride >= 0f ? waitOverride : focusWaitDuration;
        float holdElapsed = 0f;
        while (holdElapsed < holdTime)
        {
            holdElapsed += Time.deltaTime;
            yield return null;
        }

        Vector3 returnFrom = _transform.position;
        elapsed = 0f;
        float invReturn = returnMoveDuration > 0f ? 1f / returnMoveDuration : 1f;
        while (elapsed < returnMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed * invReturn);
            _transform.position = Vector3.Lerp(returnFrom, _savedPosition, t);
            yield return null;
        }
        _transform.position = _savedPosition;

        IsFocusing = false;
        _focusCoroutine = null;
    }

    public IEnumerator PlayIntroRoutine(float moveDuration, float holdDuration)
    {
        Transform introPoint = _activeIntroPoint;
        if (introPoint == null) yield break;

        IsFocusing = true;

        Vector3 startPos = _transform.position;
        Quaternion startRot = _transform.rotation;

        float elapsed = 0f;
        float invDur = moveDuration > 0f ? 1f / moveDuration : 1f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed * invDur);
            _transform.position = Vector3.Lerp(startPos, introPoint.position, t);
            _transform.rotation = Quaternion.Slerp(startRot, introPoint.rotation, t);
            yield return null;
        }

        _transform.SetPositionAndRotation(introPoint.position, introPoint.rotation);

        float holdElapsed = 0f;
        while (holdElapsed < holdDuration)
        {
            holdElapsed += Time.deltaTime;
            yield return null;
        }

        Quaternion fromRot = _transform.rotation;
        Quaternion targetRot = _postIntroRot;
        float rotElapsed = 0f;
        float rotInvDur = postIntroRotateDuration > 0f ? 1f / postIntroRotateDuration : 1f;

        while (rotElapsed < postIntroRotateDuration)
        {
            rotElapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, rotElapsed * rotInvDur);
            _transform.rotation = Quaternion.Slerp(fromRot, targetRot, t);
            yield return null;
        }
        _transform.rotation = targetRot;

        IsFocusing = false;
    }
}