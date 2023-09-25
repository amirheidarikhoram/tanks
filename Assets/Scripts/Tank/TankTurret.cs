using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankTurret : MonoBehaviour
{
    private bool isControlled = false;
    private float rotationSpeed = 50.0f; // Adjust the rotation speed as needed

    private void Start()
    {
        isControlled = GetComponentInParent<Tank>().IsControlled;
    }

    private void Update()
    {

        if (isControlled)
        {
            Vector3 aimPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Aim(aimPosition);
            Client.EmitMoveAction(GetComponentInParent<Tank>().transform, GetComponent<Transform>());
        }
    }

    public void Aim(Vector3 aimPosition)
    {

        Vector3 aimDirection = (aimPosition - transform.position).normalized;

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, angle + 90), rotationSpeed * Time.deltaTime);
    }
}
