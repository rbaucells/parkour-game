using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ProjectileShooting : MonoBehaviour
{
    public enum FireMode
    {
        Auto,
        SemiAuto,
        AutoBurst,
        SemiBurst
    }

    [SerializeField] FireMode fireMode;

    [SerializeField] [Range(0,1500)] float fireRate;

    [SerializeField] Vector2 bulletSpread;

    [SerializeField] int numberOfBullets = 1;

    [SerializeField] float bulletForce;

    [SerializeField] int numberOfBulletsInBurst = 0;
    [SerializeField] float timeBetweenBursts;

    [SerializeField] float knockBackForce;

    bool bursting;

    float timeBetweenShots;
    float lastFireTime;
    float nextFireTime;

    [SerializeField] LayerMask whatIsShootable;


    [SerializeField] InputActionReference fireInput;
    [SerializeField] Transform attackPoint;

    Transform cameraContainer;
    Reloading reloadingScript;
    Rigidbody playerRig;
    [SerializeField] GameObject bullet;

    void Start()
    {
        timeBetweenShots = 60/fireRate;
        Debug.Log ("Time Between Shots: " + timeBetweenShots);

        cameraContainer = GameObject.Find("Camera Container").transform;
        reloadingScript = GetComponent<Reloading>();

        playerRig = GameObject.Find("Player").GetComponent<Rigidbody>();
    }

    void Update()
    {
        bool shootHeld = fireInput.action.IsPressed();
        bool shootThisFrame = fireInput.action.WasPerformedThisFrame();
        // Check if nextFireTime is in between this FixedUpdate call and the next
        if (nextFireTime <= Time.time + Time.deltaTime * 0.552f)
        {
            if ((shootHeld && fireMode == FireMode.Auto) || (shootThisFrame && fireMode == FireMode.SemiAuto))
            {
                Shoot();
            }
            else if (!bursting)
            {
                if (shootHeld && fireMode == FireMode.AutoBurst)
                {
                    StartCoroutine(BurstFire());
                }
                else if (shootThisFrame && fireMode == FireMode.SemiBurst)
                {
                    StartCoroutine(BurstFire());
                }
            }
        }
    }

    IEnumerator BurstFire()
    {
        bursting = true;

        for (int i = 0; i < numberOfBulletsInBurst; i++)
        {
            Shoot();
            yield return new WaitForSeconds(timeBetweenShots);
        }

        yield return new WaitForSeconds(timeBetweenBursts);
        bursting = false;
    }


    void Shoot()
    {
        if (reloadingScript.curMag <= 0)
        {
            reloadingScript.Reload();
            return;
        }
        reloadingScript.curMag--;

        nextFireTime = (float) Time.timeAsDouble + timeBetweenShots;

        for (int i = 0; i < numberOfBullets; i++)
        {
            ProjectileFire();
        }

        playerRig.AddForce(-cameraContainer.forward * knockBackForce, ForceMode.Impulse);
    }

    void ProjectileFire()
    {
        Vector3 bulletDir = (GetTargetPoint() - attackPoint.position).normalized;

        GameObject curBullet = Instantiate(bullet, attackPoint.position, Quaternion.identity);

        curBullet.transform.forward = bulletDir;

        Rigidbody curRig = curBullet.GetComponent<Rigidbody>();

        curRig.AddRelativeForce(curBullet.transform.forward * bulletForce, ForceMode.Impulse);
    }

    Vector3 GetTargetPoint()
    {
        Vector3 direction = Quaternion.AngleAxis(Random.Range(-bulletSpread.x, bulletSpread.x), cameraContainer.up) * Quaternion.AngleAxis(Random.Range(-bulletSpread.y, bulletSpread.y), cameraContainer.right) * cameraContainer.forward;

        Ray preRay = new(cameraContainer.position , direction.normalized);
        
        Debug.DrawRay(preRay.origin, preRay.direction, Color.blue, 2);

        if (Physics.Raycast(preRay, out RaycastHit hit, Mathf.Infinity, whatIsShootable))
        {
            return hit.point;
        }
        else
        {
            return preRay.GetPoint(75);
        }
    }
}
