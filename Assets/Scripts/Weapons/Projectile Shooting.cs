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
    [SerializeField] bool burstFire;

    [SerializeField] [Range(0,1500)] int fireRate;

    [SerializeField] Vector2 bulletSpread;

    [SerializeField] int numberOfBullets;

    [SerializeField] float bulletForce;

    [SerializeField] int numberOfBulletsInBurst = 0;
    [SerializeField] float timeBetweenBursts;

    bool bursting;

    float timeBetweenShots;
    float lastFireTime;
    float nextFireTime;

    [SerializeField] LayerMask whatIsShootable;

    [SerializeField] InputActionReference fireInput;

    [SerializeField] Transform cameraContainer;
    [SerializeField] Transform attackPoint;
    [SerializeField] Reloading reloadingScript;
    [SerializeField] GameObject bullet;

    void Awake()
    {
        timeBetweenShots = 60/fireRate;
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
            return;
        reloadingScript.curMag--;

        lastFireTime = (float) Time.timeAsDouble;
        nextFireTime = (float) Time.timeAsDouble + timeBetweenShots;

        Debug.Log("Cur Time: " + Time.time + "Next Time: " + nextFireTime);

        for (int i = 0; i < numberOfBullets; i++)
        {
            ProjectileFire();
        }
    }

    void ProjectileFire()
    {
        Ray ray = new(attackPoint.position, (TargetPoint() - attackPoint.position).normalized);

        GameObject curBullet = Instantiate(bullet, attackPoint.position, Quaternion.LookRotation(ray.direction));
        Rigidbody curRig = curBullet.GetComponent<Rigidbody>();

        curRig.AddForce(ray.direction * bulletForce, ForceMode.Impulse);
    }

    Vector3 TargetPoint()
    {
        Vector3 direction = Quaternion.AngleAxis(Random.Range(-bulletSpread.x, bulletSpread.x), attackPoint.up) * Quaternion.AngleAxis(Random.Range(-bulletSpread.y, bulletSpread.y), attackPoint.right) * cameraContainer.forward;

        Ray preRay = new(cameraContainer.position, direction);

        if (Physics.Raycast(preRay, out RaycastHit hit, whatIsShootable))
            return hit.point;
        else
            return preRay.GetPoint(75);
    }
}
