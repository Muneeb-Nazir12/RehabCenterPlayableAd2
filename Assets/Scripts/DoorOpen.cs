using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    public GameObject doorObject;
    public float rotationAngle = -90f;
    public Collider doorObjectCollider;

    private Quaternion _closedLocalRotation;
    private Quaternion _openLocalRotation;
    private int _peopleInZone;

    private void Awake()
    {
        if (doorObject != null)
        {
            _closedLocalRotation = doorObject.transform.localRotation;
            _openLocalRotation = _closedLocalRotation * Quaternion.Euler(0f, rotationAngle, 0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        _peopleInZone++;
        if (_peopleInZone == 1 && doorObject != null)
        {
            doorObject.transform.localRotation = _openLocalRotation;
            if (doorObjectCollider != null) doorObjectCollider.enabled = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _peopleInZone = Mathf.Max(0, _peopleInZone - 1);
        if (_peopleInZone == 0 && doorObject != null)
        {
            doorObject.transform.localRotation = _closedLocalRotation;
            if (doorObjectCollider != null) doorObjectCollider.enabled = true;
        }
    }
}
