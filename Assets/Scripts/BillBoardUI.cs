using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Camera _cam;
    private Quaternion _targetRotation;

    private void Awake()
    {
        _cam = Camera.main;
        _targetRotation = Quaternion.Euler(30, 180f, 0f);
    }

    private void LateUpdate()
    {
        if (transform.parent != null)
        {
            transform.localRotation = Quaternion.Inverse(transform.parent.rotation) * _targetRotation;
        }
        else
        {
            transform.rotation = _targetRotation;
        }
    }
}