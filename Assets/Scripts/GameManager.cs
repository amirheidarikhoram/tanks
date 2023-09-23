using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<Player> players;

    void Start () {
        players = new List<Player>();
    }

    public bool AddPlayer(Player player) {
        int index = players.FindIndex((p) => p.id == player.id);

        if (index > -1) {
            return false;
        } else {
            players.Add(player);
            return true;
        }
    }
}
