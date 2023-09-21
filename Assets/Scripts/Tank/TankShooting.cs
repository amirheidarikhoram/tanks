using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TankShooting : MonoBehaviour
{
    private bool isControlled = false;

    public Transform FirePoint;
    public GameObject ProjectilePrefab;
    public GameObject HitPrefab;
    private float fireRate = 0.2f;

    private float fireCooldown = 0.0f;

    private AudioSource audioSource;

    private void Start () {
        isControlled = GetComponent<Tank>().IsControlled;
        fireRate = GetComponent<Tank>().FireRate;
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (isControlled) {
            if (Input.GetButton("Fire1") && Time.time >= fireCooldown)
            {
                Shoot();
                fireCooldown = Time.time + (1 / fireRate);
            }

        }
    }

    void Shoot()
    {
        // audioSource.Play();

        Vector2 fireDirection = FirePoint.up;

        Debug.Log(FirePoint.position);
        Debug.Log(fireDirection);
        
        Ray2D ray = new Ray2D(FirePoint.position, FirePoint.up);

        float maxRayDistance = 10000f;

        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, maxRayDistance);

        if (hit.collider != null)
        {
            Instantiate(HitPrefab, new Vector3(hit.point.x, hit.point.y, 10) , FirePoint.rotation);
        }

        Instantiate(ProjectilePrefab, new Vector3(FirePoint.position.x, FirePoint.position.y, 10), FirePoint.rotation);

        Client.Emit(new FireAction () {
            playerId = "1",
            firePosition = new float[2] {
                FirePoint.position.x,
                FirePoint.position.y
            },
            fireDirection = new float[2] {
                fireDirection.x,
                fireDirection.y
            },
        });
    }
}
