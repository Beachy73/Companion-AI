using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HealthManager))]
public class CompanionController : MonoBehaviour
{
    #region Companion Variables

    private FiniteStateMachine<CompanionController> FSM;

    public GameObject player;
    private PlayerController playerController;
    private SphereCollider playerSphereCollider;
    private HealthManager playerHealthManager;
    private Perception perception;
    public FieldOfView fieldOfView;

    [HideInInspector]
    public bool hitPlayerSphere = false;

    public float hunger = 0.0f;
    /// <summary>
    /// Amount of seconds it takes to reach max hunger
    /// </summary>
    public float maxHunger = 20.0f;
    [HideInInspector]
    public bool hasReachedMaxHunger = false;

    private HealthManager companionHealthManager;
    //[Range(0, 200)]
    //public int healthyLevel = 50;
    //public float isHealthyDivisor = 5f;
    public bool isHealthy = true;

    public GameObject closestEnemy;
    public bool enemyNear = false;

    public GameObject projectilePrefab;
    public float startTimeBtwShots = 0.8f;

    #endregion

    private void Awake()
    {
        Debug.Log("Companion awakes...");
        FSM = new FiniteStateMachine<CompanionController>();

        //// @TODO Implement idle/patrol state, set as initial state
        FSM.Configure(this, new IdleState());

        //FSM.Configure(this, new FollowPlayerState());

        playerController = player.GetComponent<PlayerController>();
        playerSphereCollider = player.GetComponentInChildren<SphereCollider>();
        playerHealthManager = player.GetComponent<HealthManager>();
        //playerSphereCollider = player.GetComponent<SphereCollider>();

        companionHealthManager = GetComponent<HealthManager>();
        perception = GetComponent<Perception>();
        fieldOfView = GetComponent<FieldOfView>();
    }

    private void Start()
    {
        closestEnemy = FindClosestEnemy();
    }

    void Update()
    {
        FSM.Update();

        //Debug.Log("Current State = " + FSM.GetCurrentState());


        if (hunger < maxHunger && hasReachedMaxHunger == false)
        {
            hunger += 1 * Time.deltaTime;
        }

        if (companionHealthManager.GetCurrentHealth() >= companionHealthManager.isHealthyLevel)
        {
            companionHealthManager.isHealthy = true;
        }
        else
        {
            companionHealthManager.isHealthy = false;
        }

        isHealthy = companionHealthManager.isHealthy;
        //if (isHealthy == false /*&& NO ENEMIES AROUND*/)
        //{
        //    ChangeState(new FindHealthState());
        //}

        //Debug.Log("Companion Hunger = " + hunger);

        if (Input.GetKeyDown(KeyCode.B))
        {
            companionHealthManager.ChangeHealth(-30);
        }

        if (playerHealthManager.GetCurrentHealth() <= 0)
        {
            ChangeState(new IdleState());
        }

        closestEnemy = FindClosestEnemy();
        enemyNear = EnemyNear();
    }

    public void ChangeState(FSMState<CompanionController> companion)
    {
        FSM.ChangeState(companion);
    }

    public void RevertToPreviousState()
    {
        FSM.RevertToPreviousState();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == playerSphereCollider)
        {
            hitPlayerSphere = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == playerSphereCollider)
        {
            hitPlayerSphere = false;
        }
    }

   public FiniteStateMachine<CompanionController> GetFiniteStateMachine()
    {
        return FSM;
    }

    public bool EnemyNear()
    {
        if (perception.memoryMap.Values.Count > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public GameObject FindClosestEnemy()
    {
        GameObject[] sensedObjects = new GameObject[perception.memoryMap.Count];

        if (sensedObjects.Length >= 0)
        {
            perception.memoryMap.Keys.CopyTo(sensedObjects, 0);
            GameObject entity = null;
            float distance = Mathf.Infinity;

            MemoryRecord[] sensedRecord = new MemoryRecord[perception.memoryMap.Count];
            perception.memoryMap.Values.CopyTo(sensedRecord, 0);

            for (int i = 0; i < sensedObjects.Length; i++)
            {
                Vector3 diff = sensedObjects[i].transform.position - this.transform.position;
                float curDistance = diff.sqrMagnitude;

                if (sensedRecord[i].withinFoV == false)
                {
                    curDistance = Mathf.Infinity;
                }

                if (curDistance < distance)
                {
                    entity = sensedObjects[i];
                    distance = curDistance;
                }
            }
            return entity;
        }
        else
        {
            return null;
        }
    }
}
