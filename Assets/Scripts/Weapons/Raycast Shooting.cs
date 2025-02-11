using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RaycastShooting : MonoBehaviour
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

    [SerializeField] float trailSpeed;

    [SerializeField] float force;
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
    [SerializeField] TrailRenderer bulletTrail;
    [SerializeField] GameObject impactParticleSystem;

    void Start()
    {
        timeBetweenShots = 60/fireRate;
        Debug.Log ("Time Between Shots: " + timeBetweenShots);
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

    public void Shoot()
    {
        if (reloadingScript.curMag <= 0)
        {
            reloadingScript.Reload();
            return;
        }
        reloadingScript.curMag--;

        nextFireTime = (float) Time.timeAsDouble + timeBetweenShots;

        // Debug.Log("Cur Time: " + Time.time + "Next Time: " + nextFireTime);

        for (int i = 0; i < numberOfBullets; i++)
        {
            RaycastFire();
        }
    }

    void RaycastFire()
    {
        Ray ray = new(attackPoint.position, (GetTargetPoint() - attackPoint.position).normalized);

        if (Physics.Raycast(ray, out RaycastHit hit, whatIsShootable))
        {
            StartCoroutine(RaycastTrail(attackPoint.position, hit.point, hit.normal, true));

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForceAtPosition(ray.direction * force, hit.point, ForceMode.Impulse);
            }
        }
        else
        {
            StartCoroutine(RaycastTrail(attackPoint.position, GetTargetPoint(), Vector3.zero, false));
        }
    }

    Vector3 GetTargetPoint()
    {
        Vector3 direction = Quaternion.AngleAxis(Random.Range(-bulletSpread.x, bulletSpread.x), attackPoint.up) * Quaternion.AngleAxis(Random.Range(-bulletSpread.y, bulletSpread.y), attackPoint.right) * cameraContainer.forward;

        Ray preRay = new(cameraContainer.position, direction);

        if (Physics.Raycast(preRay, out RaycastHit hit, whatIsShootable))
            return hit.point;
        else
            return preRay.GetPoint(75);
    }

    public IEnumerator RaycastTrail(Vector3 start, Vector3 end, Vector3 HitNormal, bool MadeImpact) // Called from Shoot(). Moves Trail along Raycast path
    {
        TrailRenderer Trail = Instantiate(bulletTrail, start, Quaternion.identity);
        float distance = Vector3.Distance(start, end);
        float remainingDistance = distance;

        while (remainingDistance > 0)
        {
            Trail.transform.position = Vector3.Lerp(start, end, 1 - (remainingDistance / distance));

            remainingDistance -= trailSpeed * Time.deltaTime;

            yield return null;
        }
        Trail.transform.position = end;

        if (MadeImpact)
        {
            Instantiate(impactParticleSystem, end, Quaternion.LookRotation(HitNormal));
        }

        Destroy(Trail.gameObject, Trail.time);
    }
}
