using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FireAction
{
    public string playerId;
    public string type = "fire";
    public float[] firePosition;
    public float[] fireDirection;
}
