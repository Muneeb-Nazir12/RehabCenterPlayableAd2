using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    public static PlayerAnimationController Instance;

    [SerializeField] private Animator animator;
    private static readonly int PlayerAnimState = Animator.StringToHash("PlayerAnimState");

    private const int StateIdle = 0;
    private const int StateWalk = 1;
    private const int StateWalkWithHolding = 2;
    private const int StateIdleWithHolding = 3;
    private const int StateCleaning = 4;

    private int _currentState = -1;
    private bool _atTowel;
    private bool _hasItem;
    private bool _isCleaning;

    private void Awake() => Instance = this;

    private void Update()
    {
        if (_isCleaning) return;
        RefreshAnimation();
    }
    public void SetAtTowel(bool state) { _atTowel = state; ForceRefresh(); }
    public void OnTowelPickedUp()
    {
        _atTowel = false;
        _hasItem = true;
        ForceRefresh();
    }
    public void OnTowelDelivered()
    {
        _hasItem = false;
        ForceRefresh();
    }

    public void SetHasItem(bool state) { _hasItem = state; ForceRefresh(); }

    public void ForceCleaningState()
    {
        _isCleaning = true;
        _currentState = StateCleaning;
        if (animator != null) animator.SetInteger(PlayerAnimState, StateCleaning);
    }

    public void StopCleaningState()
    {
        _isCleaning = false;
        _currentState = -1;
        RefreshAnimation();
    }

    private void ForceRefresh() { _currentState = -1; RefreshAnimation(); }

    private void RefreshAnimation()
    {
        if (animator == null) return;

        bool isMoving = CharacterMovement.Instance != null && CharacterMovement.Instance.IsMoving;

        int targetState;

        if (_hasItem)
        {
            targetState = isMoving ? StateWalkWithHolding : StateIdleWithHolding;
        }
        else if (_atTowel)
        {
            targetState = StateIdle;
        }
        else
        {
            targetState = isMoving ? StateWalk : StateIdle;
        }

        if (targetState == _currentState) return;
        _currentState = targetState;
        animator.SetInteger(PlayerAnimState, targetState);
    }
}