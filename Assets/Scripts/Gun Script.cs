using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class GunScript : MonoBehaviour
{
    public enum FireType {
        Auto, 
        SemiAuto
    };

    public enum BulletType{
        Projectile,
        Raycast
    };

    public enum RecoilSize{
        Small,
        Medium,
        Big
    };

    public enum ImpactAction{
        None,
        Explode,
        Implode
    };
    
    [Header("Magazine")]
    public float magSize;
    private float curMag;

    [Header("Shooting Type")]
    public FireType fireType;
    public BulletType bulletType;
    public ImpactAction impactAction;

    [Header("Damage")]
    public float damage;

    [Header("Fire Rate")]
    public float fireRateMin = 60f;
    public float fireAnimTime;
    private float delayBetweenShots;
    private float nextFireTime = 0.0f;

    [Header("Burst Firing")]
    public bool useBurst = false;
    public int numberOfShotsInBurst;
    public float delayBetweenBursts;
    private bool curBursting = false;
    private int shotsTaken;

    [Header("Bullet Spread")]
    public bool useBulletSpread;
    public Vector2 bulletVariance;

    [Header("Impact Action")]
    public float actionRadius;
    public float actionForce;
    public float explosionUpForce;
    [Header("Other")]
    public LayerMask layerMask;
    public Transform attackPoint;
    private GameObject shootCenter;

    [Space(10)]

    [Header("Raycast Options")]
    public float hitForce;
    public float trailSpeed;

    [Header("Projectile Settings")]
    public float bulletSpeed;
    public float bulletGravity = 9.8f;
    public GameObject bulletObject;
    public Transform bulletParent;
    private GameObject curBullet;

    [Header("Effects")]
        [Header("Audio")]
        public AudioClip reloadAudio;
        public AudioClip fireAudio;

        [Header("Particles")]
        public ParticleSystem muzzleParticleSystem;
        public ParticleSystem impactParticleSystem;
        public TrailRenderer bulletTrail;

        [Header("Animations")]
        private Animator anim;
        private AudioSource audioSource;
        private PlayerScript playerScript;
        public RecoilSize recoilSize;
        private int recoilInt;

    void Awake()
    {
        // Get Components
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        // Set Shoot Type from FireType enum
        SetRecoilSize();
        // Get ShootCenter
        shootCenter = GameObject.Find("Shooter");

        delayBetweenShots = 60/fireRateMin;

        Debug.Log("Max Fire Rate: " + 60/fireAnimTime);

        playerScript = GetComponentInParent<PlayerScript>();
    }

    void SetRecoilSize()
    {
        switch (recoilSize)
        {
            case RecoilSize.Small:
                recoilInt = 1;
                break;
            case RecoilSize.Medium:
                recoilInt = 2;
                break;
            case RecoilSize.Big:
                recoilInt = 3;
                break;
        }
    }

    public void Reload()
    {
        if (ReadyToAnim())
        {
            // Reset Mag size
            curMag = magSize;
            // Play Animation "Reload"
            anim.Play("Reload", 0, 0.0f);
            // Play Reload Audio 
            audioSource.PlayOneShot(reloadAudio);
        }
    }

    public void SemiAutoFire()
    {
        // If SemiAuto selected
        if (!useBurst && curMag != 0 && fireType == FireType.SemiAuto && Time.time >= nextFireTime && ReadyToAnim())
        {
            Shoot();
            return;
        }

        if (!curBursting && useBurst && curMag != 0 && fireType == FireType.SemiAuto && Time.time >= nextFireTime && ReadyToAnim())
        {
            StartCoroutine(RepeatFire(numberOfShotsInBurst));
        }

        if (curMag == 0)
        {
            Reload();
        }
    }

    IEnumerator RepeatFire(int shotsNumber)
    {
        curBursting = true;

        for (int i = 0; i < shotsNumber; i++)
        {
            Shoot();
            yield return new WaitForSeconds(delayBetweenShots);
        }

        Invoke(nameof(DelayedBurstingOff), delayBetweenBursts);
    }

    void DelayedBurstingOff()
    {
        curBursting = false;
    }

    public void FullAutoFire()
    {
        // If SemiAuto selected
        if (!useBurst && curMag != 0 && fireType == FireType.Auto && Time.time >= nextFireTime && ReadyToAnim())
        {
            Shoot();
            return;
        }

        if (!curBursting && useBurst && curMag != 0 && fireType == FireType.Auto && Time.time >= nextFireTime && ReadyToAnim())
        {
            StartCoroutine(RepeatFire(numberOfShotsInBurst));
        }

        if (curMag == 0)
        {
            Reload();
        }
    }

    void Shoot()
    {
        nextFireTime = Time.time + delayBetweenShots;
        // Play "Fire" animation
        anim.Play("Fire", 0, 0.0f);
        playerScript.DoFireAnim(recoilInt);
        // Play the audio fireAudio
        audioSource.PlayOneShot(fireAudio);
        // Particle for Muzzle Flash
        Instantiate(muzzleParticleSystem, attackPoint.position, Quaternion.identity, transform.GetChild(0));
        if (bulletType == BulletType.Raycast)
        {
            curMag -= 1;

            Vector3 targetPoint = Vector3.zero;

            Ray preRay = new(shootCenter.transform.position, shootCenter.transform.TransformDirection(Vector3.forward));

            if (Physics.Raycast(preRay, out RaycastHit preHit, Mathf.Infinity, layerMask))
            {
                // If it hits, our target position is at hit.point
                targetPoint = preHit.point;
            }
            else
            {
                Debug.Log("Miss");
                // // Else, chose a random distance.
                targetPoint = preRay.GetPoint(75);
            }

            Vector3 rayDirection = GetDirection(attackPoint.position, targetPoint);
            // Define the ray
            Ray ray = new(attackPoint.position, rayDirection);
            // Create the ray
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                // Do Trail stuff
                TrailRenderer trail = Instantiate(bulletTrail, attackPoint.position, Quaternion.identity);
                StartCoroutine(RaycastSpawnTrail(trail, hit.point, hit.normal, true));
                // Debug
                Debug.Log("Hit at" + hit.point);

                // If our RayCast hit a rigidbody, add a force
                hit.rigidbody?.AddForce(ray.direction * hitForce, ForceMode.Impulse);
            }
            else
            {
                // You have bad aim
                Debug.Log("Miss");

                TrailRenderer trail = Instantiate(bulletTrail, attackPoint.position, Quaternion.identity);

                StartCoroutine(RaycastSpawnTrail(trail, targetPoint, Vector3.zero, false));
            }
        }
        if (bulletType == BulletType.Projectile)
        {
            curMag -= 1;
            // Where the crosshair is "looking"
            Vector3 targetPoint = Vector3.zero;
            // Define the Ray
            Ray ray = new(shootCenter.transform.position, shootCenter.transform.TransformDirection(Vector3.forward));
            // Create the Ray to see if we will hit something
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                // If it hits, our target position is at hit.point
                targetPoint = hit.point;
            }
            else
            {
                Debug.Log("Miss");
                // // Else, chose a random distance.
                targetPoint = ray.GetPoint(75);
            }
            // Define bullet direction based on direction from attackPoint to targetPoint and with useBulletSpread
            Vector3 direction = GetDirection(attackPoint.position, targetPoint);
            // Do the Visual bullet passing the direciton
            VisualBullet(direction);
        }
    }

    private IEnumerator RaycastSpawnTrail(TrailRenderer Trail, Vector3 HitPoint, Vector3 HitNormal, bool MadeImpact)
    {
        Vector3 startPosition = Trail.transform.position;
        float distance = Vector3.Distance(Trail.transform.position, HitPoint);
        float remainingDistance = distance;

        while (remainingDistance > 0)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, HitPoint, 1 - (remainingDistance / distance));

            remainingDistance -= trailSpeed * Time.deltaTime;

            yield return null;
        }
        Trail.transform.position = HitPoint;

        if (MadeImpact)
        {
            Instantiate(impactParticleSystem, HitPoint, Quaternion.LookRotation(HitNormal));
        }

        Destroy(Trail.gameObject, Trail.time);
    }

    void VisualBullet(Vector3 direction)
    {
            // Create the Visual Bullet
            curBullet = Instantiate(bulletObject, Vector3.zero, Quaternion.identity, bulletParent);
            // Create the BulletTrail and attach it to curBullet
            Instantiate(bulletTrail, Vector3.zero, Quaternion.identity, curBullet.transform);
            // Set Pos and Rot
            curBullet.transform.localPosition = attackPoint.localPosition;
            // Get rid of Parent
            curBullet.transform.parent = null;
            // Rotate bullet to point at crosshair
            curBullet.transform.forward = direction;
            // Access the Rigidbody
            Rigidbody rig = curBullet.GetComponent<Rigidbody>();
            // Add the Force
            rig.AddForce(direction * bulletSpeed, ForceMode.Impulse);
            // Access the Bullet Script
            BulletScript bulletScript = curBullet.GetComponent<BulletScript>();
            // Set the bulletGravity Accel
            bulletScript.gravity = bulletGravity;
            // Assign the Impact Particle System
            bulletScript.impactParticleSystem = impactParticleSystem;
            bulletScript.actionForce = actionForce;
            bulletScript.explosionUpForce = explosionUpForce;
            bulletScript.actionRadius = actionRadius;
            if (impactAction == ImpactAction.Explode)
            {
                bulletScript.action = 0;
            }
            else if (impactAction == ImpactAction.Implode)
            {
                bulletScript.action = 1;
            }
            else
            {
                bulletScript.action = 3;
            }
    }

    bool ReadyToAnim()
    {
        // Return true if no other anim playing
        return anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !anim.IsInTransition(0);
    }

    Vector3 GetDirection(Vector3 start, Vector3 end)
    {
        if (useBulletSpread)
        {
            Vector3 spread = new(UnityEngine.Random.Range(-bulletVariance.x, bulletVariance.x), UnityEngine.Random.Range(-bulletVariance.y, bulletVariance.y), 0);
            
            return ((end + spread) - start).normalized;
        }
        else
        {
            return(end - start).normalized;
        }
    }
}
