using System.Collections;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] private Vector3 positionOffset;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private Transform character;

    [SerializeField] private float focusMoveDuration = 1f;
    [SerializeField] private float focusWaitDuration = 2f;
    [SerializeField] private float returnMoveDuration = 1f;

    private bool _isFocusing;
    private Coroutine _focusCoroutine;

    private WaitForSeconds _waitFocus;

    private void Awake() => _waitFocus = new WaitForSeconds(focusWaitDuration);

    private void LateUpdate()
    {
        if (_isFocusing || character == null) return;
        transform.position = Vector3.Lerp(
            transform.position,
            character.position + positionOffset,
            followSpeed * Time.deltaTime);
    }

    public void FocusOnBuilding(Transform focusPoint)
    {
        if (focusPoint == null) return;
        if (_focusCoroutine != null) StopCoroutine(_focusCoroutine);
        _focusCoroutine = StartCoroutine(FocusRoutine(focusPoint));
    }

    private IEnumerator FocusRoutine(Transform focusPoint)
    {
        if (focusPoint == null || character == null) yield break;

        _isFocusing = true;

        Vector3 startPos = transform.position;
        Vector3 returnPos = character.position + positionOffset;

        yield return MoveCamera(startPos, focusPoint.position, focusMoveDuration);
        yield return _waitFocus;

        returnPos = character.position + positionOffset;
        yield return MoveCamera(transform.position, returnPos, returnMoveDuration);

        transform.position = returnPos;
        _isFocusing = false;
        _focusCoroutine = null;
    }

    private IEnumerator MoveCamera(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        float invDur = duration > 0f ? (1f / duration) : 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed * invDur);
            transform.position = Vector3.Lerp(from, to, t);
            yield return null;
        }
        transform.position = to;
    }
}
