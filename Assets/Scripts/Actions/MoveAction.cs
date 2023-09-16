using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MoveAction
{
    public string type = "move";
    public string playerId;

    public PlayerTransform transform;

    public float[] turretRotation;

}
