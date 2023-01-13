using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyBB : Blackboard
{
    #region Enemy Variables
    public Vector3 moveToLocation;
    public Vector3 rotateToLocation;

    private HealthManager healthManager;
    public bool isHealthy = false;

    public Perception perception;
    public FieldOfView fieldOfView;

    public Transform[] patrolLocations;
    public int locationNumber;

    public bool isGettingHealth;
    public bool isFleeing = false;
    public Vector3 fleeDir;
    
    private MeleeRangeCheck rangeCheck;
    public bool inMeleeRange;
    public GameObject meleeAttackTarget;

    public GameObject closestEntity;
    public GameObject lastKnownEntity;
    #endregion

    #region Player Variables
    public GameObject player;
    public Vector3 playerLocation;
    private PlayerController playerController;
    private HealthManager playerHM;
    public int playerHealth;
    public Vector3 playerVelocity;
    #endregion

    #region Companion Variables
    public GameObject companion;
    public Vector3 companionLocation;
    private CompanionController companionController;
    private HealthManager companionHM;
    private Vehicle companionVehicle;
    public int companionHealth;
    public Vector3 companionVelocity;
    #endregion

    //public bool debugTest = false;

    // Start is called before the first frame update
    void Start()
    {
        healthManager = this.GetComponent<HealthManager>();
        perception = GetComponent<Perception>();
        fieldOfView = GetComponent<FieldOfView>();
        rangeCheck = GetComponentInChildren<MeleeRangeCheck>();

        playerController = player.GetComponent<PlayerController>();
        playerHM = player.GetComponent<HealthManager>();

        companionController = companion.GetComponent<CompanionController>();
        companionHM = companion.GetComponent<HealthManager>();
        companionVehicle = companion.GetComponent<Vehicle>();

        locationNumber = UnityEngine.Random.Range(0, patrolLocations.Length);
    }

    // Update is called once per frame
    void Update()
    {
        isHealthy = healthManager.isHealthy;
        
        // For keeping track of other entities locations
        playerLocation = player.transform.position;
        companionLocation = companion.transform.position;

        // To evaluate whether they are strong enough to fight against other entities, or should flee
        playerHealth = playerHM.GetCurrentHealth();
        companionHealth = companionHM.GetCurrentHealth();

        // Velocity
        playerVelocity = playerController.GetVelocity();
        companionVelocity = companionVehicle.velocity;


        closestEntity = FindClosestEnemy();
        if (closestEntity != null)
        {
            //Debug.Log("Getting rotation location");
            rotateToLocation = closestEntity.transform.position;
        }
        lastKnownEntity = LastKnownEnemy();

        inMeleeRange = rangeCheck.inMeleeRange;
        meleeAttackTarget = rangeCheck.attackTarget;
    }

    public GameObject FindClosestEnemy()
    {
        //GameObject[] objects = FindObjectsOfType<GameObject>();
        //foreach (GameObject obj in objects)
        //{
        //    if (obj.layer == 8)
        //    {
        //        perception.memoryMap.Values.
        //    }
        //}

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
        

        //return null;

        //if (fieldOfView.visibleTargets.Count > 0)
        //{
        //    GameObject entity = fieldOfView.visibleTargets[0].gameObject;
        //    return entity;
        //}

        //return null;

        //List<GameObject> sensedObjects = new List<GameObject>();
        //GameObject closestEntity = null;
        //float distance = Mathf.Infinity;
        //Dictionary<GameObject, MemoryRecord> memoryMap = perception.memoryMap;
        //Debug.Log("bb.perception.memoryMap = " + memoryMap.ToString());

        //if (memoryMap.Count > 0)
        //{
        //    Debug.Log("MEMORY MAP IS GREATER THAN 0");

        //    foreach (KeyValuePair<GameObject, MemoryRecord> memory in perception.memoryMap)
        //    {
        //        sensedObjects.Add(memory.Key);
        //        closestEntity = memory.Key;
        //        Debug.Log("Memory.key = " + memory.Key);
        //    }



        //    foreach (GameObject entity in sensedObjects)
        //    {

        //        Debug.Log("Entity: " + entity + " in sensedObjects");
        //        Vector3 pos = entity.transform.position;
        //        Vector3 diff = pos - this.transform.position;
        //        float curDistance = diff.sqrMagnitude;

        //        if (curDistance < distance)
        //        {
        //            closestEntity = entity;
        //            distance = curDistance;
        //        }
        //    }
        //    Debug.Log("Closest Entity = " + closestEntity);
        //    return closestEntity;
        //}
        //else
        //{
        //    return null;
        //}

    }

    public GameObject LastKnownEnemy()
    {
        MemoryRecord[] sensedRecord = new MemoryRecord[perception.memoryMap.Values.Count];

        if (sensedRecord.Length > 0)
        {
            perception.memoryMap.Values.CopyTo(sensedRecord, 0);
            GameObject[] sensedObjects = new GameObject[perception.memoryMap.Keys.Count];
            perception.memoryMap.Keys.CopyTo(sensedObjects, 0);

            double latestTime = Mathf.Infinity;
            GameObject lastKnownEnemy = null;

            for (int i = 0; i < sensedRecord.Length; i++)
            {
                TimeSpan interval = DateTime.Now - sensedRecord[i].timeLastSensed;
                //Debug.Log("Interval = " + interval);
                double curTime = interval.TotalSeconds;

                if (curTime < latestTime)
                {
                    lastKnownEnemy = sensedObjects[i];
                }
            }

            return lastKnownEnemy;
        }
        else
        {
            return null;
        }
    }

    public void RandomisePatrolPoint()
    {
        locationNumber = UnityEngine.Random.Range(0, patrolLocations.Length);
    }
}
