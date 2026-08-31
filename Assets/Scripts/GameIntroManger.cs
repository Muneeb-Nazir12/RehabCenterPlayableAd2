using System.Collections;
using UnityEngine;

public class GameIntroManager : MonoBehaviour
{
    public static bool IntroComplete { get; private set; } = false;

    [Header("Camera Intro")]
    [SerializeField] private float cameraMoveDuration = 0.8f;
    [SerializeField] private float introHoldDuration = 2f;

    [Header("Objects to Toggle After Hold")]
    [SerializeField] private GameObject disableObject1;
    [SerializeField] private GameObject disableObject2;
    [SerializeField] private GameObject enableObject;

    private void Awake()
    {
        IntroComplete = false;
    }

    private void Start()
    {
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        if (CharacterMovement.Instance != null)
            CharacterMovement.Instance.canMove = false;

        if (CameraFollower.Instance != null)
            yield return CameraFollower.Instance.PlayIntroRoutine(cameraMoveDuration, introHoldDuration);

        if (disableObject1 != null) disableObject1.SetActive(false);
        if (disableObject2 != null) disableObject2.SetActive(false);
        if (enableObject != null) enableObject.SetActive(true);

        if (CharacterMovement.Instance != null)
            CharacterMovement.Instance.canMove = true;

        IntroComplete = true;
    }
}