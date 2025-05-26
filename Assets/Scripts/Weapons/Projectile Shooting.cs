using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;

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

    [SerializeField] [Range(0,1500)] float fireRate;

    [SerializeField] Vector2 bulletSpread;

    [SerializeField] int numberOfBullets = 1;

    [SerializeField] float bulletForce;

    [SerializeField] [ShowIf(nameof(IsBurstMode))] int numberOfBulletsInBurst = 0;
    [SerializeField] [ShowIf(nameof(IsBurstMode))] float timeBetweenBursts;

    [SerializeField] float knockBackForce;

    bool bursting;

    float timeBetweenShots;
    float nextFireTime;

    [SerializeField] LayerMask whatIsShootable;

    [SerializeField] InputActionReference fireInput;
    [SerializeField] Transform attackPoint;
    [SerializeField] GameObject muzzleFlash;
    Transform aimingCameraContainer;
    Reloading reloadingScript;
    Audio audioPlayer;
    Rigidbody playerRig;

    AbstractGunAnimator gunAnimator;
    [SerializeField] GameObject bullet;

    void Start()
    {
        gunAnimator = GetComponent<AbstractGunAnimator>();
        timeBetweenShots = 60/fireRate;
        Debug.Log ("Time Between Shots: " + timeBetweenShots);

        aimingCameraContainer = GameObject.Find("Aiming Camera Container").transform;
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
            ProjectileFire();
        }

        gunAnimator.Fire();
        audioPlayer.FireSound();

        Instantiate(muzzleFlash, attackPoint.position, attackPoint.rotation);

        playerRig.AddForce(-aimingCameraContainer.forward * knockBackForce, ForceMode.Impulse);
    }

    void ProjectileFire()
    {
        Vector3 bulletDir = (GetTargetPoint() - attackPoint.position).normalized;

        GameObject curBullet = Instantiate(bullet, attackPoint.position, Quaternion.identity);

        curBullet.transform.forward = bulletDir;

        Rigidbody curRig = curBullet.GetComponent<Rigidbody>();

        curRig.AddRelativeForce(curBullet.transform.forward * bulletForce, ForceMode.Impulse);
    }

    Vector3 GetTargetPoint()
    {
        Vector3 direction = Quaternion.AngleAxis(Random.Range(-bulletSpread.x, bulletSpread.x), aimingCameraContainer.up) * Quaternion.AngleAxis(Random.Range(-bulletSpread.y, bulletSpread.y), aimingCameraContainer.right) * aimingCameraContainer.forward;

        Ray preRay = new(aimingCameraContainer.position , direction.normalized);
        
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

    // Helper method for NaughtyAttributes
    private bool IsBurstMode()
    {
        return fireMode == FireMode.AutoBurst || fireMode == FireMode.SemiBurst;
    }
}
