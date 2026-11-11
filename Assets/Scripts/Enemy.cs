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
    [SerializeField] private Animator animator;
    [UnitHeaderInspectable("Ranges")]
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float attackRange = 10f;

    [SerializeField] private float timeBetweenAttacks = 1f;

    private float nextShootTime;
    private const string isChasing = "isChasing";
    private const string isAttacking = "isShooting";

   
    private void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= chaseRange)
        {
            Attack();
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
        SetAnimation(false, true); 

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

    private void SetAnimation(bool chasing, bool sooting)
    {
        animator.SetBool(isChasing, chasing);
        animator.SetBool(isAttacking, sooting);
    }

    private void Idle()
    {
        agent.ResetPath();
        SetAnimation(false, false);
    }
    private void Chase()
    {
        agent.SetDestination(player.position);
        SetAnimation(true, false);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}

