using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyBB))]
[RequireComponent(typeof(HealthManager))]
public class EnemyController : MonoBehaviour
{
    #region Enemy Variables

    public GameObject projectile;

    private EnemyBB bb;
    private BTNode BTRootNode;

    private HealthManager enemyHealthManager;

    [Range(0, 20)]
    public float moveSpeed = 5f;
    [Range(0, 20)]
    public float patrolWaitTime = 2.0f;

    //[HideInInspector]
    public Vector3 dir;
    private Vector3 moveLocation;
    public bool isMoving = false;
    private Vector3 rotateLocation;
    private bool isRotating = false;
    private Quaternion lookRotation;
    public float roationDamping = 8f;

    //private float fireTimer = 0;
    //public float maxFiringTime;

    private GameObject closestEntity;
    public GameObject testObject;


    #endregion

    private void Awake()
    {
        enemyHealthManager = GetComponent<HealthManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //////////////////////////////////////////////////////////////////////////
        //////////////////// Creating Enemy Behaviour Tree ///////////////////////
        //////////////////////////////////////////////////////////////////////////
        
        // Reference to Enemy Blackboard
        bb = GetComponent<EnemyBB>();

        // Create root selector
        Selector rootChild = new Selector(bb);
        BTRootNode = rootChild;

        #region Combat Sequence
        /////////////////////////// Combat Sequence //////////////////////////////
        CompositeNode combatSelector = new Selector(bb);    // Sequence of actions to take in combat
        CombatDecorator combatRoot = new CombatDecorator(combatSelector, bb);   // Defines condition required to enter the combat sequence

        // Defining Sequence for Gun Combat
        CompositeNode gunCombatSequence = new Sequence(bb);
        GunCombatDecorator gunCombatRoot = new GunCombatDecorator(gunCombatSequence, bb);
        //gunCombatSequence.AddChild(new RotateTowards(bb, this, closestEntity));
        //gunCombatSequence.AddChild(new RotateTowards(bb, this, bb.player));
        //gunCombatSequence.AddChild(new RotateTowards(bb, this, bb.fieldOfView.visibleTargets[0].gameObject));
        //gunCombatSequence.AddChild(new RotateTowards(bb, this, testObject));

        //gunCombatSequence.AddChild(new GetEnemyRotationLocation(bb));
        //gunCombatSequence.AddChild(new EnemyRotateTo(bb, this));
        //gunCombatSequence.AddChild(new EnemyWaitTillRotated(bb, this));
        //gunCombatSequence.AddChild(new EnemyStopRotation(bb, this));


        gunCombatSequence.AddChild(new FireWeapon(bb, this));
        // Rotate towards Enemy
        // Attack

        // Defining Sequence for Melee Combat
        CompositeNode meleeCombatSequence = new Sequence(bb);
        MeleeCombatDecorator meleeCombatRoot = new MeleeCombatDecorator(meleeCombatSequence, bb);
        meleeCombatSequence.AddChild(new MeleeAttack(bb, this));
        meleeCombatSequence.AddChild(new DelayNode(bb, 1f));

        
        combatSelector.AddChild(gunCombatRoot);
        combatSelector.AddChild(meleeCombatRoot);
        //combatSelector.AddChild(fleeRoot);
        // combatSequence.AddChild(new MoveToEnemyLocation);


        #endregion

        #region Hunt Sequence
        //////////////////////////// Hunt Sequence ///////////////////////////////
        CompositeNode huntSequence = new Sequence(bb);  // Sequence of action to take in hunt
        HuntDecorator huntRoot = new HuntDecorator(huntSequence, bb);   // Defines condition required to enter the hunt sequence
        huntSequence.AddChild(new CalculateLastKnownLocation(bb, this));
        huntSequence.AddChild(new EnemyMoveTo(bb, this));
        huntSequence.AddChild(new EnemyWaitTillAtLocation(bb, this));
        huntSequence.AddChild(new EnemyStopMovement(bb, this));
        // Move to enemies last known location
        // Look around for enemy
        #endregion

        #region Flee Sequence
        // Defining Sequence for Flee
        CompositeNode fleeSequence = new Sequence(bb);
        FleeDecorator fleeRoot = new FleeDecorator(fleeSequence, bb);
        // Calculate flee location
        fleeSequence.AddChild(new CalculateFleeLocation(bb, this));
        //fleeSequence.AddChild(new EnemyMoveTo(bb, this)); // Move to closest...
        //fleeSequence.AddChild(new EnemyWaitTillAtLocation(bb, this));  // Wait till enemy reaches destination
        fleeSequence.AddChild(new EnemyStopMovement(bb, this));    // Stop movement
        #endregion

        #region Health Sequence
        ////////////////////////// Get Health Sequence ///////////////////////////
        CompositeNode getHealthSequence = new Sequence(bb); // Sequence of actions to take when getting health
        GetHealthDecorator getHealthRoot = new GetHealthDecorator(getHealthSequence, bb);   // defines the condition required to enter the get health sequence
        getHealthSequence.AddChild(new SeekHealthPack(bb, this));   // Find the closest health pack
        getHealthSequence.AddChild(new EnemyMoveTo(bb, this));      // Move to closest health pack
        getHealthSequence.AddChild(new EnemyWaitTillAtLocation(bb, this));  // Wait till enemy reaches destination
        getHealthSequence.AddChild(new EnemyStopMovement(bb, this));    // Stop movement
        #endregion

        #region Patrol Sequence
        /////////////////////////// Patrol Sequence //////////////////////////////
        Sequence patrolSequence = new Sequence(bb); // Sequence of actions to take when patrolling
        // Move to next patrol location
        patrolSequence.AddChild(new GetNextPatrolLocation(bb, this));
        patrolSequence.AddChild(new EnemyMoveTo(bb, this)); // Move to next patrol point
        patrolSequence.AddChild(new EnemyWaitTillAtLocation(bb, this));  // Wait till enemy reaches destination
        patrolSequence.AddChild(new EnemyStopMovement(bb, this));    // Stop movement
        patrolSequence.AddChild(new DelayNode(bb, patrolWaitTime));   // Wait for 'x' seconds with delay node

        #endregion

        // Adding to root selector
        // e.g. rootchild.AddChild(thisRoot);
        rootChild.AddChild(combatRoot);
        rootChild.AddChild(huntRoot);
        rootChild.AddChild(fleeRoot);
        rootChild.AddChild(getHealthRoot);
        rootChild.AddChild(patrolSequence);

        // Execute our BT every 0.1 seconds
        InvokeRepeating("ExecuteBT", 0.1f, 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        //transform.Rotate(new Vector3(0.0f, 90.0f * Time.deltaTime, 0.0f));

        closestEntity = bb.closestEntity;

        if (isMoving)
        {
            if (bb.isFleeing)
            {
                dir = bb.fleeDir;
            }
            else
            {
                dir = moveLocation - transform.position;
            }
            
            transform.position += dir.normalized * moveSpeed * Time.deltaTime;
            
            lookRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * roationDamping);
        }

        if (isRotating)
        {
            Quaternion rotation = Quaternion.LookRotation(rotateLocation);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * roationDamping);
        }
        

        // Debugging
        if (Input.GetKeyDown(KeyCode.B))
        {
            enemyHealthManager.ChangeHealth(-30);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            isMoving = false;
        }
    }

    public void EnemyMoveTo(Vector3 moveLocation)
    {
        isMoving = true;
        this.moveLocation = moveLocation;
    }

    public void EnemyRotateTo(Vector3 rotateLocation)
    {
        isRotating = true;
        this.rotateLocation = rotateLocation;
    }

    public void StopMovement()
    {
        //Debug.Log("Stopped Movement");
        isMoving = false;
        bb.isGettingHealth = false;
    }

    public void StopRotation()
    {
        isRotating = false;
    }

    public void ExecuteBT()
    {
        BTRootNode.Execute();
    }

    //public GameObject FindClosestEnemy()
    //{
    //    List<GameObject> sensedObjects = new List<GameObject>();
    //    GameObject closestEntity = null;
    //    float distance = Mathf.Infinity;
    //    Dictionary<GameObject, MemoryRecord> memoryMap = bb.perception.memoryMap;
    //    Debug.Log("bb.perception.memoryMap = " + memoryMap.ToString());

    //    if (memoryMap.Count > 0)
    //    {
    //        Debug.Log("MEMORY MAP IS GREATER THAN 0");

    //        foreach (KeyValuePair<GameObject, MemoryRecord> memory in bb.perception.memoryMap)
    //        {
    //            sensedObjects.Add(memory.Key);
    //            closestEntity = memory.Key;
    //            Debug.Log("Memory.key = " + memory.Key);
    //        }



    //        foreach (GameObject entity in sensedObjects)
    //        {

    //            Debug.Log("Entity: " + entity + " in sensedObjects");
    //            Vector3 pos = entity.transform.position;
    //            Vector3 diff = pos - this.transform.position;
    //            float curDistance = diff.sqrMagnitude;

    //            if (curDistance < distance)
    //            {
    //                closestEntity = entity;
    //                distance = curDistance;
    //            }
    //        }
    //        Debug.Log("Closest Entity = " + closestEntity);
    //        return closestEntity;
    //    }
    //    else
    //    {
    //        return null;
    //    }

    //}
}

#region Frequently Used Behaviours
//////////////////// Frequently Used Behaviours //////////////////////////
public class EnemyMoveTo : BTNode
{
    private EnemyBB eBB;
    private EnemyController enemyRef;

    public EnemyMoveTo(Blackboard bb, EnemyController enemy) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
    }

    public override BTStatus Execute()
    {
        //Debug.Log("Moving to location");
        enemyRef.EnemyMoveTo(eBB.moveToLocation);
        return BTStatus.SUCCESS;
    }
}

public class EnemyRotateTo : BTNode
{
    private EnemyBB eBB;
    private EnemyController enemyRef;

    public EnemyRotateTo(Blackboard bb, EnemyController enemy) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
    }

    public override BTStatus Execute()
    {
        Debug.Log("Rotating To Location");
        Debug.Log("Rotate Location = " + eBB.rotateToLocation);
        //Vector3 lookPos = entityRef.transform.position - enemyRef.transform.position;
        //lookPos.y = 0;
        //Quaternion rotation = Quaternion.LookRotation(lookPos);

        enemyRef.EnemyRotateTo(eBB.rotateToLocation);
        return BTStatus.SUCCESS;
    }
}

public class EnemyWaitTillAtLocation : BTNode
{
    private EnemyBB eBB;
    private EnemyController enemyRef;

    public EnemyWaitTillAtLocation(Blackboard bb, EnemyController enemy) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
    }

    public override BTStatus Execute()
    {
        BTStatus rv = BTStatus.RUNNING;
        //if ((enemyRef.transform.position - eBB.moveToLocation).magnitude <= 1.0f)
        //{
        //    Debug.Log("Reached target");
        //    rv = BTStatus.SUCCESS;
        //}
        //return rv;

        //Debug.Log("WAITING TO REACH LOCATION");

        if (Vector3.Distance(enemyRef.transform.position, eBB.moveToLocation) <= 1.0f)
        {
            //Debug.Log("Reached target");
            rv = BTStatus.SUCCESS;
        }
        else if (eBB.isFleeing)
        {
            rv = BTStatus.SUCCESS;
        }

        Dictionary<GameObject, MemoryRecord> memoryMap = eBB.perception.memoryMap;
        GameObject[] sensedObjects = new GameObject[memoryMap.Keys.Count];

        if (sensedObjects.Length >= 0)
        {
            MemoryRecord[] sensedRecord = new MemoryRecord[memoryMap.Values.Count];
            memoryMap.Keys.CopyTo(sensedObjects, 0);
            memoryMap.Values.CopyTo(sensedRecord, 0);

            for (int i = 0; i < sensedObjects.Length; i++)
            {
                if (sensedRecord[i].withinFoV == true)
                {
                    rv = BTStatus.SUCCESS;
                }
            }
        }

        
        
        return rv;
    }
}

public class EnemyWaitTillRotated : BTNode
{
    private EnemyBB eBB;
    private EnemyController enemyRef;

    public EnemyWaitTillRotated(Blackboard bb, EnemyController enemy) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
    }

    public override BTStatus Execute()
    {
        BTStatus rv = BTStatus.RUNNING;

        if (enemyRef.transform.rotation == Quaternion.LookRotation(eBB.rotateToLocation))
        {
            rv = BTStatus.SUCCESS;
        }

        return rv;
    }
}

public class EnemyStopMovement : BTNode
{
    private EnemyController enemyRef;

    public EnemyStopMovement(Blackboard bb, EnemyController enemy) : base(bb)
    {
        enemyRef = enemy;
    }

    public override BTStatus Execute()
    {
        enemyRef.StopMovement();
        return BTStatus.SUCCESS;
    }
}

public class EnemyStopRotation : BTNode
{
    private EnemyController enemyRef;

    public EnemyStopRotation(Blackboard bb, EnemyController enemy) : base(bb)
    {
        enemyRef = enemy;
    }

    public override BTStatus Execute()
    {
        enemyRef.StopRotation();
        return BTStatus.SUCCESS;
    }
}

public class RotateTowards : BTNode
{
    private EnemyBB eBB;
    private EnemyController enemyRef;
    private GameObject entityRef;

    private float damping = 10f;

    public RotateTowards(Blackboard bb, EnemyController enemy, GameObject entity) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
        entityRef = entity;
    }

    public override BTStatus Execute()
    {
        Debug.Log("Waiting");

        Debug.Log("Entity Ref = " + entityRef);

        Vector3 lookPos = entityRef.transform.position - enemyRef.transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        enemyRef.transform.rotation = Quaternion.Slerp(enemyRef.transform.rotation, rotation, Time.deltaTime * damping);
        //enemyRef.transform.rotation = Quaternion.Lerp(enemyRef.transform.rotation, rotation, Time.deltaTime * damping);
        return BTStatus.SUCCESS;
    }
}
#endregion

#region Combat Sequence
/////////////////////////// Combat Sequence //////////////////////////////

/// <summary>
/// Combat sequence entry requirements
/// </summary>
public class CombatDecorator : ConditionalDecorator
{
    EnemyBB eBB;

    public CombatDecorator(BTNode wrappedNode, Blackboard bb) : base(wrappedNode, bb)
    {
        eBB = (EnemyBB)bb;
    }

    public override bool CheckStatus()
    {
        // RETURN WHETHER THERE IS AN ENEMY IN SIGHT OR NOT (WITHIN FOV)
        if (eBB.fieldOfView.visibleTargets.Count > 0 && eBB.isHealthy)
        {
            //Debug.Log("Combat Decorator = True");
            return true;
        }
        else
        {
            //Debug.Log("Combat Decorator = False");
            return false;
        }
    }
}

public class GunCombatDecorator : ConditionalDecorator
{
    EnemyBB eBB;

    public GunCombatDecorator(BTNode wrappedNode, Blackboard bb) : base(wrappedNode, bb)
    {
        eBB = (EnemyBB)bb;
    }

    public override bool CheckStatus()
    {
        // RETURN IF IS IN FoV AND NOT MELEE RANGE

        if (eBB.fieldOfView.visibleTargets.Count > 0 && !eBB.inMeleeRange)
        {
            Debug.Log("Gun Combat Decorator = True");
            return true;
        }
        else
        {
            Debug.Log("Gun Combat Decorator = False");
            return false;
        }

        
    }
}

public class GetEnemyRotationLocation : BTNode
{
    private EnemyBB eBB;

    public GetEnemyRotationLocation(Blackboard bb) : base(bb)
    {
        eBB = (EnemyBB)bb;
    }

    public override BTStatus Execute()
    {
        eBB.rotateToLocation = eBB.closestEntity.transform.position;

        return BTStatus.SUCCESS;
    }
}

public class FireWeapon : BTNode
{
    private EnemyBB eBB;
    private EnemyController enemyRef;

    //private bool canShoot = true;
    //private MonoBehaviour monoBehaviour = new MonoBehaviour();
    //private int currentShots = 0;
    //private int maxShots = 3;

    private float timer = 0f;
    private float waitingTime = 0.1f;
    private bool hasFired = false;

    public FireWeapon(Blackboard bb, EnemyController enemy) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
        //monoBehaviour = new MonoBehaviour();
        //currentShots = 0;
        //maxShots = 3;
    }

    public override BTStatus Execute()
    {
        BTStatus rv = BTStatus.RUNNING;

        Debug.Log("Firing");

        // @TODO Keep firing if enemy is within sight, if not, return success to move on with tree

        timer += 1f * Time.deltaTime;
        //Debug.Log("MaxTime = " + waitingTime);
        //Debug.Log("Timer = " + timer);

        if (!hasFired)
        {
            Fire();
            hasFired = true;
        }

        if (timer >= waitingTime)
        {
            Fire();
        }

        //eBB.fieldOfView.visibleTargets[0].

        //StartCoroutine(FireShot());

        //if (currentShots >= maxShots)
        //{
        //    Debug.Log("Returning Success!");
        //    rv = BTStatus.SUCCESS;
        //}
        //else
        //{
        //    timer += Time.deltaTime;
        //    Debug.Log("MaxTime = " + waitingTime);
        //    Debug.Log("Timer = " + timer);

        //    if (timer >= waitingTime)
        //    {
        //        Fire();
        //    }
        //}

        return rv;
    }

    //IEnumerator FireShot()
    //{
    //    //canShoot = true;
    //    Fire();
    //    currentShots += 1;
    //    yield return new WaitForSeconds(1);
    //    //canShoot = false;
    //}

    private void Fire()
    {
        //GameObject instancedObject = MonoBehaviour.Instantiate(enemyRef.projectile, enemyRef.transform.position, Quaternion.identity) as GameObject;
        //MonoBehaviour.Instantiate(enemyRef.projectile, enemyRef.transform.position, Quaternion.identity);
        MonoBehaviour.Instantiate(enemyRef.projectile, enemyRef.transform, false);
        //currentShots += 1;
        timer = 0f;
        Debug.Log("BANG");
    }
}

public class MeleeCombatDecorator : ConditionalDecorator
{
    EnemyBB eBB;

    public MeleeCombatDecorator(BTNode wrappedNode, Blackboard bb) : base(wrappedNode, bb)
    {
        eBB = (EnemyBB)bb;
    }

    public override bool CheckStatus()
    {
        // Placeholder
        if (eBB.inMeleeRange && eBB.isHealthy)
        {
            Debug.Log("In Melee Range");
            return true;
        }

        return false;

        // RETURN IF IS HEALTHY AND ENEMY WITHIN MELEE RANGE
    }
}

public class MeleeAttack : BTNode
{
    private EnemyBB eBB;
    private EnemyController enemyRef;

    private HealthManager hm;
    private float timer = 0f;
    private float waitingTime = 0.03f;
    private bool hasAttacked = false;

    public MeleeAttack(Blackboard bb, EnemyController enemy) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
    }

    public override BTStatus Execute()
    {
        hm = eBB.meleeAttackTarget.GetComponent<HealthManager>();
        Debug.Log("Has Attacked = " + hasAttacked);
        timer += 1f * Time.deltaTime;

        if (!hasAttacked)
        {
            hm.ChangeHealth(-20);
            Debug.Log("Melee Attack");
            hasAttacked = true;
        }
        else if (timer >= waitingTime)
        {
            hasAttacked = false;
            timer = 0f;
        }

        return BTStatus.SUCCESS;
    }

    public void Attack()
    {
        hm.ChangeHealth(-20);
        timer = 0;
    }
}

#endregion

#region Hunt Sequence
//////////////////////////// Hunt Sequence ///////////////////////////////
public class HuntDecorator : ConditionalDecorator
{
    EnemyBB eBB;

    public HuntDecorator(BTNode wrappedNode, Blackboard bb) : base(wrappedNode, bb)
    {
        eBB = (EnemyBB)bb;
    }

    public override bool CheckStatus()
    {
        // Placeholder


        if (eBB.perception.memoryMap.Count > 0 && eBB.isHealthy)
        {
            Debug.Log("Hunt Decorator True");
            return true;
        }

        return false;
        // RETURN WHETHER THERE IS AN ENEMY IN MEMORY, BUT NOT IN SIGHT CURRENTLY
        // ALSO HAS TO BE HEALTHY
    }
}

public class CalculateLastKnownLocation : BTNode
{
    private EnemyBB eBB;
    private EnemyController enemyRef;

    public CalculateLastKnownLocation(Blackboard bb, EnemyController enemy) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
    }

    public override BTStatus Execute()
    {
        if (eBB.lastKnownEntity != null)
        {
            eBB.moveToLocation = eBB.lastKnownEntity.transform.position;
            return BTStatus.SUCCESS;
        }

        return BTStatus.FAILURE;
    }
}
#endregion

#region Flee Sequence
public class FleeDecorator : ConditionalDecorator
{
    EnemyBB eBB;

    public FleeDecorator(BTNode wrappedNode, Blackboard bb) : base(wrappedNode, bb)
    {
        eBB = (EnemyBB)bb;
    }

    public override bool CheckStatus()
    {
        //Debug.Log("Enemy is not healthy: " + !eBB.isHealthy);

        // RETURN IF IS NOT HEALTHY && ENEMY IN MEMORY
        if (eBB.perception.memoryMap.Count > 0 && !eBB.isHealthy)
        {
            return true;
        }
        return false;
    }
}

public class CalculateFleeLocation : BTNode
{
    private EnemyBB eBB;
    private EnemyController enemyRef;

    private GameObject player;
    private GameObject companion;

    private Vector3 avoidLocation;
    private float safeDistance = 40f;

    //private float timer = 0f;
    //private float waitingTime = 0.5f;

    public CalculateFleeLocation(Blackboard bb, EnemyController enemy) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
    }

    public override BTStatus Execute()
    {
        BTStatus rv = BTStatus.RUNNING;

        Debug.Log("Fleeing");

        eBB.isFleeing = true;
        enemyRef.isMoving = true;

        Vector3 runDir = enemyRef.transform.position - (eBB.playerLocation + eBB.companionLocation);
        eBB.fleeDir = runDir;

        avoidLocation = eBB.playerLocation + eBB.companionLocation;

        Debug.Log(Vector3.Distance(enemyRef.transform.position, avoidLocation));

        if (Vector3.Distance(enemyRef.transform.position, avoidLocation) >= safeDistance)
        {
            eBB.isFleeing = false;
            enemyRef.isMoving = false;
            rv = BTStatus.SUCCESS;
            Debug.Log("No longer fleeing");
        }

        return rv;








        //avoidLocation = eBB.playerLocation + eBB.companionLocation;
        //avoidLocation = -avoidLocation;
        //avoidLocation.y = enemyRef.transform.position.y;
        //eBB.moveToLocation = avoidLocation;













        //BTStatus rv = BTStatus.RUNNING;

        //avoidLocation = eBB.playerLocation + eBB.companionLocation;
        //Vector3 runDir = enemyRef.transform.position - (eBB.playerLocation + eBB.companionLocation);
        //runDir.y = enemyRef.transform.position.y;
        //runDir = runDir.normalized;


        //Debug.Log("RunDir = " + runDir);
        //Debug.Log(Vector3.Distance(enemyRef.transform.position, avoidLocation));

        ////enemyRef.transform.rotation = Quaternion.Slerp(enemyRef.transform.rotation, fleeRotation, enemyRef.roationDamping * Time.deltaTime);


        //if (Vector3.Distance(enemyRef.transform.position, avoidLocation) <= safeDistance)
        //{
        //    Debug.Log("Running Away");
        //    Quaternion fleeRotation = Quaternion.LookRotation(runDir);
        //    enemyRef.transform.rotation = fleeRotation;
        //    enemyRef.transform.position += runDir * enemyRef.moveSpeed * Time.deltaTime;
        //    rv = BTStatus.RUNNING;
        //}
        //else
        //{
        //    Debug.Log("Success");
        //    rv = BTStatus.SUCCESS;
        //}












        //timer += 1 * Time.deltaTime;

        //if (timer <= waitingTime)
        //{
        //    enemyRef.dir = runDir;
        //    enemyRef.EnemyMoveTo(enemyRef.transform.position);
        //    rv = BTStatus.RUNNING;

        //    Debug.Log("RunDir == dir");
        //}
        //else
        //{
        //    timer = 0;
        //    enemyRef.StopMovement();
        //    rv = BTStatus.SUCCESS;
        //    Debug.Log("Returning success now");
        //}



        //runTo.y = enemyRef.transform.position.y;
        //eBB.moveToLocation = runTo;

        //return rv;
    }
}
#endregion

#region Get Health Sequence
////////////////////////// Get Health Sequence ///////////////////////////
public class GetHealthDecorator : ConditionalDecorator
{
    EnemyBB eBB;

    public GetHealthDecorator(BTNode wrappedNode, Blackboard bb) : base(wrappedNode, bb)
    {
        eBB = (EnemyBB)bb;
    }

    public override bool CheckStatus()
    {
        if (!eBB.isHealthy || eBB.isGettingHealth)
        {
            return true;
        }

        return false;
        // RETURN WHETHER AI IS NOT HEALTHY 
    }
}

public class SeekHealthPack : BTNode
{
    private EnemyBB eBB;
    private EnemyController enemyRef;

    private List<HealthPack> healthPacks;
    private HealthPack closestPack;

    public SeekHealthPack(Blackboard bb, EnemyController enemy) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
    }

    public override BTStatus Execute()
    {
        Debug.Log("Finding Health Packs");
        healthPacks = new List<HealthPack>();

        foreach (HealthPack hp in GameObject.FindObjectsOfType<HealthPack>())
        {
            healthPacks.Add(hp);
        }

        eBB.isGettingHealth = true;

        closestPack = GetClosestHealthPack(healthPacks, enemyRef);
        if (closestPack == null)
        {
            return BTStatus.FAILURE;
        }

        eBB.moveToLocation = closestPack.transform.position;

        return BTStatus.SUCCESS;
    }

    private HealthPack GetClosestHealthPack(List<HealthPack> healthPacks, EnemyController enemy)
    {
        HealthPack closestPack = null;
        float distance = Mathf.Infinity;
        Vector3 pos = enemy.transform.position;

        foreach (HealthPack hp in healthPacks)
        {
            Vector3 diff = hp.transform.position - pos;
            float curDistance = diff.sqrMagnitude;

            if (curDistance < distance)
            {
                closestPack = hp;
                distance = curDistance;
            }
        }

        return closestPack;
    }
}
#endregion

#region Patrol Sequence
//////////////////////////// Patrol Sequence /////////////////////////////
public class GetNextPatrolLocation : BTNode
{
    private EnemyBB eBB;
    private EnemyController enemyRef;

    public GetNextPatrolLocation(Blackboard bb, EnemyController enemy) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
    }

    public override BTStatus Execute()
    {
        eBB.RandomisePatrolPoint();
        Transform nextPos = eBB.patrolLocations[eBB.locationNumber];
        eBB.moveToLocation = nextPos.transform.position;
        eBB.moveToLocation.y = enemyRef.transform.position.y;
        return BTStatus.SUCCESS;
    }
}

public class LookAround : BTNode
{
    private EnemyBB eBB;
    private EnemyController enemyRef;

    public LookAround(Blackboard bb, EnemyController enemy) : base(bb)
    {
        eBB = (EnemyBB)bb;
        enemyRef = enemy;
    }

    public override BTStatus Execute()
    {
        //Quaternion rotation = Quaternion.LookRotation(eBB.moveToLocation - new Vector3(eBB.moveToLocation.x, eBB.moveToLocation.y, eBB.moveToLocation.z - 900f), Vector3.up);
        //enemyRef.transform.rotation = Quaternion.Slerp(enemyRef.transform.rotation, rotation, Time.deltaTime * 4f);

        enemyRef.transform.Rotate(new Vector3(0.0f, 90.0f * Time.deltaTime, 0.0f));

        return BTStatus.SUCCESS;
    }
}
#endregion
