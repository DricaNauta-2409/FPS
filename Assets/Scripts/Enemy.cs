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

    // Sincronizado com a animação "Shoot" (40 frames a 24 fps = 1,667 s).
    // O Animator reinicia a animação em 95% dela, então um ciclo dura 1,667 x 0,95 = 1,583 s.
    [SerializeField] private float timeBetweenAttacks = 1.583f;

    // Momento da animação em que a pata da frente está no alto (frame 11 = 0,458 s).
    // É aqui que a bala sai. Ajuste se quiser o tiro um pouco antes ou depois.
    [SerializeField] private float shootDelay = 0.458f;

    private float nextShootTime;
    private bool isAttackingNow;
    private Collider[] myColliders;

    private void Awake()
    {
        myColliders = GetComponentsInChildren<Collider>();
    }
    private const string isChasing = "isChasing";
    private const string isAttacking = "isShooting";

   
    private void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        // Perto o bastante para atirar: ataca.
        // Dentro do alcance de perseguição (mas ainda longe): persegue.
        // Fora dos dois: fica parado.
        if (distance <= attackRange)
        {
            Attack();
        }
        else if (distance <= chaseRange)
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

        // Acabou de começar a atacar: a animação Shoot começa do zero agora,
        // então o primeiro tiro sai quando a pata sobe.
        if (!isAttackingNow)
        {
            isAttackingNow = true;
            nextShootTime = Time.time + shootDelay;
        }

        if (Time.time >= nextShootTime)
        {
            Shoot();
            // Soma ao horário anterior (e não ao Time.time) para não ir saindo do ritmo da animação.
            nextShootTime += timeBetweenAttacks;
        }
    }


    private void Shoot()
    {
        // A direção é calculada na hora: da boca (firepoint) até o player.
        // Assim não importa como a animação mexe o corpo, a bala sempre vai no player.
        Vector3 direction = player.position - firepoint.position;
        GameObject bullet = Instantiate(Bullet, firepoint.position, Quaternion.LookRotation(direction));

        // A boca fica dentro do colisor do próprio inimigo,
        // então a bala ignora os colisores dele para não se acertar.
        foreach (Collider bulletCollider in bullet.GetComponentsInChildren<Collider>())
        {
            foreach (Collider myCollider in myColliders)
            {
                Physics.IgnoreCollision(bulletCollider, myCollider);
            }
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
        isAttackingNow = false;
        agent.ResetPath();
        SetAnimation(false, false);
    }
    private void Chase()
    {
        isAttackingNow = false;
        agent.SetDestination(player.position);
        SetAnimation(true, false);
    }
    private void OnDrawGizmosSelected()
    {
        // Alcance de perseguição (amarelo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        // Alcance de ataque (vermelho)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Ponto de onde a bala sai (ciano) e a direção do tiro até o player
        if (firepoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(firepoint.position, 0.05f);
            if (player != null)
                Gizmos.DrawLine(firepoint.position, player.position);
        }
    }
}

