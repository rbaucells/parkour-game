using System;
using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using UnityEngine.Pool;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Reflection;

public enum AttackPointDirection
{
    Forward,
    Backward,
    Right,
    Left
}

[RequireComponent(typeof(AudioSource))]
public class GunScript : MonoBehaviour
{
    public enum BulletType
    {
        Raycast,
        Projectile
    }

    [Header("Bullet Type")]
    [SerializeField] BulletType bulletType;

    public enum FireMode
    {
        Auto,
        SemiAuto,
        AutoBurst,
        SemiBurst
    }

    [Header("Fire Mode")]
    [SerializeField] FireMode fireMode;

    [SerializeField][ShowIf(nameof(IsBurstMode))] int numberOfBulletsInBurst = 0;
    [SerializeField][ShowIf(nameof(IsBurstMode))] float timeBetweenBursts;
    bool bursting;

    [Header("Fire Rate")]
    [SerializeField][Range(0, 1500)] float fireRate; // rounds per minute
    float timeBetweenShots; // seconds
    float nextFireTime;

    [Header("Bullet Count")]
    [SerializeField] int numberOfBullets = 1;

    [Header("Spread")]
    [SerializeField] Vector2 bulletSpread;

    [Header("Bullet Settings")]
    [SerializeField] float bulletSpeed;
    [SerializeField][ShowIf("bulletType", BulletType.Raycast)] float bulletForce;
    [SerializeField][ShowIf("bulletType", BulletType.Projectile)] float bulletGravity;
    [SerializeField] float knockBackForce;


    [Header("Aiming")]
    [SerializeField] BulletOrigin[] bulletOrigins;
    [SerializeField] LayerMask whatIsShootable;

    [Header("References")]
    [SerializeField] InputActionReference fireInput;
    Transform aimingCameraContainer;
    Rigidbody playerRig;
    AudioSource audioSource;
    AbstractGunAnimator gunAnimator;

    [Header("Magazine")]
    public int maxMagSize;
    [HideInInspector] public int curMag;
    [SerializeField] int chanceToUseAmmo = 100;

    [Header("Screen Position")]
    public Vector3[] screenPositions = new Vector3[4];

    [Header("Audio")]
    [SerializeField] AudioClip fireClip;
    [SerializeField] AudioClip[] audioClips;

    [Header("Object Pools")]
    [SerializeField] ObjectPool muzzleFlashPool;
    [SerializeField] ObjectPool bulletPool;
    [SerializeField] ObjectPool impactPool;

    void Start()
    {
        timeBetweenShots = 60 / fireRate;
        Debug.Log("Time Between Shots: " + timeBetweenShots);
        // get references
        gunAnimator = GetComponent<AbstractGunAnimator>();
        audioSource = GetComponent<AudioSource>();
        // get references with FindMyIphone
        aimingCameraContainer = GameObject.Find("Aiming Camera Container").transform;
        playerRig = GameObject.Find("Player").GetComponent<Rigidbody>();
        // set magazine to be full
        curMag = maxMagSize;
        // subscribe to my youtube channel
        gunAnimator.onReload.AddListener(Reload);
        gunAnimator.PlayAudio.AddListener(PlayAudio);
        gunAnimator.addToMag.AddListener(AddToMag);
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
        if (curMag <= 0)
        {
            gunAnimator.Reload();
            return;
        }

        if (Random.Range(1, 100) <= chanceToUseAmmo)
            curMag--;

        nextFireTime = (float)Time.timeAsDouble + timeBetweenShots; // don't change EVER

        switch (bulletType)
        {
            case BulletType.Raycast:
                for (int i = 0; i < numberOfBullets; i++)
                {
                    RaycastFire();
                }
                break;

            case BulletType.Projectile:
                for (int i = 0; i < numberOfBullets; i++)
                {
                    ProjectileFire();
                }
                break;
        }


        FireSound();
        gunAnimator.Fire();
        MuzzleFlash();

        // apply knockback
        playerRig.AddForce(-aimingCameraContainer.forward * knockBackForce, ForceMode.Impulse);
    }

    void MuzzleFlash()
    {
        foreach (BulletOrigin bulletOrigin in bulletOrigins)
        {
            Transform attackPoint = bulletOrigin.attackPoint;

            GameObject muzzleFlash = muzzleFlashPool.GetObject();

            muzzleFlash.transform.position = attackPoint.position;
            muzzleFlash.transform.rotation = attackPoint.rotation;
        }
    }

    Vector3 GetTargetPoint(AttackPointDirection attackPointDirection)
    {
        // randominizer
        Vector3 direction = Quaternion.AngleAxis(Random.Range(-bulletSpread.x, bulletSpread.x), aimingCameraContainer.up) * Quaternion.AngleAxis(Random.Range(-bulletSpread.y, bulletSpread.y), aimingCameraContainer.right) * aimingCameraContainer.forward;
        Vector3 rotatedDirection = Vector3.zero;

        switch (attackPointDirection)
        {
            case AttackPointDirection.Forward:
                rotatedDirection = direction;
                break;
            case AttackPointDirection.Backward:
                rotatedDirection = -direction;
                break;
            case AttackPointDirection.Right:
                rotatedDirection = Quaternion.AngleAxis(90f, aimingCameraContainer.up) * direction;
                break;
            case AttackPointDirection.Left:
                rotatedDirection = Quaternion.AngleAxis(-90f, aimingCameraContainer.up) * direction;
                break;
        }

        Ray preRay = new(aimingCameraContainer.position, rotatedDirection.normalized);

        if (Physics.Raycast(preRay, out RaycastHit hit, Mathf.Infinity, whatIsShootable))
        {
            return hit.point;
        }
        else
        {
            return preRay.GetPoint(75);
        }
    }

    void RaycastFire()
    {
        foreach (BulletOrigin bulletOrigin in bulletOrigins)
        {
            Transform attackPoint = bulletOrigin.attackPoint;
            Vector3 targetPoint = GetTargetPoint(bulletOrigin.attackPointDirection);
            // this is where were aiming
            Ray ray = new(attackPoint.position, (targetPoint - attackPoint.position).normalized);

            // if it hits something, do move toward it, if it doesn't, move in that direction for a bit
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, whatIsShootable))
            {
                StartCoroutine(RaycastTrail(attackPoint.position, hit.point, hit.normal, true));
                // add force to hit
                hit.rigidbody?.AddForceAtPosition(ray.direction * bulletForce, hit.point, ForceMode.Impulse);
            }
            else
            {
                StartCoroutine(RaycastTrail(attackPoint.position, targetPoint, Vector3.zero, false));
            }
        }
    }

    public IEnumerator RaycastTrail(Vector3 start, Vector3 end, Vector3 HitNormal, bool MadeImpact)
    {
        // see how far we must travel
        float distance = Vector3.Distance(start, end);
        float remainingDistance = distance;
        // get a trail
        GameObject trailObject = bulletPool.GetObject();
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
            remainingDistance -= bulletSpeed * Time.deltaTime;
            yield return null;
        }

        // make sure its at the end
        trailObject.transform.position = end;
        // give it back to god
        bulletPool.ReleaseObject(trailObject);

        if (MadeImpact)
        {
            // make impact go boom
            GameObject impactParticleSystem = impactPool.GetObject();
            impactParticleSystem.transform.SetPositionAndRotation(end, Quaternion.LookRotation(HitNormal));
        }
    }

    void ProjectileFire() // don't change, works fine
    {
        foreach (BulletOrigin bulletOrigin in bulletOrigins)
        {
            Transform attackPoint = bulletOrigin.attackPoint;
            Vector3 targetPoint = GetTargetPoint(bulletOrigin.attackPointDirection);
            // get a bullet
            GameObject curBullet = bulletPool.GetObject();
            // get its references
            Rigidbody curRig = curBullet.GetComponent<Rigidbody>();
            TrailRenderer curTrail = curBullet.GetComponent<TrailRenderer>();
            curRig.isKinematic = false;
            // this is where we aiming at
            Vector3 direction = (targetPoint - attackPoint.position).normalized;
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
            curScript.impactObjectPool = impactPool;
            curScript.gravity = bulletGravity;

            // make it go in direction
            curRig.AddForce(direction * bulletSpeed, ForceMode.Impulse);
        }
    }

    // Magazine
    void Reload()
    {
        curMag = maxMagSize;
    }

    void AddToMag(int i)
    {
        curMag += i;
    }

    // Audio Player
    void FireSound()
    {
        audioSource.PlayOneShot(fireClip);
    }

    public void PlayAudio(int audio)
    {
        audioSource.PlayOneShot(audioClips[audio]);
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
        FieldInfo fieldInfo = typeof(GunScript).GetField(varName);

        if (fieldInfo.FieldType == typeof(float))
        {
            float currentValue = (float)fieldInfo.GetValue(this);
            fieldInfo.SetValue(this, currentValue * multiplier);
        }
    }

    public void ApplyModifier(string varName, object value)
    {
        // set var with varName to value
        FieldInfo fieldInfo = typeof(GunScript).GetField(varName);

        if (fieldInfo.FieldType == value.GetType())
        {
            fieldInfo.SetValue(this, value);
        }
    }
}

[Serializable]
public class BulletOrigin
{
    public Transform attackPoint;
    public AttackPointDirection attackPointDirection;
}