using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    public static PlayerAnimationController Instance;

    [SerializeField] private Animator animator;
    private static readonly int PlayerAnimState = Animator.StringToHash("PlayerAnimState");

    private const int StateIdle = 0;
    private const int StateWalk = 1;
    private const int StateCleaning = 2;

    private int _currentState = -1;
    private bool _isCleaning;
    private bool _wasMoving;

    private CharacterMovement _characterMovement;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _characterMovement = CharacterMovement.Instance;
        SetAnimState(StateIdle);
    }

    public void OnMovementChanged(bool isMoving)
    {
        if (_isCleaning) return;
        if (isMoving == _wasMoving) return;
        _wasMoving = isMoving;
        SetAnimState(isMoving ? StateWalk : StateIdle);
    }

    public void ForceCleaningState()
    {
        _isCleaning = true;
        SetAnimState(StateCleaning);
    }

    public void StopCleaningState()
    {
        _isCleaning = false;
        _currentState = -1;
        if (_characterMovement == null) _characterMovement = CharacterMovement.Instance;
        bool isMoving = _characterMovement != null && _characterMovement.IsMoving;
        _wasMoving = isMoving;
        SetAnimState(isMoving ? StateWalk : StateIdle);
    }

    private void SetAnimState(int state)
    {
        if (animator == null || state == _currentState) return;
        _currentState = state;
        animator.SetInteger(PlayerAnimState, state);
    }
}