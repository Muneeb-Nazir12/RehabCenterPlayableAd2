using System.Collections;
using UnityEngine;

public class GymManager : MonoBehaviour
{
    public static GymManager Instance { get; private set; }

    [Header("Patient Mover")]
    [SerializeField] private PatientMover mover;
    [SerializeField] private Transform patient;

    [Header("Treadmill — 2 waypoints")]
    [SerializeField] private Transform treadmillPoint1;
    [SerializeField] private Transform treadmillPoint2;
    [SerializeField] private GameObject treadmillEquipment;
    [SerializeField] private GameObject treadmillMessObject;
    [SerializeField] private GameObject treadmillCleaningLogo;

    [Header("Bicep Machine — 3 waypoints")]
    [SerializeField] private Transform bicepPoint1;
    [SerializeField] private Transform bicepPoint2;
    [SerializeField] private Transform bicepPoint3;
    [SerializeField] private GameObject dumbbellLeftHand;
    [SerializeField] private GameObject dumbbellRightHand;
    [SerializeField] private GameObject bicepEquipment;
    [SerializeField] private ParticleSystem confettiParticle;
    [SerializeField] private ParticleSystem streamers;
    [SerializeField] private ParticleSystem confettiParticle1;
    [SerializeField] private ParticleSystem streamers1;
    [SerializeField] private GameObject bicepMessObject;
    [SerializeField] private GameObject bicepCleaningLogo;

    [Header("Other")]
    [SerializeField] private Transform exitPoint1, exitPoint2, exitPoint3;

    [Header("Arrow Targets")]
    [SerializeField] private Transform treadmillCleanArrowTarget;
    [SerializeField] private Transform bicepCleanArrowTarget;

    [Header("VFX")]
    [SerializeField] private GameObject exerciseSparkleVFX;

    private bool _treadmillCleaned, _bicepCleaned, _patientLeaving;

    private static readonly WaitForSeconds WaitAnim = new WaitForSeconds(1f);
    private static readonly WaitForSeconds WaitAnim2 = new WaitForSeconds(1.5f);

    private Transform[] _treadmillPath;
    private Transform[] _bicepPath;
    private Transform[] _exitPath;

    private void Awake()
    {
        Instance = this;
        _treadmillPath = new Transform[] { treadmillPoint1, treadmillPoint2 };
        _bicepPath = new Transform[] { bicepPoint1, bicepPoint2, bicepPoint3 };
        _exitPath = new Transform[] { exitPoint1, exitPoint2, exitPoint3 };
    }

    private static void Show(GameObject go, bool state)
    {
        if (go != null && go.activeSelf != state) go.SetActive(state);
    }

    private void PlayVFX(GameObject vfx)
    {
        if (vfx == null) return;
        vfx.SetActive(false);
        vfx.SetActive(true);
    }

    public void OnGymUnlocked()
    {
        TargetArrowIndicator.Hide(6);
        _treadmillCleaned = _bicepCleaned = _patientLeaving = false;
        PlayableSequenceManager.Instance?.HideQuestText();
        PlayableSequenceManager.Instance?.ShowGymFinalRecovery();
        StartCoroutine(WaitForShowerThenStart());
    }

    private IEnumerator WaitForShowerThenStart()
    {
        if (ShowerManager.Instance != null)
            while (!ShowerManager.Instance.IsShowerCompleted) yield return null;

        StartCoroutine(GymSequence());
    }

    private IEnumerator GymSequence()
    {
        if (mover != null && _treadmillPath != null)
        {
            mover.MovePath(_treadmillPath);
            while (mover.IsMoving) yield return null;
        }

        if (ShowerManager.Instance != null && ShowerManager.Instance.gymUI != null)
            ShowerManager.Instance.gymUI.SetActive(false);
        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetTreadmill();
        yield return WaitAnim;

        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetIdle();
        PlayVFX(exerciseSparkleVFX);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayEffectSound();

        PlayableSequenceManager.Instance?.HideGymFinalRecovery();
        Show(treadmillEquipment, false);
        Show(treadmillMessObject, true);
        Show(treadmillCleaningLogo, true);

        if (!_treadmillCleaned)
            ArrowManager.Instance?.PointArrowTowards(treadmillCleanArrowTarget);
        PlayableSequenceManager.Instance?.ShowQuestText("Clean the gym");
        TargetArrowIndicator.GoToTarget(7);

        if (mover != null && _bicepPath != null)
        {
            mover.MovePath(_bicepPath);
            while (mover.IsMoving) yield return null;
        }

        Show(dumbbellLeftHand, true);
        Show(dumbbellRightHand, true);
        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetBicep();
        yield return WaitAnim;

        TargetArrowIndicator.GoToTarget2(0);
        Show(dumbbellLeftHand, false);
        Show(dumbbellRightHand, false);
        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetIdle();
        PlayVFX(exerciseSparkleVFX);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayEffectSound();

        Show(bicepEquipment, false);
        Show(bicepMessObject, true);
        Show(bicepCleaningLogo, true);

        StartCoroutine(PatientExitSequence());

        while (!_treadmillCleaned || !_bicepCleaned)
        {
            if (_treadmillCleaned && !_bicepCleaned)
            {
                ArrowManager.Instance?.PointArrowTowards(bicepCleanArrowTarget);
                TargetArrowIndicator.Hide(7);
                TargetArrowIndicator.GoToTarget(9);
            }
            else if (_bicepCleaned && !_treadmillCleaned)
            {
                ArrowManager.Instance?.PointArrowTowards(treadmillCleanArrowTarget);
                TargetArrowIndicator.Hide2(0);
            }

            yield return null;
        }

        PlayableSequenceManager.Instance?.HideQuestText();
        CharacterMovement.Instance.canMove = false;
        PatientMover.Instance.Stop();
        ArrowManager.Instance?.HideArrow();
        AudioManager.Instance?.LevelCompletionSound();
        confettiParticle?.Play();
        streamers.Play();
        confettiParticle1.Play();
        streamers1.Play();
        yield return WaitAnim2;
        GameCompletionManager.Instance?.TriggerCompletion();
    }

    private IEnumerator PatientExitSequence()
    {
        if (_patientLeaving) yield break;
        _patientLeaving = true;
        if (PatientAnimationController.Instance != null)
            PatientAnimationController.Instance.SetHappyWalk();

        if (mover != null && _exitPath != null)
        {
            mover.MovePath(_exitPath);
            while (mover.IsMoving) yield return null;
        }

        if (patient != null) patient.gameObject.SetActive(false);
    }

    public void OnTreadmillCleaned()
    {
        if (_treadmillCleaned) return;
        TargetArrowIndicator.Hide(7);
        _treadmillCleaned = true;
        Show(treadmillMessObject, false);
        Show(treadmillCleaningLogo, false);
        Show(treadmillEquipment, true);
        AudioManager.Instance?.PlayCleaningSound();
    }

    public void OnBicepCleaned()
    {
        if (_bicepCleaned) return;
        TargetArrowIndicator.Hide2(0);
        _bicepCleaned = true;
        Show(bicepMessObject, false);
        Show(bicepCleaningLogo, false);
        Show(bicepEquipment, true);
        AudioManager.Instance?.PlayCleaningSound();
    }
}