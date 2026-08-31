using UnityEngine;

public class FootstepParticleController : MonoBehaviour
{
    [SerializeField] private ParticleSystem footstepParticle;
    [SerializeField] private CharacterMovement _movement;
    [SerializeField] private float footstepInterval = 0.5f;
    [SerializeField] private int footstepEmitCount = 5;

    private float _footstepTimer;

    private void Awake()
    {
        enabled = false;
        _footstepTimer = 0f;
    }

    private void OnDisable()
    {
        _footstepTimer = 0f;
    }

    public void OnMovementChanged(bool isMoving)
    {
        if (isMoving)
        {
            enabled = true;
        }
        else
        {
            _footstepTimer = 0f;
            enabled = false;
        }
    }

    private void Update()
    {
        if (footstepParticle == null) return;

        _footstepTimer += Time.deltaTime;
        if (_footstepTimer >= footstepInterval)
        {
            _footstepTimer = 0f;
            footstepParticle.Emit(footstepEmitCount);
        }
    }
}