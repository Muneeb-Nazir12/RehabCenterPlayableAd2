using System.Collections;
using UnityEngine;
public class GameIntroManager : MonoBehaviour
{
    public static bool IntroComplete { get; private set; } = false;

    [Header("Camera Intro")]
    [Tooltip("GameObject whose Position and Rotation the camera will move to at the start.")]
    [SerializeField] private Transform cameraIntroPoint;
    [Tooltip("Seconds the camera takes to travel to the intro point.")]
    [SerializeField] private float cameraMoveDuration = 0.8f;
    [Tooltip("Seconds to hold at the intro position before releasing gameplay (both characters idle during this).")]
    [SerializeField] private float introHoldDuration = 2f;

    [Header("Objects to Toggle After Hold")]
    [Tooltip("First GameObject to disable when the hold ends.")]
    [SerializeField] private GameObject disableObject1;
    [Tooltip("Second GameObject to disable when the hold ends.")]
    [SerializeField] private GameObject disableObject2;
    [Tooltip("GameObject to enable when the hold ends (e.g. the 'Unlock room' UI element).")]
    [SerializeField] private GameObject enableObject;

    private WaitForSeconds _waitIntroDelay;

    private void Awake()
    {
        IntroComplete = false;
        _waitIntroDelay = new WaitForSeconds(0f); // Will be set dynamically in IntroSequence
    }

    private void Start()
    {
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        if (CharacterMovement.Instance != null)
            CharacterMovement.Instance.canMove = false;

        if (CameraFollower.Instance != null && cameraIntroPoint != null)
        {
            yield return CameraFollower.Instance.PlayIntroRoutine(
                cameraIntroPoint, cameraMoveDuration, introHoldDuration);
        }
        else
        {
            _waitIntroDelay = new WaitForSeconds(cameraMoveDuration + introHoldDuration);
            yield return _waitIntroDelay;
        }

        if (disableObject1 != null) disableObject1.SetActive(false);
        if (disableObject2 != null) disableObject2.SetActive(false);
        if (enableObject != null) enableObject.SetActive(true);

        if (CharacterMovement.Instance != null)
            CharacterMovement.Instance.canMove = true;

        IntroComplete = true;
    }
}