using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 15f;

    private static readonly Quaternion TargetRotation = Quaternion.Euler(45f, 180f, 0f);
    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
    }

    private void LateUpdate()
    {
        _transform.rotation = Quaternion.Slerp(
            _transform.rotation,
            TargetRotation,
            Time.deltaTime * rotationSpeed
        );
    }
}