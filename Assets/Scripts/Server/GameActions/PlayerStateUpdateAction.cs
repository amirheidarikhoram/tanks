using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerStateUpdateAction
{
    public string g_type = "player_state_update";
    public Player player;
}
