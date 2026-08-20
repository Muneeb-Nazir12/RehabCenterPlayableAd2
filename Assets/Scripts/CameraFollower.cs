using System.Collections;
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    public static CameraFollower Instance { get; private set; }

    [SerializeField] private Vector3 positionOffset;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private Transform character;

    [SerializeField] private float focusMoveDuration = 1f;
    [SerializeField] private float focusWaitDuration = 2f;
    [SerializeField] private float returnMoveDuration = 1f;

    [Header("Post-Intro Rotation")]
    [Tooltip("Camera rotates to this Euler angle after the intro hold finishes.")]
    [SerializeField] private Vector3 postIntroRotationEuler = new Vector3(60f, 180f, 0f);
    [Tooltip("Seconds to smoothly rotate to the post-intro angle.")]
    [SerializeField] private float postIntroRotateDuration = 0.4f;

    private bool _isFocusing;
    private Coroutine _focusCoroutine;

    private WaitForSeconds _waitFocus;
    private WaitForSeconds _waitHold;

    private void Awake()
    {
        Instance = this;
        _waitFocus = new WaitForSeconds(focusWaitDuration);
        _waitHold = new WaitForSeconds(0f); // Will be set in PlayIntroRoutine
    }

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

    /// <summary>
    /// Called by GameIntroManager. Moves the camera to introPoint's position AND rotation
    /// over moveDuration seconds, holds for holdDuration seconds, then releases.
    /// After this coroutine finishes, LateUpdate resumes smooth following automatically.
    /// </summary>
    public IEnumerator PlayIntroRoutine(Transform introPoint, float moveDuration, float holdDuration)
    {
        _isFocusing = true;

        _waitHold = new WaitForSeconds(holdDuration);

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        // Move to intro point — lerp both position and rotation.
        float elapsed = 0f;
        float invDur = moveDuration > 0f ? 1f / moveDuration : 1f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed * invDur);
            transform.position = Vector3.Lerp(startPos, introPoint.position, t);
            transform.rotation = Quaternion.Slerp(startRot, introPoint.rotation, t);
            yield return null;
        }

        transform.SetPositionAndRotation(introPoint.position, introPoint.rotation);

        // Hold at intro position for the desired duration.
        yield return _waitHold;

        // Smoothly rotate to the post-intro angle (e.g. 60, 180, 0) after the hold.
        Quaternion fromRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(postIntroRotationEuler);
        float rotElapsed = 0f;
        float rotInvDur = postIntroRotateDuration > 0f ? 1f / postIntroRotateDuration : 1f;

        while (rotElapsed < postIntroRotateDuration)
        {
            rotElapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, rotElapsed * rotInvDur);
            transform.rotation = Quaternion.Slerp(fromRot, targetRot, t);
            yield return null;
        }
        transform.rotation = targetRot;

        // Release — LateUpdate resumes position-following; rotation stays at targetRot.
        _isFocusing = false;
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