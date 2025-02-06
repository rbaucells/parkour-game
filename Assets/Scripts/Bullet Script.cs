using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    private Rigidbody rig;
    [HideInInspector]
    public float gravity;
    [HideInInspector]
    public float impactForce;
    [HideInInspector]
    public Vector3 direction;
    [HideInInspector]
    public ParticleSystem impactParticleSystem;
    [HideInInspector]
    public float actionForce;
    [HideInInspector]
    public float actionRadius;
    [HideInInspector]
    public float explosionUpForce;
    [HideInInspector]
    public int action;

    void Start()
    {
        rig = GetComponent<Rigidbody>();
        Invoke("DelayDelete", 3.0f);
    }
    void OnCollisionEnter (Collision other)
    {
        Instantiate(impactParticleSystem, other.GetContact(0).point, Quaternion.LookRotation(other.GetContact(0).normal));
        
        if (action == 0) // Aka explode
        {
            Boom.Explode(transform.position, actionRadius, actionForce, explosionUpForce);
        }
        else if (action == 1)
        {
            Boom.Implode(transform.position, actionRadius, actionForce);
        }

        Destroy(gameObject);
    }

    void DelayDelete()
    {
        Destroy(gameObject);
    }

    void FixedUpdate() 
    {
        rig.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }
}
