using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireActionResponse
{
    public string playerId;
    public string type = "fire_response";
    public bool didHit;
    public Player hitPlayer = null;
    public float[] hitPosition = null;
}
