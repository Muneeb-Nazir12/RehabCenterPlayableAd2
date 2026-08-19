using System.Collections;
using UnityEngine;

public class IntroCamera : MonoBehaviour
{
    [SerializeField] private GameObject childObject1;
    [SerializeField] private GameObject childObject2;
    [SerializeField] private float activeDuration = 1f;

    private IEnumerator Start()
    {
        gameObject.SetActive(true);
        if (childObject1 != null) childObject1.SetActive(true);
        if (childObject2 != null) childObject2.SetActive(true);

        yield return new WaitForSeconds(activeDuration);

        if (childObject1 != null) childObject1.SetActive(false);
        if (childObject2 != null) childObject2.SetActive(false);
        gameObject.SetActive(false);
    }
}
