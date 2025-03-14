using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public enum ImpactAction
    {
        Explode,
        Implode,
        None
    }

    [SerializeField] ImpactAction impactAction;

    [SerializeField] float gravity;
    [SerializeField] float bulletDestroyTime;

    [SerializeField] float actionForce;
    [SerializeField] float actionRadius;
    [SerializeField] float explosionUpForce;

    [SerializeField] GameObject impactParticle;
    Rigidbody rig;

    void Awake()
    {
        rig = GetComponent<Rigidbody>();

        StartCoroutine(DestroyBullet(bulletDestroyTime));
    }

    void FixedUpdate()
    {
        rig.AddForce(Vector3.down * gravity, ForceMode.Acceleration);       
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided with: " + collision.collider.gameObject.name);
        switch (impactAction)
        {
            case ImpactAction.Explode:
                Boom.Explode(transform.position, actionRadius, actionForce, explosionUpForce);
                break;
            case ImpactAction.Implode:
                Boom.Implode(transform.position, actionRadius, actionForce);
                break;
        }

        Instantiate(impactParticle, transform.position, Quaternion.Euler(collision.GetContact(0).normal));

        StartCoroutine(DestroyBullet(Time.deltaTime));
    }

    IEnumerator DestroyBullet(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
