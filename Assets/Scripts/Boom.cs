using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using UnityEngine;

public static class Boom  {

    public static void Explode(Vector3 position, float radius, float force, [Optional] float explosionUpForce)
    {
        Collider[] colliders = Physics.OverlapSphere(position, radius);
        foreach (var collider in colliders)
        {
            if (collider.gameObject.GetComponent<Rigidbody>() != null)
            {
                collider.gameObject.GetComponent<Rigidbody>().AddExplosionForce(force, position, radius, explosionUpForce);
            }
        }
    }

    public static void Implode(Vector3 position, float radius, float force)
    {
        Collider[] colliders = Physics.OverlapSphere(position, radius);
        foreach (var collider in colliders)
        {
            if (collider.gameObject.GetComponent<Rigidbody>() != null)
            {
                collider.gameObject.GetComponent<Rigidbody>().AddForce((position - collider.transform.position).normalized * force, ForceMode.Impulse);
            }   
        }
    }
}