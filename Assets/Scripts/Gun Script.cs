using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
using Debug = UnityEngine.Debug;
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
        VerySmall,
        Small,
        Medium,
        Big,
        VeryBig,
        VeryVeryBig
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

    private bool isInFireLoop = false;
    //--------------------Bullet Spread--------------------//
    [Header("Bullet Spread")]
    public bool useBulletSpread;
    public Vector2 bulletVariance;

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

    //--------------------References--------------------//
    [Header("References")]
    public LayerMask layerMask;
    public Transform attackPoint;
    private GameObject shootCenter;
    private Rigidbody playerRig;
    private Animator anim;
    private AudioSource audioSource;
    private PlayerScript playerScript;

    //--------------------Positions--------------------//
    public Vector3[] weaponPos = new Vector3[4];
    public int positionIndex = 0;
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

    private AnimationScript animController;
    //--------------------Input--------------------//
    public InputActionReference fireAction;
    public InputActionReference reloadAction;

    #region Start

    void Awake() // Called Before First Frame
    {
        DefineComponents();

        SetAnimClipSizes();

        Debug.Log("Max Fire Rate: " + 60/fireAnimTime);
    }

    void DefineComponents() // Called in Awake(). Sets Referencs
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        shootCenter = GameObject.Find("Camera Container");
        delayBetweenShots = 60/fireRateMin;

        playerScript = GetComponentInParent<PlayerScript>();
        playerRig = playerScript.gameObject.GetComponent<Rigidbody>();

        animController = playerScript.animController;
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
    #endregion
    #region Update
    void Update()
    {
        if (fireAction.action.WasPressedThisFrame() && !reloading)
        {
            if (curMag != 0)
            {
                if (fireMode == FireMode.SemiAuto && CanFire())
                {
                    SemiAutoFire();
                }
                else if (fireMode == FireMode.Auto && CanFire())
                {
                    FullAutoFire();
                }
            }
            else
            {
                StartCoroutine(WaitForReload());
            }
        }
        else if (!isInFireLoop && CanFire() && ReadyToAnim() && reloadAction.action.WasPressedThisFrame())
        {
            Reload();
        }

        // attackPointPos = attackPoint.position;
    }

    void LateUpdate() {
        attackPointPos = attackPoint.position;
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

    void SemiAutoFire() // Called from PlayerScript.OnFireInput()
    {
        if (!useBurst)
        {
            Shoot();
        }
        else if (!curBursting && useBurst)
        {
            StartCoroutine(RepeatFire(numberOfShotsInBurst));
        }
    }

    void FullAutoFire() // Called from PlayerScript.OnFireInput() and PlayerScript.FixedUpdate
    {
        StartCoroutine(AutoFireLoop());
    }

    IEnumerator AutoFireLoop()
    {
        isInFireLoop = true;

        while (fireAction.action.IsPressed())
        {
            if (curMag != 0)
            {
                if (!useBurst)
                {
                    Shoot();
                }
                else if (!curBursting && useBurst)
                {
                    StartCoroutine(RepeatFire(numberOfShotsInBurst));
                }
                yield return new WaitUntil(CanFire);
            }
            else
            {
                yield return new WaitUntil(() => Time.time > nextFireTime);
                Reload();
               // StartCoroutine(WaitForReload());
            }
        }

        isInFireLoop = false;
    }

    IEnumerator WaitForReload()
    {
        yield return new WaitUntil(() => Time.time > nextFireTime);
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

        yield return new WaitForSeconds(delayBetweenBursts - delayBetweenShots);
        curBursting = false;
    }

    private Vector3 attackPointPos;

    void Shoot() // Called from SemiAutoFire(), FullAutoFire() and RepeatFire(). Shoots
    {
        nextFireTime = (float)(Time.timeAsDouble + delayBetweenShots);
        curMag -= 1;
        for (int i = 0; i < numberOfBullets; i++)
        {
            Vector3 preRayDirection = preRayGenerator(shootCenter.transform.forward);
            Vector3 preRayOrigin = shootCenter.transform.position;

            Vector3 targetPoint;

            Ray preRay = new(preRayOrigin, preRayDirection);
            
            Debug.DrawRay(preRayOrigin, preRayDirection, Color.red, 5);

            if (Physics.Raycast(preRay, out RaycastHit preHit, Mathf.Infinity, layerMask))
            {
                // If it hits, our target position is at hit.point
                targetPoint = preHit.point;
            }
            else
            {
                // Else, chose a random distance.
                targetPoint = preRay.GetPoint(75);
            }

            Vector3 bulletDirection = (targetPoint - attackPointPos).normalized;

            if (bulletType == BulletType.Raycast)
            {
                // Define the ray
                var ray = new Ray(attackPointPos, bulletDirection);
    
                Debug.DrawRay(attackPointPos, bulletDirection, Color.blue, 5);
    
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    StartCoroutine(RaycastSpawnTrail(attackPointPos, targetPoint, hit.normal, true));
                    // If our RayCast hit a rigidbody, add a force
                    hit.rigidbody?.AddForce(ray.direction * hitForce, ForceMode.Impulse);
                }
                else
                {
                    StartCoroutine(RaycastSpawnTrail(attackPointPos, targetPoint, Vector3.zero, false));
                }
                Debug.Log("Shot With RayCast");
            }
            else if (bulletType == BulletType.Projectile)
            {
                // Do the Visual bullet passing the direciton
                VisualBullet(bulletDirection);
                Debug.Log("Shot With Projectile");
            }
        }

        if (usePositionalRecoil)
            playerRig.AddForce(-shootCenter.transform.forward * positionalRecoilForce, ForceMode.Impulse);
        // Play "Fire" animation
        anim.Play("Fire", 0, 0.0f);
        Recoil();
        // Play the audio fireAudio
        audioSource.PlayOneShot(fireAudio);
        // Particle for Muzzle Flash
        ParticleSystem muzzleParticle = Instantiate(muzzleParticleSystem, attackPoint.position, Quaternion.identity);
        muzzleParticle.gameObject.transform.parent = transform.GetChild(0);
    }

    void Recoil()
    {
        switch (recoilSize)
        {
            case RecoilSize.VerySmall:
                animController.Recoil(positionIndex, 0);
                break;
            case RecoilSize.Small:
                animController.Recoil(positionIndex, 1);
                break;
            case RecoilSize.Medium:
                animController.Recoil(positionIndex, 2);
                break;
            case RecoilSize.Big:
                animController.Recoil(positionIndex, 3);
                break;
            case RecoilSize.VeryBig:
                animController.Recoil(positionIndex, 4);
                break;
            case RecoilSize.VeryVeryBig:
                animController.Recoil(positionIndex, 5);
                break;
        }
    }

    private IEnumerator RaycastSpawnTrail(Vector3 start, Vector3 end, Vector3 HitNormal, bool MadeImpact) // Called from Shoot(). Moves Trail along Raycast path
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

    void VisualBullet(Vector3 direction) // Called from Shoot(). Spawns bullet and adds forces
    {
        // Create the Visual Bullet
        GameObject curBullet = Instantiate(bulletModel, Vector3.zero, Quaternion.identity);
        // Create the BulletTrail and attach it to curBullet
        Instantiate(bulletTrail, Vector3.zero, Quaternion.identity, curBullet.transform);
        // Set Pos
        curBullet.transform.position = attackPointPos;
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

    Vector3 preRayGenerator(Vector3 originDirection)
    {
        Vector3 direction = originDirection.normalized;

        if (useBulletSpread)
        {
            direction = Quaternion.AngleAxis(UnityEngine.Random.Range(-bulletVariance.x, bulletVariance.x), shootCenter.transform.up) * direction;
            direction = Quaternion.AngleAxis(UnityEngine.Random.Range(-bulletVariance.y, bulletVariance.y), shootCenter.transform.right) * direction;
        }

        return direction;
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

    bool CanFire()
    {
        return (Time.fixedTime >= nextFireTime - Time.deltaTime);
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(attackPoint.position, new Vector3(0.1f, 0.1f, 0.1f));
        // attackPointPos = attackPoint.position;
    }
    #endregion
}
