using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
        [SerializeField] private float speed;
        [SerializeField] private float lifeTime = 5f;
        [SerializeField] private float damage = 10f;
    
        [SerializeField] private GameObject impactEffectPrefab;
    
        void Start()
        {
            Launch();
            Destroy(gameObject, lifeTime);
        }
    
        private void Launch()
        {
            rb.AddForce(transform.forward * speed, ForceMode.VelocityChange);
        }
    
        private void OnCollisionEnter(Collision collision)
        {
            // Mostra no Console em que objeto a bala bateu (ajuda a achar problemas de dano)
            Debug.Log($"{name} bateu em {collision.gameObject.name} (layer {LayerMask.LayerToName(collision.gameObject.layer)})", collision.gameObject);

            SpawnImpact(collision);

            if (collision.gameObject.TryGetComponent(out Health health))
            {
                health.TakeDamage(damage);
            }
    
            Destroy(gameObject);
        }
    
        private void SpawnImpact(Collision collision)
        {
            Vector3 point = transform.position;
            Vector3 normal = -transform.forward;
    
            if(collision.contactCount > 0)
            {
                ContactPoint contact = collision.GetContact(0);
                point = contact.point;
                normal = contact.normal;
            }
    
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, normal);
            Instantiate(impactEffectPrefab, point + normal * 0.02f, rotation);
    
        }

  
}
