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

    private static readonly Vector3 Zero3 = Vector3.zero;

    private void Awake()
    {
        Instance = this;
        _movementThresholdSqr = movementThreshold * movementThreshold;

        if (rb == null) rb = GetComponent<Rigidbody>();

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

        if (canMove && rb != null)
        {
            Move(h, v, inputSqr);
        }
        else if (rb != null)
        {
            ApplyBraking();
        }

        IsMoving = canMove && inputSqr > _movementThresholdSqr;
    }

    private void Move(float h, float v, float sqrMag)
    {
        if (sqrMag > 0.01f)
        {
            float invMag = 1f / Mathf.Sqrt(sqrMag);
            Vector3 dir = new Vector3(h * invMag, 0f, v * invMag);

            Vector3 targetVelocity = dir * moveSpeed;
            Vector3 currentVel = rb.linearVelocity;
            Vector3 velocityDiff = targetVelocity - currentVel;
            velocityDiff.y = 0f;
            if (velocityDiff.sqrMagnitude > 0.0001f)
                rb.AddForce(velocityDiff, ForceMode.VelocityChange);

            Quaternion targetRot = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, rotationSpeed * 50f * Time.fixedDeltaTime));
        }
        else
        {
            ApplyBraking();
        }
    }

    private void ApplyBraking()
    {
        if (rb == null) return;
        Vector3 vel = rb.linearVelocity;
        Vector3 braking = -vel;
        braking.y = 0f;
        if (braking.sqrMagnitude > 0.0001f)
            rb.AddForce(braking, ForceMode.VelocityChange);
    }
}
