using System;
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

    void Awake()
    {
        if (IsControlled)
        {
            Id = Guid.NewGuid().ToString();
        }
    }

    void Update()
    {
        if (IsControlled)
        {
            Camera camera = FindObjectOfType<Camera>();

            if (camera != null)
            {
                camera.transform.position = new Vector3(transform.position.x, transform.position.y, camera.transform.position.z);
            }

        }

    }

    public void UpdateWithPlayer(Player player)
    {

        // Debug.Log(JsonUtility.ToJson(player));

        TankTurret turret = GetComponentInChildren<TankTurret>();
        turret.transform.rotation = new Quaternion(player.turretRotation[0], player.turretRotation[1], player.turretRotation[2], player.turretRotation[3]);

        transform.position = new Vector3(player.transform.position[0], player.transform.position[1], 10);
        transform.rotation = new Quaternion(player.transform.rotation[0], player.transform.rotation[1], player.transform.rotation[2], player.transform.rotation[3]);
    }
}
