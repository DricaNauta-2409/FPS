using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;


   
    [SerializeField] private Transform firepoint;
    [SerializeField] private GameObject Bullet;

    [UnitHeaderInspectable("Ranges")]
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float attackRange = 10f;

    [SerializeField] private float timeBetweenAttacks = 1f;

    private float nextShootTime;

   
    private void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= chaseRange)
        {
            Chase();
        }
        else if (distance <= attackRange)
        {
            Chase();
        }
        else
        {
            Idle();
        }
    }

    private void Attack()
    {
        agent.ResetPath();
        LookAtPlayer();

        if (Time.time >= nextShootTime)
        {
            Instantiate(Bullet, firepoint.position, firepoint.rotation);
            nextShootTime = Time.time + timeBetweenAttacks;
        }
    }


    private void LookAtPlayer()
    {
        Vector3 target = player.position;
        target.y = transform.position.y;
        transform.LookAt(target);
    }

    private void Idle()
    {
        agent.ResetPath();
    }
    private void Chase()
    {
        agent.SetDestination(player.position);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}

