using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    public GameObject doorObject;
    public float rotationAngle = -90f;

    private Transform _doorTransform;
    private Quaternion _closedLocalRotation;
    private Quaternion _openLocalRotation;
    private int _peopleInZone;

    private void Awake()
    {
        if (doorObject != null)
        {
            _doorTransform = doorObject.transform;
            _closedLocalRotation = _doorTransform.localRotation;
            _openLocalRotation = _closedLocalRotation * Quaternion.Euler(0f, rotationAngle, 0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        _peopleInZone++;
        if (_peopleInZone == 1 && _doorTransform != null)
        {
            _doorTransform.localRotation = _openLocalRotation;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _peopleInZone = Mathf.Max(0, _peopleInZone - 1);
        if (_peopleInZone == 0 && _doorTransform != null)
        {
            _doorTransform.localRotation = _closedLocalRotation;
        }
    }
}
