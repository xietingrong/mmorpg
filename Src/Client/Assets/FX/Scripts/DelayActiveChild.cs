using UnityEngine;
using System.Collections;

public class DelayActiveChild : MonoBehaviour
{

    public Transform child;
    public float delayTime;

    void OnEnable()
    {
        ActiveChild(false);
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        yield return new WaitForSeconds(delayTime);
        ActiveChild(true);
    }

    void ActiveChild(bool active)
    {
        if (child != null && child != transform)
        {
            child.gameObject.SetActive(active);
        }
    }
}
