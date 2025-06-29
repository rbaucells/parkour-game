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

    [SerializeField][ShowIf(nameof(IsBurstMode))] int numberOfBulletsInBurst = 0;
    [SerializeField][ShowIf(nameof(IsBurstMode))] float timeBetweenBursts;
    bool bursting;

    [SerializeField] [Range(0, 1500)] float fireRate; // rounds per minute
    float timeBetweenShots; // seconds
    float nextFireTime;

    [SerializeField] int numberOfBullets = 1;
    [SerializeField] Vector2 bulletSpread;

    [SerializeField] float trailSpeed;
    [SerializeField] float force;
    [SerializeField] float knockBackForce;

    [SerializeField] LayerMask whatIsShootable;

    [SerializeField] InputActionReference fireInput;
    [SerializeField] Transform attackPoint;

    [Header("Object Pool")]
    [SerializeField] ObjectPool muzzleFlashPool;
    [SerializeField] ObjectPool bulletTrailPool;
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
            RaycastFire();
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

    void RaycastFire()
    {
        // this is where were aiming
        Ray ray = new(attackPoint.position, (GetTargetPoint() - attackPoint.position).normalized);

        // if it hits something, do move toward it, if it doesn't, move in that direction for a bit
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, whatIsShootable))
        {
            StartCoroutine(RaycastTrail(attackPoint.position, hit.point, hit.normal, true));
            // add force to hit
            hit.rigidbody?.AddForceAtPosition(ray.direction * force, hit.point, ForceMode.Impulse);
        }
        else
        {
            StartCoroutine(RaycastTrail(attackPoint.position, GetTargetPoint(), Vector3.zero, false));
        }
    }

    Vector3 GetTargetPoint()
    {
        // randominizer
        Vector3 direction = Quaternion.AngleAxis(Random.Range(-bulletSpread.x, bulletSpread.x), aimingCameraContainer.up) * Quaternion.AngleAxis(Random.Range(-bulletSpread.y, bulletSpread.y), aimingCameraContainer.right) * aimingCameraContainer.forward;

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

    public IEnumerator RaycastTrail(Vector3 start, Vector3 end, Vector3 HitNormal, bool MadeImpact)
    {
        // see how far we must travel
        float distance = Vector3.Distance(start, end);
        float remainingDistance = distance;
        // get a trail
        GameObject trailObject = bulletTrailPool.GetObject();
        TrailRenderer trailRenderer = trailObject.GetComponent<TrailRenderer>();

        // make it not do trail
        trailRenderer.emitting = false;
        // while we move it to the start
        trailObject.transform.SetPositionAndRotation(start, Quaternion.identity);
        // then Clear it so it doesnt remember where it just was
        trailRenderer.Clear();
        // and turn trail back on
        trailRenderer.emitting = true;

        // they see me moving, they trailin
        while (remainingDistance > 0)
        {
            trailObject.transform.position = Vector3.Lerp(start, end, 1 - (remainingDistance / distance));
            remainingDistance -= trailSpeed * Time.deltaTime;
            yield return null;
        }

        // make sure its at the end
        trailObject.transform.position = end;
        // give it back to god
        bulletTrailPool.ReleaseObject(trailObject);

        if (MadeImpact)
        {
            // make impact go boom
            GameObject impactParticleSystem = impactParticleSystemPool.GetObject();
            impactParticleSystem.transform.SetPositionAndRotation(end, Quaternion.LookRotation(HitNormal));
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
