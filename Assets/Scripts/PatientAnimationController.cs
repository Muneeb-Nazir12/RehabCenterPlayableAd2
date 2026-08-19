using UnityEngine;

public class PatientAnimationController : MonoBehaviour
{
    public static PatientAnimationController Instance;

    [SerializeField] private Animator animator;

    private static readonly int AnimStat = Animator.StringToHash("AnimStat");

    private void Awake() => Instance = this;

    public void SetIdle() => Set(0);
    public void SetWalk() => Set(1);
    public void SetLayDown() => Set(2);
    public void SetWalkWithHolding() => Set(3);
    public void SetSitting() => Set(4);
    public void SetTreadmill() => Set(5);
    public void SetBicep() => Set(6);
    public void SetHappyWalk() => Set(7);

    private void Set(int value)
    {
        if (animator == null) return;
        animator.SetInteger(AnimStat, value);
    }
}