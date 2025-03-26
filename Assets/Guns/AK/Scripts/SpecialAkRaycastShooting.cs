using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class SpecialAkRaycastShooting : MonoBehaviour
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
    [SerializeField] float knockBackForce;
    bool bursting;

    float timeBetweenShots;
    float lastFireTime;
    float nextFireTime;

    [SerializeField] LayerMask whatIsShootable;

    [SerializeField] InputActionReference fireInput;
    [SerializeField] Transform attackPoint1;
    [SerializeField] Transform attackPoint2;
    Transform cameraContainer;
    Reloading reloadingScript;
    Audio audioPlayer;
    Rigidbody playerRig;
    [SerializeField] TrailRenderer bulletTrail;
    [SerializeField] GameObject impactParticleSystem;
    [SerializeField] GameObject muzzleFlash;

    [SerializeField] AbstractGunAnimator gunAnimator;

    void Start()
    {
        timeBetweenShots = 60/fireRate;
        Debug.Log ("Time Between Shots: " + timeBetweenShots);

        cameraContainer = GameObject.Find("Camera Container").transform;
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

        nextFireTime = (float) Time.timeAsDouble + timeBetweenShots;

        for (int i = 0; i < numberOfBullets; i++)
        {
            RaycastFire();
        }

        gunAnimator.Fire();
        audioPlayer.FireSound();
        
        Instantiate(muzzleFlash, attackPoint1.position, attackPoint1.rotation);
        Instantiate(muzzleFlash, attackPoint2.position, attackPoint2.rotation);


        playerRig.AddForce(-cameraContainer.forward * knockBackForce, ForceMode.Impulse);
    }

    void RaycastFire()
    {
        Ray ray = new(attackPoint1.position, (GetTargetPoint1() - attackPoint1.position).normalized);

        Debug.DrawRay(ray.origin, ray.direction, Color.red, 2);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, whatIsShootable))
        {
            StartCoroutine(RaycastTrail(attackPoint1.position, hit.point, hit.normal, true));

            hit.rigidbody?.AddForceAtPosition(ray.direction * force, hit.point, ForceMode.Impulse);

            Debug.Log("Hit Position at: " + hit.point + "Hit Rigidbyd name: " + hit.rigidbody + "On Layer: " + hit.collider.gameObject.layer);
        }
        else
        {
            StartCoroutine(RaycastTrail(attackPoint1.position, GetTargetPoint1(), Vector3.zero, false));
        }

        Ray ray2 = new(attackPoint2.position, (GetTargetPoint2() - attackPoint2.position).normalized);

        Debug.DrawRay(ray2.origin, ray2.direction, Color.red, 2);

        if (Physics.Raycast(ray2, out RaycastHit hit2, Mathf.Infinity, whatIsShootable))
        {
            StartCoroutine(RaycastTrail(attackPoint2.position, hit2.point, hit2.normal, true));

            hit2.rigidbody?.AddForceAtPosition(ray2.direction * force, hit2.point, ForceMode.Impulse);

            Debug.Log("Hit Position at: " + hit2.point + "Hit Rigidbyd name: " + hit2.rigidbody + "On Layer: " + hit2.collider.gameObject.layer);
        }
        else
        {
            StartCoroutine(RaycastTrail(attackPoint2.position, GetTargetPoint2(), Vector3.zero, false));
        }
    }

    Vector3 GetTargetPoint1()
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
    Vector3 GetTargetPoint2()
    {
        Vector3 direction = Quaternion.AngleAxis(Random.Range(-bulletSpread.x, bulletSpread.x), -cameraContainer.up) * Quaternion.AngleAxis(Random.Range(-bulletSpread.y, bulletSpread.y), -cameraContainer.right) * -cameraContainer.forward;

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
