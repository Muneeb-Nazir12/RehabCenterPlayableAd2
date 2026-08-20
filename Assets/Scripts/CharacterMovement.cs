using UnityEngine;
public class CharacterMovement : MonoBehaviour
{
    public static CharacterMovement Instance;
    [Header("Input")]
    public PlayableDynamicJoystick joystick;
    public CameraFollower          playerCamera;

    [Header("Movement")]
    public float moveSpeed     = 4f;
    public float rotationSpeed = 15f;
    public bool  canMove       = true;

    [Header("Misc")]
    [SerializeField] private float movementThreshold = 0.01f;

    public bool IsMoving { get; private set; }
    private Rigidbody _rb;
    private float     _moveSqrThreshold;
    private float     _rotSpeedDeg;          
    private Vector3 _inputDir;
    private Vector3 _targetVel;
    private Vector3 _velDiff;

    private void Awake()
    {
        Instance = this;

        _rb = GetComponent<Rigidbody>();
        _rb.linearVelocity   = Vector3.zero;
        _rb.angularVelocity  = Vector3.zero;
        _rb.constraints      = RigidbodyConstraints.FreezeRotationX
                             | RigidbodyConstraints.FreezeRotationY
                             | RigidbodyConstraints.FreezeRotationZ;

        _moveSqrThreshold = movementThreshold * movementThreshold;
        _rotSpeedDeg      = rotationSpeed * 50f;

        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true);
            playerCamera.enabled = true;
        }
    }

    private void FixedUpdate()
    {
        float h = joystick != null ? joystick.Horizontal : 0f;
        float v = joystick != null ? joystick.Vertical   : 0f;
        float sqr = h * h + v * v;

        if (canMove)
        {
            if (sqr > 0.01f)
            {
                float invMag = 1f / Mathf.Sqrt(sqr);
                _inputDir.x = h * invMag;
                _inputDir.y = 0f;
                _inputDir.z = v * invMag;

                _targetVel.x = _inputDir.x * moveSpeed;
                _targetVel.y = 0f;
                _targetVel.z = _inputDir.z * moveSpeed;

                Vector3 curVel = _rb.linearVelocity;
                _velDiff.x = _targetVel.x - curVel.x;
                _velDiff.y = 0f;
                _velDiff.z = _targetVel.z - curVel.z;

                if (_velDiff.x * _velDiff.x + _velDiff.z * _velDiff.z > 0.0001f)
                    _rb.AddForce(_velDiff, ForceMode.VelocityChange);

                _rb.MoveRotation(Quaternion.RotateTowards(
                    _rb.rotation,
                    Quaternion.LookRotation(_inputDir),
                    _rotSpeedDeg * Time.fixedDeltaTime));
            }
            else
            {
                Brake();
            }
        }
        else
        {
            Brake();
        }

        IsMoving = canMove && sqr > _moveSqrThreshold;
    }

    private void Brake()
    {
        Vector3 vel = _rb.linearVelocity;
        float bx = -vel.x, bz = -vel.z;
        if (bx * bx + bz * bz > 0.0001f)
            _rb.AddForce(new Vector3(bx, 0f, bz), ForceMode.VelocityChange);
    }
}
