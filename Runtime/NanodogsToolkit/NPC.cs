using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{
    [Header("Dialogue Details")]
    public string NpcName;

    [Header("Other Details")]

    public NpcType npcType;
    public enum NpcType
    {
        Friendly, // can join Player's squad
        Enemy, // can only attack
        Bystander // cant join squad or attack.
    }
    public Animator animator;
    public float tooCloseRadius = 3f;  // Radius at which NPC flees from player
    public float fleeDistance = 2f;    // Distance NPC moves back when fleeing
    public float stopDistance = 2f;    // Distance at which NPC stops fleeing
    public float followRadius = 10f;   // Radius at which NPC starts following the player

    bool isInSquad = true;
    bool isMoving;
    bool hasReachedSafeDistance = false;

    GameObject player;
    NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void OnJoinSquad()
    {
        Debug.Log(NpcName + " has joined player's squad");
        isInSquad = true;
    }

    void Update()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Looking again!");
            player = GameObject.FindGameObjectWithTag("Player");
        }

        if (isInSquad)
        {
            // Use OverlapSphere to detect nearby objects
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, followRadius);
            bool playerNearby = false;
            bool playerTooClose = false;

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    playerNearby = true;

                    // Check if the player is too close to the NPC
                    if (Vector3.Distance(hitCollider.transform.position, transform.position) < tooCloseRadius)
                    {
                        playerTooClose = true;
                        break; // No need to check further once we know the player is too close
                    }
                }
            }

            if (playerTooClose)
            {
                FleeFromPlayer();
            }
            else if (playerNearby)
            {
                FollowPlayer();
            }
            else
            {
                StopFollowing();
            }

            // Set animator state based on movement
            animator.SetBool("Walking", isMoving);

            // Determine whether the NPC is moving
            if (agent.velocity.sqrMagnitude > 0.1f)
            {
                isMoving = true;
            }
            else
            {
                isMoving = false;
            }
        }
    }

    void FleeFromPlayer()
    {
        // Make the NPC take a few steps back from the player
        Vector3 directionAwayFromPlayer = transform.position - player.transform.position;
        Vector3 fleePosition = transform.position + directionAwayFromPlayer.normalized * fleeDistance;

        // Set the destination to the flee position
        agent.SetDestination(fleePosition);
        hasReachedSafeDistance = false;
    }

    void FollowPlayer()
    {
        // Set the destination to the player's position (outside of the followRadius)
        agent.SetDestination(player.transform.position);
        hasReachedSafeDistance = false;
    }

    void StopFollowing()
    {
        // Stop NPC movement when the player is inside the followRadius
        agent.SetDestination(transform.position);  // Set destination to current position to stop
        hasReachedSafeDistance = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, tooCloseRadius);
        Gizmos.DrawWireSphere(transform.position, followRadius);
    }
}
