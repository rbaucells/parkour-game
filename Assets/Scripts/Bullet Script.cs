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

    void Start()
    {
        rig = GetComponent<Rigidbody>();
        Invoke("DelayDelete", 3.0f);
    }
    void OnCollisionEnter (Collision other)
    {
        Debug.Log("Collided at:" + other.GetContact(0).point);
        
        Instantiate(impactParticleSystem, other.GetContact(0).point, Quaternion.LookRotation(other.GetContact(0).normal));
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
