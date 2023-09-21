using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{
    public bool IsControlled = false;
    public float BodyRotationSpeed = 75f;
    public float TurretRotationSpeed = 75f;
    public float MoveSpeed = 5.0f;
    public float FireRate = 0.5f;

    public string Id;

    public void UpdateWithPlayer(Player player)
    {

        Debug.Log(JsonUtility.ToJson(player));

        TankTurret turret = GetComponent<TankTurret>();
        turret.transform.rotation = Quaternion.Euler(player.transform.rotation[0], player.transform.rotation[1], player.transform.rotation[2]);

        transform.position = new Vector3(player.transform.position[0], player.transform.position[1], 10);
        transform.rotation = Quaternion.Euler(player.transform.rotation[0], player.transform.rotation[1], player.transform.rotation[2]);
    }
}
