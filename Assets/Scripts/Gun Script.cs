using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
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

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Animator))]

public class GunScript : MonoBehaviour
{
    public enum FireMode {
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
    //--------------------Gun Options--------------------//
    [Header("Gun Options")]
    public FireMode fireMode; // Auto, SemiAuto
    public BulletType bulletType; // Projectile, Raycast
    public ImpactAction impactAction; // Explode, Implode, None
    //--------------------Magazine--------------------//
    [Header("Magazine Options")]
    public int magSize;
    private int curMag;
    //--------------------Fire Rate--------------------//
    [Header("Fire Rate")]
    public float fireRateMin = 60f;

    private float delayBetweenShots;

    private float nextFireTime = 0.0f;
    //--------------------Bullet Spread--------------------//
    [Header("Bullet Spread")]
    public bool useBulletSpread;
    public Vector2 bulletVariance;
    public bool distanceBasedSpread = true;
    public float distanceDivision = 10f; // Applies more spread if distance larger than distanceDivions, less if distance less than distanceDivision

    //--------------------Burst Fire--------------------//
    [Header("Burst Fire")]
    public bool useBurst = false;
    public int numberOfShotsInBurst;
    public float delayBetweenBursts;

    private bool curBursting = false;

    //--------------------Multiple Bullets--------------------//
    [Header("Multiple Bullets")]
    public int numberOfBullets = 1;

    //--------------------Raycast Options--------------------//
    [Header("Raycast Options")]
    public float hitForce;
    public float trailSpeed;

    //--------------------Projectile Options--------------------//
    [Header("Projectile Settings")]
    public float bulletSpeed;
    public float bulletGravity = 9.8f;
    [OptionalField] public GameObject bulletModel;

    //--------------------Reloading--------------------//
    [Header("Reloading Options")]
    public bool multipleReloadAnim;

    private bool reloading;

    //--------------------Damage--------------------//
    [Header("Damage")]
    public float damage;

    //--------------------Impact Action--------------------//
    [Header("Impact Action")]
    public float actionRadius;
    public float actionForce;
    public float explosionUpForce;

    //--------------------Recoil--------------------//
    [Header("Recoil")]
    public RecoilSize recoilSize;
    public bool usePositionalRecoil;
    public float positionalRecoilForce;
    private int recoilInt; // 1 = Small, 2 = Medium, 3 = Big

    //--------------------References--------------------//
    [Header("References")]
    public LayerMask layerMask;
    public Transform attackPoint;
    private GameObject shootCenter;
    private Rigidbody playerRig;
    private Animator anim;
    private AudioSource audioSource;
    private PlayerScript playerScript;

    //--------------------Audio--------------------//
    [Header("Audio")]
    [OptionalField] public AudioClip reloadAudio;
    [OptionalField] public AudioClip fireAudio;

    [OptionalField] public AudioClip audio1;
    [OptionalField] public AudioClip audio2;
    [OptionalField] public AudioClip audio3;


    //--------------------Particles--------------------//
    [Header("Particles")]
    public ParticleSystem muzzleParticleSystem;
    public TrailRenderer bulletTrail;
    public ParticleSystem impactParticleSystem;

    //--------------------Animations--------------------//
    [Header("Animations")]
    private float fireAnimTime;
    private float reloadAnimTime;
    private float reloadInAnimTime;

    #region Start

    void Awake() // Called Before First Frame
    {
        DefineComponents();

        SetAnimClipSizes();
        SetRecoilSize();

        Debug.Log("Max Fire Rate: " + 60/fireAnimTime);
    }

    void DefineComponents() // Called in Awake(). Sets Referencs
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        shootCenter = GameObject.Find("Shooter");
        delayBetweenShots = 60/fireRateMin;

        playerScript = GetComponentInParent<PlayerScript>();
        playerRig = playerScript.gameObject.GetComponent<Rigidbody>();
    }

    void SetAnimClipSizes() // Called in Awake(). Defines animTime values
    {
        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
        foreach(AnimationClip clip in clips)
        {
            switch(clip.name)
            {
                case "Fire":
                    fireAnimTime = clip.length;
                    break;
                case "Reload":
                    reloadAnimTime = clip.length;
                    break;
                case "Reload In":
                    reloadInAnimTime = clip.length;
                    break;
            }
        }
    }

    void SetRecoilSize() // Called in Awake(). Defines recoilInt base on RecoilSize enum.
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
    #endregion
    #region Reload

    public void Reload() // Called from PlayerScript.Reload(). Reloads
    {
        if (ReadyToAnim() && !reloading && curMag != magSize)
        {
            if (multipleReloadAnim)
            {
                StartCoroutine(RepeatReload());
            }
            else
            {
                reloading = true;
                // Play Animation "Reload"
                anim.Play("Reload", 0, 0.0f);
                // Play Reload Audio 
                audioSource.PlayOneShot(reloadAudio);

                Invoke(nameof(ResetMagSize), reloadAnimTime);
            }
        }
    }

    void ResetMagSize() // Called from Reload() after delay. Resets magSize once reloadAnim is done
    {
        // Reset Mag size
        curMag = magSize;
        reloading = false;
    }

    IEnumerator RepeatReload() // Called from Reload(). For use in Multi-Reload guns. Pump Shotguns
    {
        reloading = true;

        // Trigger the rotation into reload (transition in)
        anim.Play("Reload In", 0, 0.0f);

        yield return new WaitForSeconds(reloadInAnimTime);

        // Start reloading
        while (curMag < magSize)
        {
            anim.Play("Reload", 0, 0.0f);
            audioSource.PlayOneShot(reloadAudio);
            curMag++;

            Debug.Log("Reload");
            yield return new WaitForSeconds(reloadAnimTime);
        }

        // Trigger the rotation out of reload (transition out)
        anim.Play("Reload Out", 0, 0.0f);
        reloading = false;
    }
    #endregion
    #region Fire

    public void SemiAutoFire() // Called from PlayerScript.OnFireInput()
    {
        if (!useBurst && curMag != 0 && fireMode == FireMode.SemiAuto && Time.time >= nextFireTime && ReadyToAnim())
        {
            Shoot();
            return;
        }

        if (!curBursting && useBurst && curMag != 0 && fireMode == FireMode.SemiAuto && Time.time >= nextFireTime && ReadyToAnim())
        {
            StartCoroutine(RepeatFire(numberOfShotsInBurst));
        }

        if (curMag == 0)
        {
            StartCoroutine(WaitForReload());
        }
    }

    public void FullAutoFire() // Called from PlayerScript.OnFireInput() and PlayerScript.FixedUpdate
    {
        if (!useBurst && curMag != 0 && fireMode == FireMode.Auto && Time.time >= nextFireTime && ReadyToAnim())
        {
            Shoot();
            return;
        }

        if (!curBursting && useBurst && curMag != 0 && fireMode == FireMode.Auto && Time.time >= nextFireTime && ReadyToAnim())
        {
            StartCoroutine(RepeatFire(numberOfShotsInBurst));
        }

        if (curMag == 0)
        {
            StartCoroutine(WaitForReload());
        }
    }

    IEnumerator WaitForReload()
    {
        while (Time.time < nextFireTime)
        {
            yield return null;
        }

        Reload();
    }

    IEnumerator RepeatFire(int shotsNumber) // Called from SemiAutoFire() and FullAutoFire(). Calls Shoot function repeatedly
    {
        curBursting = true;

        for (int i = 0; i < shotsNumber; i++)
        {
            Shoot();
            yield return new WaitForSeconds(delayBetweenShots);
        }

        Invoke(nameof(DelayedBurstingOff), delayBetweenBursts);
    }

    void DelayedBurstingOff() // Called from RepeatFire(). Doesn't allow Bursting until delay
    {
        curBursting = false;
    }

    void Shoot() // Called from SemiAutoFire(), FullAutoFire() and RepeatFire(). Shoots
    {
        nextFireTime = Time.time + delayBetweenShots;
        // Play "Fire" animation
        anim.Play("Fire", 0, 0.0f);
        playerScript.DoFireAnim(recoilInt);
        // Play the audio fireAudio
        audioSource.PlayOneShot(fireAudio);
        // Particle for Muzzle Flash
        ParticleSystem muzzleParticle = Instantiate(muzzleParticleSystem, attackPoint.position, Quaternion.identity);
        
        muzzleParticle.gameObject.transform.parent = transform.GetChild(0);
        curMag -= 1;
        for (int i = 0; i < numberOfBullets; i++)
        {
            if (bulletType == BulletType.Raycast)
            {
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
        if (usePositionalRecoil)
            playerRig.AddForce(-shootCenter.transform.forward * positionalRecoilForce, ForceMode.Impulse);
    }

    private IEnumerator RaycastSpawnTrail(TrailRenderer Trail, Vector3 HitPoint, Vector3 HitNormal, bool MadeImpact) // Called from Shoot(). Moves Trail along Raycast path
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

    void VisualBullet(Vector3 direction) // Called from Shoot(). Spawns bullet and adds forces
    {
        // Create the Visual Bullet
        GameObject curBullet = Instantiate(bulletModel, Vector3.zero, Quaternion.identity);
        // Create the BulletTrail and attach it to curBullet
        Instantiate(bulletTrail, Vector3.zero, Quaternion.identity, curBullet.transform);
        // Set Pos and Rot
        curBullet.transform.position = attackPoint.position;
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
    #endregion
    #region Utilities

    bool ReadyToAnim() // Returns true if no other anim playing
    {
        return anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !anim.IsInTransition(0);
    }

    Vector3 GetDirection(Vector3 start, Vector3 end) // Gives direction using Spread and distanceBasedSpread
    {
        if (useBulletSpread)
        {
            // Base direction from start to end
            Vector3 direction = (end - start).normalized;

            // Calculate right and up vectors based on the direction
            Vector3 right = Vector3.Cross(Vector3.up, direction).normalized; // Right vector
            Vector3 up = Vector3.Cross(direction, right).normalized; // Up vector

            // Spread based on distance
            float distance = Vector3.Distance(start, end);
            float spreadFactor = distance / 5f; // Adjust spread intensity

            if (!distanceBasedSpread)
            {
                spreadFactor = 1;
            }

            // Apply random spread for horizontal (right) and vertical (up)
            float horizontalSpread = UnityEngine.Random.Range(-bulletVariance.x, bulletVariance.x) * spreadFactor;
            float verticalSpread = UnityEngine.Random.Range(-bulletVariance.y, bulletVariance.y) * spreadFactor;

            // Adjust the target position (end) with spread
            Vector3 spread = (horizontalSpread * right + verticalSpread * up);

            // Apply the spread to the target point (end)
            Vector3 adjustedEnd = end + spread;

            // Return the direction from start to the new, spread-adjusted end position
            return (adjustedEnd - start).normalized;
        }
        else
        {
            return (end - start).normalized;
        }

        #endregion
    }

    public void PlayAudio1()
    {
        audioSource.PlayOneShot(audio1);
    }

    public void PlayAudio2()
    {
        audioSource.PlayOneShot(audio2);
    }

    public void PlayAudio3()
    {
        audioSource.PlayOneShot(audio3);
    }
}
