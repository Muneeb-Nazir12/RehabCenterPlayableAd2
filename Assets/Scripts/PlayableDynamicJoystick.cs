using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayableDynamicJoystick : MonoBehaviour
{
    public static PlayableDynamicJoystick Instance;
    public float deadZone = 0f;
    public float handleRange = 1f;
    public RectTransform background;
    public RectTransform joystickHandle;
    public AxisOptions axisOptions;
    public bool invertHorizontal = false;
    public bool invertVertical = false;

    [System.Serializable]
    public enum AxisOptions { Both, Horizontal, Vertical }

    public float Horizontal => invertHorizontal ? -_input.x : _input.x;
    public float Vertical => invertVertical ? -_input.y : _input.y;
    public Vector2 Direction => new Vector2(Horizontal, Vertical);

    private Vector2 _input;
    private Vector2 _fixedPosition;
    private Camera _cam;
    private RectTransform _baseRect;
    private Canvas _canvas;

    private Image _backgroundImage;
    private Image _handleImage;

    private PointerEventData _cachedPointerData;
    private readonly List<RaycastResult> _cachedRaycastResults = new List<RaycastResult>(8);

    private bool _isPointerDown;
    private bool _checkUIOverlap = false;
    private float _currentAlpha = -1f;

    private Vector2 _radius;
    private float _invScaleRadiusX;
    private float _invScaleRadiusY;

    private const float AlphaUp = 0.5f;
    private const float AlphaDown = 1.0f;

    public void OnGameEnd() => _checkUIOverlap = true;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _baseRect = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();

        Vector2 center = new Vector2(0.5f, 0.5f);
        if (background != null) background.pivot = center;
        if (joystickHandle != null)
        {
            joystickHandle.anchorMin = center;
            joystickHandle.anchorMax = center;
            joystickHandle.pivot = center;
            joystickHandle.anchoredPosition = Vector2.zero;
        }

        if (_canvas != null)
        {
            _cam = _canvas.renderMode == RenderMode.ScreenSpaceCamera
                ? (_canvas.worldCamera != null ? _canvas.worldCamera : Camera.main)
                : null;
        }

        if (background != null)
        {
            _backgroundImage = background.GetComponent<Image>();
            _handleImage = background.childCount > 0
                ? background.GetChild(0).GetComponent<Image>()
                : null;
            _fixedPosition = background.anchoredPosition;
            _radius = background.sizeDelta * 0.5f;
            float scale = _canvas != null ? _canvas.scaleFactor : 1f;
            _invScaleRadiusX = _radius.x * scale > 0f ? 1f / (_radius.x * scale) : 1f;
            _invScaleRadiusY = _radius.y * scale > 0f ? 1f / (_radius.y * scale) : 1f;
        }

        if (EventSystem.current != null)
            _cachedPointerData = new PointerEventData(EventSystem.current);

        SetBackgroundAlpha(AlphaUp);
    }

    private void Update()
    {
        if (!_isPointerDown && _checkUIOverlap && IsPointerOverUI()) return;

        if (Input.GetMouseButtonDown(0)) HandlePointerDown(Input.mousePosition);
        else if (Input.GetMouseButton(0) && _isPointerDown) HandlePointerDrag(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0) && _isPointerDown) HandlePointerUp();
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (_cachedPointerData == null) _cachedPointerData = new PointerEventData(EventSystem.current);
        _cachedPointerData.position = Input.mousePosition;
        _cachedRaycastResults.Clear();
        EventSystem.current.RaycastAll(_cachedPointerData, _cachedRaycastResults);
        int count = _cachedRaycastResults.Count;
        for (int i = 0; i < count; i++)
            if (_cachedRaycastResults[i].gameObject == gameObject) return false;
        return count > 0;
    }

    private Vector2 _bgScreenPos;

    private void HandlePointerDrag(Vector2 mousePosition)
    {
        float inX = (mousePosition.x - _bgScreenPos.x) * _invScaleRadiusX;
        float inY = (mousePosition.y - _bgScreenPos.y) * _invScaleRadiusY;
        _input = new Vector2(inX, inY);

        FormatInput();
        float mag = _input.magnitude;
        if (mag > deadZone)
        {
            if (mag > 1f) _input /= mag;
        }
        else
        {
            _input = Vector2.zero;
        }

        if (joystickHandle != null)
            joystickHandle.anchoredPosition = new Vector2(_input.x * _radius.x * handleRange, _input.y * _radius.y * handleRange);
    }

    private void HandlePointerDown(Vector2 mousePosition)
    {
        _isPointerDown = true;
        _input = Vector2.zero;
        if (background != null)
        {
            background.anchoredPosition = ScreenPointToAnchoredPosition(mousePosition);
            _bgScreenPos = RectTransformUtility.WorldToScreenPoint(_cam, background.position);
        }
        SetBackgroundAlpha(AlphaDown);
        HandlePointerDrag(mousePosition);
    }

    private void HandlePointerUp()
    {
        _isPointerDown = false;
        if (background != null) background.anchoredPosition = _fixedPosition;
        if (joystickHandle != null) joystickHandle.anchoredPosition = Vector2.zero;
        _input = Vector2.zero;
        SetBackgroundAlpha(AlphaUp);
    }

    private void SetBackgroundAlpha(float a)
    {
        a = Mathf.Clamp01(a);
        if (Mathf.Approximately(_currentAlpha, a)) return;
        _currentAlpha = a;

        if (_backgroundImage != null)
        {
            Color c = _backgroundImage.color; c.a = a; _backgroundImage.color = c;
        }
        if (_handleImage != null)
        {
            Color c = _handleImage.color; c.a = a; _handleImage.color = c;
        }
    }

    private Vector2 ScreenPointToAnchoredPosition(Vector2 screenPos)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _baseRect, screenPos, _cam, out Vector2 local)
            ? local
            : Vector2.zero;
    }

    private void FormatInput()
    {
        if (axisOptions == AxisOptions.Horizontal) _input.y = 0f;
        else if (axisOptions == AxisOptions.Vertical) _input.x = 0f;
    }
}