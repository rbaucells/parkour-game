using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ProjectileBullet : MonoBehaviour
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

        Invoke(nameof(DestroyBullet), bulletDestroyTime);
    }

    void FixedUpdate()
    {
        rig.AddForce(Vector3.down * gravity, ForceMode.Acceleration);       
    }

    void OnCollisionEnter(Collision collision)
    {
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
        DestroyBullet();
    }

    void DestroyBullet()
    {
        Destroy(gameObject);
    }
}
