using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using UnityEngine.Pool;
using Unity.VisualScripting;
using Unity.Mathematics;

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

    [SerializeField][ShowIf(nameof(IsBurstMode))] int numberOfBulletsInBurst = 0;
    [SerializeField][ShowIf(nameof(IsBurstMode))] float timeBetweenBursts;
    bool bursting;

    [SerializeField] [Range(0, 1500)] float fireRate; // rounds per minute
    float timeBetweenShots; // seconds
    float nextFireTime;

    [SerializeField] int numberOfBullets = 1;
    [SerializeField] Vector2 bulletSpread;

    [SerializeField] float bulletSpeed;
    [SerializeField] float force;
    [SerializeField] float knockBackForce;
    [SerializeField] float gravity;

    [SerializeField] LayerMask whatIsShootable;

    [SerializeField] InputActionReference fireInput;
    [SerializeField] Transform attackPoint;

    [Header("Object Pool")]
    [SerializeField] ObjectPool muzzleFlashPool;
    [SerializeField] ObjectPool bulletPool;
    [SerializeField] ObjectPool impactParticleSystemPool;

    // references
    Transform aimingCameraContainer;
    Reloading reloadingScript;
    Audio audioPlayer;
    Rigidbody playerRig;
    AbstractGunAnimator gunAnimator;

    void Start()
    {
        timeBetweenShots = 60 / fireRate;
        Debug.Log("Time Between Shots: " + timeBetweenShots);
        // get references
        reloadingScript = GetComponent<Reloading>();
        audioPlayer = GetComponent<Audio>();
        gunAnimator = GetComponent<AbstractGunAnimator>();
        // get references with FindMyIphone
        aimingCameraContainer = GameObject.Find("Aiming Camera Container").transform;
        playerRig = GameObject.Find("Player").GetComponent<Rigidbody>();
    }

    void Update()
    {
        bool shootHeld = fireInput.action.IsPressed();
        bool shootThisFrame = fireInput.action.WasPerformedThisFrame();

        // idk why this timing works, but dont change it EVER
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

        nextFireTime = (float)Time.timeAsDouble + timeBetweenShots; // don't change EVER

        // if there's multiple bullets, shoot multiple times.
        for (int i = 0; i < numberOfBullets; i++)
        {
            ProjectileFire();
        }

        gunAnimator.Fire();
        audioPlayer.FireSound();

        MuzzleFlash();

        // apply knockback
        playerRig.AddForce(-aimingCameraContainer.forward * knockBackForce, ForceMode.Impulse);

    }

    void MuzzleFlash()
    {
        GameObject muzzleFlash = muzzleFlashPool.GetObject();

        muzzleFlash.transform.position = attackPoint.position;
        muzzleFlash.transform.rotation = attackPoint.rotation;
    }

    void ProjectileFire() // don't change, works fine
    {
        // get a bullet
        GameObject curBullet = bulletPool.GetObject();
        // get its references
        Rigidbody curRig = curBullet.GetComponent<Rigidbody>();
        TrailRenderer curTrail = curBullet.GetComponent<TrailRenderer>();
        curRig.isKinematic = false;
        // this is where we aiming at
        Vector3 direction = (GetTargetPoint() - attackPoint.position).normalized;
        Debug.DrawRay(attackPoint.position, direction, Color.red, 3);

        // turn off trail
        curTrail.emitting = false;
        // move it to where it needs to be, and rotation it
        curBullet.transform.position = attackPoint.position;
        curBullet.transform.forward = direction;
        // clear the trail so it forgets where it was and then set trail on again
        curTrail.Clear();
        curTrail.emitting = true;

        ProjectileBullet curScript = curBullet.GetComponent<ProjectileBullet>();
        curScript.impactObjectPool = impactParticleSystemPool;
        curScript.gravity = gravity;

        // make it go in direction
        curRig.AddForce(direction * bulletSpeed, ForceMode.Impulse);
    }

    Vector3 GetTargetPoint()
    {
        // randominizer
        Vector3 direction = Quaternion.AngleAxis(Random.Range(-bulletSpread.x, bulletSpread.x), aimingCameraContainer.up) * Quaternion.AngleAxis(Random.Range(-bulletSpread.y, bulletSpread.y), aimingCameraContainer.right) * aimingCameraContainer.forward;
        Debug.DrawRay(aimingCameraContainer.position, direction, Color.blue, 3);
        Ray preRay = new(aimingCameraContainer.position, direction.normalized);

        if (Physics.Raycast(preRay, out RaycastHit hit, Mathf.Infinity, whatIsShootable))
        {
            return hit.point;
        }
        else
        {
            return preRay.GetPoint(75);
        }
    }

    // Helper method for NaughtyAttributes
    private bool IsBurstMode()
    {
        return fireMode == FireMode.AutoBurst || fireMode == FireMode.SemiBurst;
    }

    // powerups and items
    public void ApplyModifier(string varName, float multiplier)
    {
        // set var with varName to itself * multiplier
    }
}
