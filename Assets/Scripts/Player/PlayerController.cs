using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HealthManager))]
public class PlayerController : MonoBehaviour
{
    #region Player Variables

    private GameManager gameManager;

    public float moveSpeed = 5.0f;
    [SerializeField]
    private float rotateSpeed = 200f;

    public GameObject projectilePrefab;
    private float timeBtwShots;
    public float startTimeBtwShots = 0.5f;

    [HideInInspector]
    public Vector3 moveInput;
    private Vector3 velocity;

    private HealthManager playerHealthManager;

    #endregion

    private void Awake()
    {
        playerHealthManager = GetComponent<HealthManager>();
        timeBtwShots = 0f;
    }

    private void Update()
    {
        moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.B))
        {
            playerHealthManager.ChangeHealth(-30);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            //Debug.Log("Time between shots = " + timeBtwShots);
            
            if (timeBtwShots <= 0)
            {
                Fire();
                timeBtwShots = startTimeBtwShots;
            }
            else
            {
                timeBtwShots -= Time.deltaTime;
            }
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            timeBtwShots = 0f;
        }

        ////////////////////////////Player Movement///////////////////////////////
        
        // Clamps players diagonal movement speed
        if (moveInput.magnitude > 1)
        {
            moveInput = moveInput.normalized;
        }

        // Sprint Movement
        if (Input.GetKey(KeyCode.LeftShift))
        {
            velocity = moveInput * (moveSpeed * 1.5f);
        }
        else
        {
            velocity = moveInput * moveSpeed;
        }

        transform.position += velocity * Time.deltaTime;

        ////////////////////////////Player Rotation///////////////////////////////
        RotateTowardMovement(moveInput);


        ////////////////////////////Death///////////////////////////////
        if (playerHealthManager.GetCurrentHealth() <= 0)
        {
            gameManager = FindObjectOfType<GameManager>();
            gameManager.EndGame();
        }
    }

    private void RotateTowardMovement(Vector3 moveInput)
    {
        if (moveInput.magnitude == 0)
        {
            return;
        }
        
        Quaternion rotation = Quaternion.LookRotation(moveInput);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, rotateSpeed * Time.deltaTime);
        //transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotateSpeed);
    }

    public Vector3 GetVelocity()
    {
        return velocity;
    }

    public void Fire()
    {
        Instantiate(projectilePrefab, transform.position, transform.localRotation);
    }











    //////////////////////////////////////////////////////////////////////////
    //////////////////////////Second Implementation///////////////////////////
    //////////////////////////////////////////////////////////////////////////




    //#region Player Variables

    //private Rigidbody rb;

    //public float moveSpeed;

    //private Vector3 moveInput;
    //private Vector3 velocity;

    //#endregion

    //private void Start()
    //{
    //    rb = GetComponent<Rigidbody>();
    //}

    //private void Update()
    //{
    //    moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
    //    velocity = moveInput * moveSpeed;

    //    rb.velocity = velocity;
    //}






    //////////////////////////////////////////////////////////////////////////
    /////////////////////////Original Implementation//////////////////////////
    //////////////////////////////////////////////////////////////////////////

    //private Rigidbody rb;

    //#region Player Variables

    ////[SerializeField]
    ////int health = 100;
    //[SerializeField]
    //float moveSpeed = 3.0f;

    //private float xMove = 0.0f;
    //private float zMove = 0.0f;
    //private Vector3 moveBy = Vector3.zero;


    //#endregion

    //private void Awake()
    //{
    //    Debug.Log("Player awakes...");
    //    rb = GetComponent<Rigidbody>();
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    xMove = Input.GetAxisRaw("Horizontal");
    //    zMove = Input.GetAxisRaw("Vertical");

    //    moveBy = transform.right * xMove + transform.forward * zMove;

    //    if (Input.GetKey(KeyCode.LeftShift))
    //    {
    //        rb.MovePosition(transform.position + moveBy.normalized * (moveSpeed * 2) * Time.deltaTime);
    //    } 
    //    else
    //    {
    //        rb.MovePosition(transform.position + moveBy.normalized * moveSpeed * Time.deltaTime);
    //    }


    //}

    //public Vector3 GetCurrentLocation()
    //{
    //    Vector3 loc = Vector3.zero;

    //    loc.x = transform.position.x;
    //    loc.y = transform.position.y;
    //    loc.z = transform.position.z;

    //    return loc;
    //}
}
