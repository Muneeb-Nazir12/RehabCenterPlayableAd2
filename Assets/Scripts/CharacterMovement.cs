using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public static CharacterMovement Instance;

    public PlayableDynamicJoystick joystick;
    public CameraFollower playerCamera;
    public float moveSpeed = 4f;
    public float rotationSpeed = 15f;
    public bool canMove;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private float movementThreshold = 0.01f;
    [SerializeField] private FootstepParticleController _footstep;

    public bool IsMoving { get; private set; }

    private float _movementThresholdSqr;
    private bool _wasMoving;

    private static readonly Vector3 Zero3 = Vector3.zero;

    private void Awake()
    {
        Instance = this;
        _movementThresholdSqr = movementThreshold * movementThreshold;

        if (rb != null)
        {
            rb.linearVelocity = Zero3;
            rb.angularVelocity = Zero3;
            rb.constraints = RigidbodyConstraints.FreezeRotationX
                           | RigidbodyConstraints.FreezeRotationY
                           | RigidbodyConstraints.FreezeRotationZ;
        }

        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
            playerCamera.enabled = true;
        }
        canMove = true;
    }

    private void FixedUpdate()
    {
        float h = joystick != null ? joystick.Horizontal : 0f;
        float v = joystick != null ? joystick.Vertical : 0f;
        float inputSqr = h * h + v * v;

        bool moving = canMove && inputSqr > _movementThresholdSqr;

        if (moving)
        {
            float invMag = 1f / Mathf.Sqrt(inputSqr);
            float dirX = h * invMag;
            float dirZ = v * invMag;

            Vector3 currentVel = rb.linearVelocity;
            rb.linearVelocity = new Vector3(dirX * moveSpeed, currentVel.y, dirZ * moveSpeed);

            Vector3 dir = new Vector3(dirX, 0f, dirZ);
            Quaternion targetRot = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, rotationSpeed * 50f * Time.fixedDeltaTime));
        }
        else
        {
            ApplyBraking();
        }

        if (moving != _wasMoving)
        {
            _wasMoving = moving;
            IsMoving = moving;
            if (PlayerAnimationController.Instance != null)
                PlayerAnimationController.Instance.OnMovementChanged(moving);
            if (_footstep != null)
                _footstep.OnMovementChanged(moving);
        }
    }

    private void ApplyBraking()
    {
        if (rb == null) return;
        Vector3 vel = rb.linearVelocity;
        if (vel.x != 0f || vel.z != 0f)
        {
            if (vel.x * vel.x + vel.z * vel.z > 0.0001f)
            {
                rb.linearVelocity = new Vector3(0f, vel.y, 0f);
            }
        }
    }
}