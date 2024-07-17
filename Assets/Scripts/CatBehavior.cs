using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum CatState
{
  Waiting,
  Pursuing,
  Caught,
  Patrolling
}

public class Cat : MonoBehaviour
{
    [SerializeField]
    private float DetectionRadius;

    [SerializeField]
    private float StopRadius;

    [SerializeField]
    private Transform[] PatrolSpots;
    private int CurrentSpot = 0;

    [SerializeField]
    private float Speed;

    [SerializeField]
    private float WaitTime;

    private Transform Target;

    private CatState State;

    private Vector2 TargetVelocity;
    private Animator animator;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        Speed = 200;
        State = CatState.Waiting;
        TargetVelocity = Vector2.zero;
        StartCoroutine(WaitForPatrol(WaitTime));

        if (GameObject.FindWithTag("Player") == null)
        {
            Debug.LogError("Player not found for Cat");
        }
        else
        {
            Target = GameObject.FindWithTag("Player").transform;
        }

        if (!TryGetComponent<Rigidbody2D>(out rb))
        {
            Debug.LogError("Rigidbody2D not found for Cat");
        }

        if (!TryGetComponent<Animator>(out animator))
        {
            Debug.LogError("Animator not found for Cat");
        }
        else
        {
            animator.SetBool("Walking", false);
        }
    }

    void Update()
    {
        if (State == CatState.Patrolling)
        {
            Debug.DrawLine(transform.position, PatrolSpots[CurrentSpot].position, Color.green);
        }
        else if (State == CatState.Pursuing)
        {
            Debug.DrawLine(transform.position, Target.position, Color.red);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Always clear previous target velocity
        TargetVelocity = Vector2.zero;

        // Check if player is in range
        DetectPlayer();

        // Update based on current state
        switch (State)
        {
            case CatState.Patrolling:
                // Move towards next point in patrol
                Patrol();
                break;

            case CatState.Pursuing:
                // Walk towards target
                TargetVelocity = (Target.position - transform.position).normalized;
                break;
        }

        // Update walk state
        CatWalk(TargetVelocity);
    }

    IEnumerator WaitForPatrol(float WaitSeconds)
    {
        Debug.Log($"Waiting for {WaitSeconds} seconds");
        // Change to waiting then wait
        State = CatState.Waiting;
        yield return new WaitForSeconds(WaitSeconds);

        // If still waiting, pick next spot and return to patrolling
        if (State == CatState.Waiting)
        {
            CurrentSpot = (CurrentSpot + 1) % PatrolSpots.Length;
            Debug.Log($"Patrolling to spot {CurrentSpot}");
            State = CatState.Patrolling;
        }
    }

    public void DetectPlayer()
    {
        float Dist = Vector3.Distance(Target.position, transform.position);
        if (Dist <= StopRadius)
        {
            if (State != CatState.Caught)
            {
                Debug.Log("Caught the player!");
            }
            State = CatState.Caught;
        }
        else if(Dist <= DetectionRadius)
        {
            if (State != CatState.Pursuing)
            {
                Debug.Log("Pursuing the player");
            }
            State = CatState.Pursuing;
        }
        else if(State == CatState.Pursuing || State == CatState.Caught)
        {
            StartCoroutine(WaitForPatrol(WaitTime));
        }
    }

    void Patrol()
    {
        Vector3 PatrolPos = PatrolSpots[CurrentSpot].position;
        float Dist = Vector3.Distance(PatrolPos, transform.position);
        Debug.Log($"Distance to patrol spot: {Dist}");
        if (Dist > StopRadius)
        {
            TargetVelocity = (PatrolPos - transform.position).normalized;
        }
        else
        {
            float RandomWait = Random.Range(WaitTime / 2, WaitTime * 2);
            StartCoroutine(WaitForPatrol(RandomWait));
        }
    }

    void CatWalk(Vector2 Direction)
    {
        if (Direction == Vector2.zero)
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("Walking", false);
        }
        else
        {
            rb.velocity = Speed * Time.fixedDeltaTime * Direction;
            animator.SetBool("Walking", true);
            animator.SetFloat("Horizontal", Direction.x);
            animator.SetFloat("Vertical", Direction.y);
        }
    }
}
