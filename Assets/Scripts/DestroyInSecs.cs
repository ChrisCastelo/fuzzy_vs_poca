using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyInSecs : MonoBehaviour
{
    public float secondsToDestroy;
    void Awake()
    {
        Destroy(gameObject, secondsToDestroy);
    }

}
