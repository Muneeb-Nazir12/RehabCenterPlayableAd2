using UnityEngine;

public class FootstepParticleController : MonoBehaviour
{
    [SerializeField] private ParticleSystem footstepParticle;
    [SerializeField] private float footstepInterval = 0.5f;
    [SerializeField] private int footstepEmitCount = 5;

    private float _footstepTimer;

    private void OnDisable() => _footstepTimer = 0f;

    private void Update()
    {
        if (footstepParticle == null || !CharacterMovement.Instance.IsMoving)
        {
            _footstepTimer = 0f;
            return;
        }

        _footstepTimer += Time.deltaTime;
        if (_footstepTimer >= footstepInterval)
        {
            _footstepTimer = 0f;
            footstepParticle.Emit(footstepEmitCount);
        }
    }
}
