using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayableDynamicJoystick : MonoBehaviour
{
    public float deadZone = 0f;
    public float handleRange = 1f;
    public RectTransform background;
    public RectTransform joystickHandle;
    public AxisOptions axisOptions;
    public float maxRadius = 10f;

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
    private readonly List<RaycastResult> _cachedRaycastResults = new List<RaycastResult>();

    private bool _isPointerDown;
    private bool _checkUIOverlap = false;

    private const float AlphaUp = 0.5f;
    private const float AlphaDown = 1.0f;

    public void OnGameEnd() => _checkUIOverlap = true;

    public static PlayableDynamicJoystick Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _baseRect = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();

        Vector2 center = new Vector2(0.5f, 0.5f);
        background.pivot = center;
        joystickHandle.anchorMin = center;
        joystickHandle.anchorMax = center;
        joystickHandle.pivot = center;
        joystickHandle.anchoredPosition = Vector2.zero;

        _cam = _canvas.renderMode == RenderMode.ScreenSpaceCamera
            ? (_canvas.worldCamera != null ? _canvas.worldCamera : Camera.main)
            : null;

        _backgroundImage = background.GetComponent<Image>();
        _handleImage = background.childCount > 0
            ? background.GetChild(0).GetComponent<Image>()
            : null;

        _cachedPointerData = new PointerEventData(EventSystem.current);
        _fixedPosition = background.anchoredPosition;
        SetBackgroundAlpha(AlphaUp);
    }

    private void Update()
    {
        if (!_isPointerDown && _checkUIOverlap && IsPointerOverUI()) return;

        if (Input.GetMouseButtonDown(0)) OnMouseDown(Input.mousePosition);
        else if (Input.GetMouseButton(0) && _isPointerDown) OnDrag(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0) && _isPointerDown) OnMouseUp(Input.mousePosition);
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        _cachedPointerData.position = Input.mousePosition;
        _cachedRaycastResults.Clear();
        EventSystem.current.RaycastAll(_cachedPointerData, _cachedRaycastResults);
        int count = _cachedRaycastResults.Count;
        for (int i = 0; i < count; i++)
            if (_cachedRaycastResults[i].gameObject == gameObject) return false;
        return count > 0;
    }

    private Vector2 _bgScreenPos;

    private void OnDrag(Vector2 mousePosition)
    {
        Vector2 radius = background.sizeDelta * 0.5f;
        _input = (mousePosition - _bgScreenPos) / (radius * _canvas.scaleFactor);
        FormatInput();
        HandleInput(_input.magnitude, _input.normalized);
        joystickHandle.anchoredPosition = _input * radius * handleRange;
    }

    private void OnMouseDown(Vector2 mousePosition)
    {
        _isPointerDown = true;
        _input = Vector2.zero;
        background.anchoredPosition = ScreenPointToAnchoredPosition(mousePosition);
        _bgScreenPos = RectTransformUtility.WorldToScreenPoint(_cam, background.position);
        SetBackgroundAlpha(AlphaDown);
        OnDrag(mousePosition);
    }

    private void OnMouseUp(Vector2 mousePosition)
    {
        _isPointerDown = false;
        background.anchoredPosition = _fixedPosition;
        joystickHandle.anchoredPosition = Vector2.zero;
        _input = Vector2.zero;
        SetBackgroundAlpha(AlphaUp);
    }

    private void SetBackgroundAlpha(float a)
    {
        a = Mathf.Clamp01(a);
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

    private void HandleInput(float magnitude, Vector2 normalised)
    {
        _input = magnitude > deadZone ? (magnitude > 1f ? normalised : _input) : Vector2.zero;
    }

    private void FormatInput()
    {
        if (axisOptions == AxisOptions.Horizontal) _input.y = 0f;
        else if (axisOptions == AxisOptions.Vertical) _input.x = 0f;
    }
}
