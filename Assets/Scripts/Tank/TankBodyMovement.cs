using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankBodyMovement : MonoBehaviour
{

    private bool isControlled = false;
    private float rotationSpeed = 100.0f;
    private float moveSpeed = 5.0f;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        isControlled = GetComponent<Tank>().IsControlled;
        rotationSpeed = GetComponent<Tank>().BodyRotationSpeed;
        moveSpeed = GetComponent<Tank>().MoveSpeed;
    }

    private void Update()
    {

        if (isControlled)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            RotateTank(horizontalInput);
            MoveTank(verticalInput);

            Client.EmitMoveAction(GetComponent<Transform>(), GetComponentInChildren<TankTurret>().GetComponent<Transform>());
        }
    }

    public void RotateTank(float horizontalInput)
    {
        float targetRotation = transform.eulerAngles.z - horizontalInput * rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0, 0, targetRotation);
    }

    public void MoveTank(float verticalInput)
    {
        Vector2 moveDirection = transform.up * verticalInput;
        rb.velocity = moveDirection * moveSpeed;
    }
}
