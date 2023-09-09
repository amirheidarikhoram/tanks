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
                Debug.Log("cooldown" + fireCooldown.ToString());
                Debug.Log("time" + Time.time.ToString());
            }

        }
    }

    void Shoot()
    {
        audioSource.Play();
        
        Ray2D ray = new Ray2D(FirePoint.position, FirePoint.up);

        float maxRayDistance = 10000f;

        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, maxRayDistance);

        if (hit.collider != null)
        {
            Instantiate(HitPrefab, new Vector3(hit.point.x, hit.point.y, 10) , FirePoint.rotation);
        }

        Instantiate(ProjectilePrefab, new Vector3(FirePoint.position.x, FirePoint.position.y, 10), FirePoint.rotation);
    }
}
