using System;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class RelativeMovement : NetworkBehaviour
{
    public Transform Target { set => target = value; }
    public bool Jerked => jerked;

    private float prevJerkDistance = 600.0f;
    [SerializeField] private float jerkDistance = 600.0f;
    [SyncVar] private float newJerkDistance = 600.0f;
    private float rotationSpeed = 15.0f;
    private float moveSpeed = 6.0f;
    private float gravity = -9.8f;
    private float minFall = -1.5f;
    private float terminalVelocity = -10.0f;
    private float verticalSpeed;
    private float verticalSpeedFactor = 5;
    private Transform target;
    private CharacterController characterController;
    private Animator animator;
    private Transform previousTransform;
    [SyncVar] private bool jerked;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        verticalSpeed = minFall;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isServer)
        {
            if (prevJerkDistance != jerkDistance)
            {
                prevJerkDistance = jerkDistance;
                newJerkDistance = jerkDistance;
            }
        }
        
        if (!isLocalPlayer)
            return;
        jerkDistance = newJerkDistance;
        
        if (NetworkClient.ready)
            CmdResetJerked();

        Vector3 movement = Vector3.zero;

        if (Input.GetMouseButtonDown(0))
            movement = Jerk();
        else
            movement = Movement();

        if (characterController.isGrounded)
        {
            verticalSpeed = minFall;
        }
        else
        {
            verticalSpeed += gravity * verticalSpeedFactor * Time.deltaTime;
            if (verticalSpeed < terminalVelocity)
                verticalSpeed = terminalVelocity;
        }

        movement.y = verticalSpeed;
        movement *= Time.deltaTime;
        characterController.Move(movement);

        var speed = (float)Math.Sqrt(Math.Pow(movement.x, 2) + Math.Pow(movement.z, 2));
        animator.SetFloat("Speed", speed);
    }

    private Vector3 Movement()
    {
        Vector3 movement = Vector3.zero;

        var xInput = Input.GetAxis("Horizontal");
        var zInput = Input.GetAxis("Vertical");
        if (xInput != 0 || zInput != 0)
        {
            movement.x = xInput * moveSpeed;
            movement.z = zInput * moveSpeed;
            movement = Vector3.ClampMagnitude(movement, moveSpeed);

            var tmp = target.rotation;
            target.eulerAngles = new Vector3(0, target.eulerAngles.y, 0);
            movement = target.TransformDirection(movement);
            target.rotation = tmp;

            var direction = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Lerp(transform.rotation, direction, rotationSpeed * Time.deltaTime);
        }
        
        return movement;
    }

    private Vector3 Jerk()
    {
        CmdSetJerked();
        
        Vector3 movement = Vector3.zero;
        
        var cameraTransform = target.transform;
        var cameraForward = cameraTransform.forward;//
        var playerForward = transform.forward;//
        var codirection = playerForward.z / cameraForward.z;
        
        if (codirection >= 0)
        {
            movement.z = jerkDistance;
            movement = Vector3.ClampMagnitude(movement, jerkDistance);            
        }
        else
        {
            movement.z = -jerkDistance;
            movement = Vector3.ClampMagnitude(movement, -jerkDistance);
        }
        
        var tmp = target.rotation;
        target.eulerAngles = new Vector3(0, target.eulerAngles.y, 0);
        movement = target.TransformDirection(movement);
        target.rotation = tmp;
        
        var direction = Quaternion.LookRotation(movement);
        transform.rotation = Quaternion.Lerp(transform.rotation, direction, rotationSpeed * Time.deltaTime);

        return movement;
    }

    [Command]
    private void CmdSetJerked()
    {
        jerked = true;
    }
    
    [Command]
    private void CmdResetJerked()
    {
        jerked = false;
    }
}