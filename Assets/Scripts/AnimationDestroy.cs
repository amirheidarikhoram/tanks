using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationDestroy : MonoBehaviour
{

    public float LifeTime = 1.0f;

    void Start()
    {
        Destroy(gameObject, LifeTime);
    }
}
