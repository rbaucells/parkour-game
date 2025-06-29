using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(TrailRenderer), typeof(Collider))]

public class ProjectileBullet : MonoBehaviour
{
    [HideInInspector] public ObjectPool impactObjectPool;
    [HideInInspector] public float gravity;

    Rigidbody rig;

    void OnEnable()
    {
        // get reference
        rig = GetComponent<Rigidbody>();
        // turn physics back on
        rig.isKinematic = false;
        // reset the velocities
        rig.velocity = Vector3.zero;
        rig.angularVelocity = Vector3.zero;
    }

    void OnCollisionEnter(Collision collision)
    {
        // turn physics off
        rig.isKinematic = true;
        // get an impact particle system
        GameObject curImpact = impactObjectPool.GetObject();
        // get contactPoint
        ContactPoint contactPoint = collision.GetContact(0);
        // put it where it needs to go
        curImpact.transform.SetPositionAndRotation(contactPoint.point, Quaternion.Euler(contactPoint.normal));
    }

    void FixedUpdate()
    {
        // apply gravity
        rig.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }
}
