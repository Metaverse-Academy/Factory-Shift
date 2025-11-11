using UnityEngine;
using UnityEngine.AI;


public class EnemyAi : MonoBehaviour
{

 public int maxHealth = 100; // *****صحة الوحش الكاملة
 private int currentHealth;
    


    private enum State { Patrol, Chase, Attack}
    private State currentState = State.Patrol;

    [SerializeField] Transform[] patrolPoints;
    [SerializeField] Transform player;
    [SerializeField] float chaseDistance = 5f;
    [SerializeField] float attackDistance = 2f;

    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Animator anim;
    private int currentIndex = 0;


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        Patrol();


         currentHealth = maxHealth; // في البداية يكون بكامل صحته***
    }

    void Update()
    {

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);


        Debug.Log(currentState);

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                agent.speed = 2f;
                if (distanceToPlayer <= chaseDistance)
                    currentState = State.Chase;
                break;
            case State.Chase:
                Chase();
                agent.speed = 4f;
                if (distanceToPlayer <= attackDistance)
                    currentState = State.Attack;
                else
                    currentState = State.Patrol;
                break;
            case State.Attack:
                Attack();
                if (distanceToPlayer > attackDistance)
                    currentState = State.Chase;
                break;

        }
    }

    void Patrol()
    {
        agent.isStopped = false;
        if (patrolPoints.Length == 0) return;
        if (!agent.pathPending && agent.remainingDistance < 0.8f)
        {
            agent.SetDestination(patrolPoints[currentIndex].position);
            currentIndex = (currentIndex + 1) % patrolPoints.Length;
        }

    }

    void Chase()
    {
        agent.isStopped = false;
        audioSource.Play();
        agent.SetDestination(player.position);
    }

    void Attack()
    {
        //Attack logic
        agent.isStopped = true;
        anim.SetTrigger("isAttacking");

    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseDistance);
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log("Enemy took damage! Current health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }
     void Die()
    {
        Debug.Log("Enemy died 💀 Bye loser");
        Destroy(gameObject); // يختفي الوحش****
    }


}