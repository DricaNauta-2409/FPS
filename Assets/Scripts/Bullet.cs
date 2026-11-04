using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed = 60f;

   // Start is called before the first frame update
   void Start()
    {
        Launch();

    }

    private void Launch()
    {
        rb.AddForce(transform.forward * speed, ForceMode.VelocityChange);
    }

  
}
