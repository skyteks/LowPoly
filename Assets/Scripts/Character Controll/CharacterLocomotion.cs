using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterLocomotion : MonoBehaviour
{
    [System.Serializable]
    public class MovementSpeed
    {
        [Range(1f, 20f)]
        public float movementSmooth = 6f;
        [Range(0f, 1f)]
        public float animationSmooth = 0.2f;
        [Tooltip("Rotation speed of the character")]
        public float rotationSpeed = 16f;
        [Tooltip("Character will limit the movement to walk instead of running")]
        public bool walkByDefault = false;
        [Tooltip("Rotate with the Camera forward when standing idle")]
        public bool rotateWithCamera = false;
        [Tooltip("Speed to Walk using rigidbody or extra speed if you're using RootMotion")]
        public float walkSpeed = 2f;
        [Tooltip("Speed to Run using rigidbody or extra speed if you're using RootMotion")]
        public float runningSpeed = 4f;
        [Tooltip("Speed to Sprint using rigidbody or extra speed if you're using RootMotion")]
        public float sprintSpeed = 6f;
    }

    public enum LocomotionType
    {
        FreeWithStrafe,
        OnlyStrafe,
        OnlyFree,
    }

    [Header("MOVEMENT")]

    [Tooltip("Turn off if you have 'in place' animations and use this values above to move the character, or use with root motion as extra speed")]
    public bool useRootMotion = false;
    [Tooltip("Use this to rotate the character using the World axis, or false to use the camera axis - CHECK for Isometric Camera")]
    public bool rotateByWorld = false;
    [Tooltip("Check This to use sprint on press button to your Character run until the stamina finish or movement stops\nIf uncheck your Character will sprint as long as the SprintInput is pressed or the stamina finishes")]
    public bool useContinuousSprint = true;
    [Tooltip("Check this to sprint always in free movement")]
    public bool sprintOnlyFree = true;

    public LocomotionType locomotionType = LocomotionType.FreeWithStrafe;

    public MovementSpeed freeSpeed, strafeSpeed;

    [Header("AIRBORNE")]

    [Tooltip("Use the currently Rigidbody Velocity to influence on the Jump Distance")]
    public bool jumpWithRigidbodyForce = false;
    [Tooltip("Rotate or not while airborne")]
    public bool jumpAndRotate = true;
    [Tooltip("How much time the character will be jumping")]
    public float jumpTimer = 0.3f;
    [Tooltip("Add Extra jump height, if you want to jump only with Root Motion leave the value with 0.")]
    public float jumpHeight = 4f;

    [Tooltip("Speed that the character will move while airborne")]
    public float airSpeed = 5f;
    [Tooltip("Smoothness of the direction while airborne")]
    public float airSmooth = 6f;
    [Tooltip("Apply extra gravity when the character is not grounded")]
    public float extraGravity = -10f;
    [HideInInspector]
    public float limitFallVelocity = -15f;

    [Header("GROUND")]
    [Tooltip("Layers that the character can walk on")]
    public LayerMask groundLayer = 1 << 0;
    [Tooltip("Distance to became not grounded")]
    public float groundMinDistance = 0.25f;
    public float groundMaxDistance = 0.5f;
    [Tooltip("Max angle to walk")]
    [Range(30, 80)]
    public float slopeLimit = 75f;

    // access the Rigidbody component
    private static PhysicMaterial frictionPhysics, maxFrictionPhysics, slippyPhysics;
    private Rigidbody rigid;
    private CapsuleCollider capsuleColl;
    private CharacterAnimator charAnim;

    // movement bools
    public Vector3 input { get; set; }                 // generate raw input for the controller              
    public bool isJumping { get; private set; }
    public bool isGrounded { get; private set; }
    public bool isSprinting { get; private set; }
    public bool stopMove { get; private set; }
    public bool isStrafing { get; private set; }                           // privately used to set the strafe movement                
    public float groundDistance { get; private set; }                      // used to know the distance from the ground
    public float horizontalSpeed { get; set; }                     // set the horizontalSpeed based on the horizontalInput       
    public float verticalSpeed { get; set; }                       // set the verticalSpeed based on the verticalInput
    public float inputMagnitude { get; set; }                      // sets the inputMagnitude to update the animations in the animator controller
    public Vector3 moveDirection { get; private set; }                     // used to know the direction you're moving 

    private float moveSpeed;                           // set the current moveSpeed for the MoveCharacter method
    private float verticalVelocity;                    // set the vertical velocity of the rigidbody       
    private float heightReached;                       // max height that character reached in air;
    private float jumpCounter;                         // used to count the routine to reset the jump
    private RaycastHit groundHit;                      // raycast to hit the ground 
    private bool lockMovement;                         // lock the movement of the controller (not the animation)
    private bool lockRotation;                         // lock the rotation of the controller (not the animation)        
    private Transform rotateTarget;                    // used as a generic reference for the camera.transform
    private Vector3 inputSmooth;                       // generate smooth input based on the inputSmooth value       


    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        capsuleColl = GetComponent<CapsuleCollider>();
        charAnim = GetComponent<CharacterAnimator>();
    }

    public void Init()
    {
        if (frictionPhysics == null)
        {
            // slides the character through walls and edges
            frictionPhysics = new PhysicMaterial("frictionPhysics");
            frictionPhysics.staticFriction = .25f;
            frictionPhysics.dynamicFriction = .25f;
            frictionPhysics.frictionCombine = PhysicMaterialCombine.Multiply;
        }

        if (maxFrictionPhysics == null)
        {
            // prevents the collider from slipping on ramps
            maxFrictionPhysics = new PhysicMaterial("maxFrictionPhysics");
            maxFrictionPhysics.staticFriction = 1f;
            maxFrictionPhysics.dynamicFriction = 1f;
            maxFrictionPhysics.frictionCombine = PhysicMaterialCombine.Maximum;
        }

        if (slippyPhysics == null)
        {
            // air physics 
            slippyPhysics = new PhysicMaterial("slippyPhysics");
            slippyPhysics.staticFriction = 0f;
            slippyPhysics.dynamicFriction = 0f;
            slippyPhysics.frictionCombine = PhysicMaterialCombine.Minimum;
        }

        isGrounded = true;
    }

    public void UpdateMotor()
    {
        CheckGround();
        CheckSlopeLimit();
        ControlJumpBehaviour();
        AirControl();
    }

    public void SetControllerMoveSpeed(MovementSpeed speed)
    {
        if (speed.walkByDefault)
        {
            moveSpeed = Mathf.Lerp(moveSpeed, isSprinting ? speed.runningSpeed : speed.walkSpeed, speed.movementSmooth * Time.deltaTime);
        }
        else
        {
            moveSpeed = Mathf.Lerp(moveSpeed, isSprinting ? speed.sprintSpeed : speed.runningSpeed, speed.movementSmooth * Time.deltaTime);
        }
    }

    public void MoveCharacter(Vector3 direction)
    {
        // calculate input smooth
        inputSmooth = Vector3.Lerp(inputSmooth, input, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);

        if (!isGrounded || isJumping)
        {
            return;
        }

        direction.y = 0f;
        direction.x = Mathf.Clamp(direction.x, -1f, 1f);
        direction.z = Mathf.Clamp(direction.z, -1f, 1f);
        // limit the input
        if (direction.magnitude > 1f)
        {
            direction.Normalize();
        }

        Vector3 targetPosition = (useRootMotion ? charAnim.animator.rootPosition : rigid.position) + direction * (stopMove ? 0f : moveSpeed) * Time.deltaTime;
        Vector3 targetVelocity = (targetPosition - transform.position) / Time.deltaTime;

        bool useVerticalVelocity = true;
        if (useVerticalVelocity)
        {
            targetVelocity.y = rigid.velocity.y;
        }
        rigid.velocity = targetVelocity;
    }

    public void CheckSlopeLimit()
    {
        if (input.sqrMagnitude < 0.1f)
        {
            return;
        }

        RaycastHit hitinfo;

        if (Physics.Linecast(transform.position + Vector3.up * (capsuleColl.height * 0.5f), transform.position + moveDirection.normalized * (capsuleColl.radius + 0.2f), out hitinfo, groundLayer))
        {
            float hitAngle = Vector3.Angle(Vector3.up, hitinfo.normal);

            var targetPoint = hitinfo.point + moveDirection.normalized * capsuleColl.radius;
            if ((hitAngle > slopeLimit) && Physics.Linecast(transform.position + Vector3.up * (capsuleColl.height * 0.5f), targetPoint, out hitinfo, groundLayer))
            {
                hitAngle = Vector3.Angle(Vector3.up, hitinfo.normal);

                if (hitAngle > slopeLimit && hitAngle < 85f)
                {
                    stopMove = true;
                    return;
                }
            }
        }
        stopMove = false;
    }

    public void RotateToPosition(Vector3 position)
    {
        Vector3 desiredDirection = position - transform.position;
        RotateToDirection(desiredDirection.normalized);
    }

    public void RotateToDirection(Vector3 direction)
    {
        RotateToDirection(direction, isStrafing ? strafeSpeed.rotationSpeed : freeSpeed.rotationSpeed);
    }

    private void RotateToDirection(Vector3 direction, float rotationSpeed)
    {
        if (!jumpAndRotate && !isGrounded)
        {
            return;
        }
        direction.y = 0f;
        Vector3 desiredForward = Vector3.RotateTowards(transform.forward, direction.normalized, rotationSpeed * Time.deltaTime, 0.1f);
        Quaternion newRotation = Quaternion.LookRotation(desiredForward);
        transform.rotation = newRotation;
    }

    private void ControlJumpBehaviour()
    {
        if (!isJumping)
        {
            return;
        }

        jumpCounter -= Time.deltaTime;
        if (jumpCounter <= 0)
        {
            jumpCounter = 0;
            isJumping = false;
        }
        // apply extra force to the jump height
        Vector3 velocity = rigid.velocity;
        velocity.y = jumpHeight;
        rigid.velocity = velocity;
    }

    public void AirControl()
    {
        if (isGrounded && !isJumping)
        {
            return;
        }
        if (transform.position.y > heightReached)
        {
            heightReached = transform.position.y;
        }
        inputSmooth = Vector3.Lerp(inputSmooth, input, airSmooth * Time.deltaTime);

        if (jumpWithRigidbodyForce && !isGrounded)
        {
            rigid.AddForce(moveDirection * airSpeed * Time.deltaTime, ForceMode.VelocityChange);
            return;
        }

        moveDirection = new Vector3(Mathf.Clamp(moveDirection.x, -1f, 1f), 0f, Mathf.Clamp(moveDirection.z, -1f, 1f));

        Vector3 targetPosition = rigid.position + moveDirection * airSpeed * Time.deltaTime;
        Vector3 targetVelocity = (targetPosition - transform.position) / Time.deltaTime;

        targetVelocity.y = rigid.velocity.y;
        rigid.velocity = Vector3.Lerp(rigid.velocity, targetVelocity, airSmooth * Time.deltaTime);
    }

    private bool CheckJumpFwdCondition()
    {
        Vector3 p1 = transform.position + capsuleColl.center + Vector3.down * capsuleColl.height * 0.5f;
        Vector3 p2 = p1 + Vector3.up * capsuleColl.height;
        return Physics.CapsuleCast(p1, p2, capsuleColl.radius * 0.5f, transform.forward, 0.6f, groundLayer);
    }

    private void CheckGround()
    {
        CheckGroundDistance();
        ControlMaterialPhysics();

        if (groundDistance <= groundMinDistance)
        {
            isGrounded = true;
            if (!isJumping && groundDistance > 0.05f)
            {
                rigid.AddForce(transform.up * (extraGravity * 2f * Time.deltaTime), ForceMode.VelocityChange);
            }

            heightReached = transform.position.y;
        }
        else
        {
            if (groundDistance >= groundMaxDistance)
            {
                // set IsGrounded to false 
                isGrounded = false;
                // check vertical velocity
                verticalVelocity = rigid.velocity.y;
                // apply extra gravity when falling
                if (!isJumping)
                {
                    rigid.AddForce(transform.up * extraGravity * Time.deltaTime, ForceMode.VelocityChange);
                }
            }
            else if (!isJumping)
            {
                rigid.AddForce(transform.up * (extraGravity * 2f * Time.deltaTime), ForceMode.VelocityChange);
            }
        }
    }

    private void ControlMaterialPhysics()
    {
        // change the physics material to very slip when not grounded
        capsuleColl.material = (isGrounded && GroundAngle() <= slopeLimit + 1f) ? frictionPhysics : slippyPhysics;

        if (isGrounded)
        {
            capsuleColl.material = input == Vector3.zero ? maxFrictionPhysics : frictionPhysics;
        }
        else
        {
            capsuleColl.material = slippyPhysics;
        }
    }

    private void CheckGroundDistance()
    {
        if (capsuleColl != null)
        {
            // radius of the SphereCast
            float radius = capsuleColl.radius * 0.9f;
            float dist = 10f;
            // ray for RayCast
            Ray ray2 = new Ray(transform.position + (Vector3.up * capsuleColl.height * 0.5f), Vector3.down);
            // raycast for check the ground distance
            if (Physics.Raycast(ray2, out groundHit, (capsuleColl.height / 2f) + dist, groundLayer) && !groundHit.collider.isTrigger)
            {
                dist = transform.position.y - groundHit.point.y;
            }
            // sphere cast around the base of the capsule to check the ground distance
            if (dist >= groundMinDistance)
            {
                Vector3 pos = transform.position + Vector3.up * capsuleColl.radius;
                Ray ray = new Ray(pos, -Vector3.up);
                if (Physics.SphereCast(ray, radius, out groundHit, capsuleColl.radius + groundMaxDistance, groundLayer) && !groundHit.collider.isTrigger)
                {
                    Physics.Linecast(groundHit.point + (Vector3.up * 0.1f), groundHit.point + Vector3.down * 0.15f, out groundHit, groundLayer);
                    float newDist = transform.position.y - groundHit.point.y;
                    if (dist > newDist) dist = newDist;
                }
            }
            groundDistance = (float)System.Math.Round(dist, 2);
        }
    }

    public float GroundAngle()
    {
        float groundAngle = Vector3.Angle(groundHit.normal, Vector3.up);
        return groundAngle;
    }

    public float GroundAngleFromDirection()
    {
        Vector3 dir = isStrafing && input.magnitude > 0f ? (transform.right * input.x + transform.forward * input.z).normalized : transform.forward;
        float movementAngle = Vector3.Angle(dir, groundHit.normal) - 90f;
        return movementAngle;
    }

    public void ControlAnimatorRootMotion()
    {
        if (enabled)
        {
            return;
        }

        if (inputSmooth == Vector3.zero)
        {
            transform.SetPositionAndRotation(charAnim.animator.rootPosition, charAnim.animator.rootRotation);
        }

        if (useRootMotion)
        {
            MoveCharacter(moveDirection);
        }
    }

    public void ControlLocomotionType()
    {
        if (lockMovement)
        {
            return;
        }

        if (locomotionType == LocomotionType.FreeWithStrafe && !isStrafing || locomotionType == LocomotionType.OnlyFree)
        {
            SetControllerMoveSpeed(freeSpeed);
            charAnim.SetAnimatorMoveSpeed(freeSpeed);
        }
        else if (locomotionType == LocomotionType.OnlyStrafe || locomotionType == LocomotionType.FreeWithStrafe && isStrafing)
        {
            isStrafing = true;
            SetControllerMoveSpeed(strafeSpeed);
            charAnim.SetAnimatorMoveSpeed(strafeSpeed);
        }

        if (!useRootMotion)
        {
            MoveCharacter(moveDirection);
        }
    }

    public void ControlRotationType()
    {
        if (lockRotation)
        {
            return;
        }

        bool validInput = input != Vector3.zero || (isStrafing ? strafeSpeed.rotateWithCamera : freeSpeed.rotateWithCamera);

        if (validInput)
        {
            // calculate input smooth
            inputSmooth = Vector3.Lerp(inputSmooth, input, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);

            Vector3 dir = (isStrafing && (!isSprinting || sprintOnlyFree == false) || (freeSpeed.rotateWithCamera && input == Vector3.zero)) && rotateTarget ? rotateTarget.forward : moveDirection;
            RotateToDirection(dir);
        }
    }

    public void UpdateMoveDirection(Transform referenceTransform = null)
    {
        if (input.magnitude <= 0.01)
        {
            moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);
            return;
        }

        if (referenceTransform && !rotateByWorld)
        {
            //get the right-facing direction of the referenceTransform
            Vector3 right = referenceTransform.right;
            right.y = 0f;
            //get the forward direction relative to referenceTransform Right
            Vector3 forward = Quaternion.AngleAxis(-90f, Vector3.up) * right;
            // determine the direction the player will face based on input and the referenceTransform's right and forward directions
            moveDirection = (inputSmooth.x * right) + (inputSmooth.z * forward);
        }
        else
        {
            moveDirection = new Vector3(inputSmooth.x, 0f, inputSmooth.z);
        }
    }

    public void Sprint(bool value)
    {
        var sprintConditions = input.sqrMagnitude > 0.1f && isGrounded && !(isStrafing && !strafeSpeed.walkByDefault && (horizontalSpeed >= 0.5f || horizontalSpeed <= -0.5f || verticalSpeed <= 0.1f));

        if (value && sprintConditions)
        {
            if (input.sqrMagnitude > 0.1f)
            {
                if (isGrounded && useContinuousSprint)
                {
                    isSprinting = !isSprinting;
                }
                else if (!isSprinting)
                {
                    isSprinting = true;
                }
            }
            else if (!useContinuousSprint && isSprinting)
            {
                isSprinting = false;
            }
        }
        else if (isSprinting)
        {
            isSprinting = false;
        }
    }

    public void Strafe()
    {
        isStrafing = !isStrafing;
    }

    public void Jump()
    {
        // trigger jump behaviour
        jumpCounter = jumpTimer;
        isJumping = true;

        // trigger jump animations
        if (input.sqrMagnitude < 0.1f)
        {
            charAnim.animator.CrossFadeInFixedTime("Jump", 0.1f);
        }
        else
        {
            charAnim.animator.CrossFadeInFixedTime("JumpMove", 0.2f);
        }
    }
}
