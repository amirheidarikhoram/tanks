using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class Player
{
    public string id;
    public int hp;
    public int lastFireTS;
    public PlayerTransform transform;

    public float[] turretRotation;
}
