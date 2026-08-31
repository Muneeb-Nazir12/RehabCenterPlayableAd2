using UnityEngine;

public class ButtonAnimation : MonoBehaviour
{
    [SerializeField] private float scaleAmount = 1.2f;
    [SerializeField] private float speed = 2f;

    private Transform _transform;
    private Vector3 _maxScale;
    private float _timer;

    private void Awake()
    {
        _transform = transform;
        _maxScale = Vector3.one * scaleAmount;
    }

    private void OnEnable() => _timer = 0f;

    private void Update()
    {
        _timer += Time.unscaledDeltaTime;
        float t = Mathf.PingPong(_timer * speed, 1f);
        _transform.localScale = Vector3.LerpUnclamped(Vector3.one, _maxScale, t);
    }
}