using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using UnityEngine.Pool;
using Unity.VisualScripting;

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

    [SerializeField][Range(0, 1500)] float fireRate;

    [SerializeField] Vector2 bulletSpread;

    [SerializeField] int numberOfBullets = 1;

    [SerializeField] float trailSpeed;

    [SerializeField] float force;
    [SerializeField] [ShowIf(nameof(IsBurstMode))] int numberOfBulletsInBurst = 0;
    [SerializeField] [ShowIf(nameof(IsBurstMode))] float timeBetweenBursts;
    [SerializeField] float knockBackForce;
    bool bursting;

    float timeBetweenShots;
    float nextFireTime;

    [SerializeField] LayerMask whatIsShootable;

    [SerializeField] InputActionReference fireInput;
    [SerializeField] Transform attackPoint;

    Transform cameraContainer;
    Reloading reloadingScript;
    Audio audioPlayer;
    Rigidbody playerRig;
    AbstractGunAnimator gunAnimator;

    [SerializeField] ObjectPool muzzleFlashPool;
    [SerializeField] ObjectPool bulletTrailPool;
    [SerializeField] ObjectPool impactParticleSystemPool;

    // IObjectPool<ParticleSystem> objectPool;

    void Start()
    {
        gunAnimator = GetComponent<AbstractGunAnimator>();
        timeBetweenShots = 60 / fireRate;
        Debug.Log("Time Between Shots: " + timeBetweenShots);

        cameraContainer = GameObject.Find("Aiming Container").transform;
        reloadingScript = GetComponent<Reloading>();

        playerRig = GameObject.Find("Player").GetComponent<Rigidbody>();

        audioPlayer = GetComponent<Audio>();
    }

    void Update()
    {
        bool shootHeld = fireInput.action.IsPressed();
        bool shootThisFrame = fireInput.action.WasPerformedThisFrame();
        // Check if nextFireTime is in between this FixedUpdate call and the next
        if (nextFireTime <= Time.time + Time.deltaTime * 0.552f && !gunAnimator.IsReloading())
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

        nextFireTime = (float)Time.timeAsDouble + timeBetweenShots;

        for (int i = 0; i < numberOfBullets; i++)
        {
            RaycastFire();
        }

        gunAnimator.Fire();
        audioPlayer.FireSound();

        GameObject muzzleFlash = muzzleFlashPool.GetObject();

        muzzleFlash.transform.position = attackPoint.position;
        muzzleFlash.transform.rotation = attackPoint.rotation;

        playerRig.AddForce(-cameraContainer.forward * knockBackForce, ForceMode.Impulse);

    }

    void RaycastFire()
    {
        Ray ray = new(attackPoint.position, (GetTargetPoint() - attackPoint.position).normalized);

        Debug.DrawRay(ray.origin, ray.direction, Color.red, 2);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, whatIsShootable))
        {
            StartCoroutine(RaycastTrail(attackPoint.position, hit.point, hit.normal, true));

            hit.rigidbody?.AddForceAtPosition(ray.direction * force, hit.point, ForceMode.Impulse);

            // Debug.Log("Hit Position at: " + hit.point + "Hit Rigidbyd name: " + hit.rigidbody + "On Layer: " + hit.collider.gameObject.layer);
        }
        else
        {
            StartCoroutine(RaycastTrail(attackPoint.position, GetTargetPoint(), Vector3.zero, false));
        }
    }

    Vector3 GetTargetPoint()
    {
        Vector3 direction = Quaternion.AngleAxis(Random.Range(-bulletSpread.x, bulletSpread.x), cameraContainer.up) * Quaternion.AngleAxis(Random.Range(-bulletSpread.y, bulletSpread.y), cameraContainer.right) * cameraContainer.forward;

        Ray preRay = new(cameraContainer.position, direction.normalized);

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

    public IEnumerator RaycastTrail(Vector3 start, Vector3 end, Vector3 HitNormal, bool MadeImpact) // Called from Shoot(). Moves Trail along Raycast path
    {
        GameObject Trail = bulletTrailPool.GetObject();

        Trail.SetActive(true);
        Trail.GetComponent<TrailRenderer>().emitting = true;
        Trail.GetComponent<TrailRenderer>().Clear();
        Trail.GetComponent<TrailRenderer>().enabled = true;

        Trail.transform.position = start;
        Trail.transform.rotation = Quaternion.identity;
        float distance = Vector3.Distance(start, end);
        float remainingDistance = distance;
        while (remainingDistance > 0)
        {
            Trail.transform.position = Vector3.Lerp(start, end, 1 - (remainingDistance / distance));

            remainingDistance -= trailSpeed * Time.deltaTime;

            yield return null;
        }
        Trail.transform.position = end;
        
        Trail.GetComponent<TrailRenderer>().emitting = false;
        Trail.GetComponent<TrailRenderer>().Clear();
        Trail.SetActive(false);
        Trail.GetComponent<TrailRenderer>().enabled = false;

        bulletTrailPool.ReleaseObject(Trail);

        if (MadeImpact)
        {
            GameObject impactParticleSystem = impactParticleSystemPool.GetObject();
            impactParticleSystem.transform.position = end;
            impactParticleSystem.transform.rotation = Quaternion.LookRotation(HitNormal);
        }
    }

    // Helper method for NaughtyAttributes
    private bool IsBurstMode()
    {
        return fireMode == FireMode.AutoBurst || fireMode == FireMode.SemiBurst;
    }
}
