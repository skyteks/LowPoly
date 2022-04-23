using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public static class AnimatorParameters
    {
        public static int InputHorizontal = Animator.StringToHash("InputHorizontal");
        public static int InputVertical = Animator.StringToHash("InputVertical");
        public static int InputMagnitude = Animator.StringToHash("InputMagnitude");
        public static int IsGrounded = Animator.StringToHash("IsGrounded");
        public static int IsStrafing = Animator.StringToHash("IsStrafing");
        public static int IsSprinting = Animator.StringToHash("IsSprinting");
        public static int GroundDistance = Animator.StringToHash("GroundDistance");
    }

    public Animator animator { get; private set; }
    private CharacterLocomotion charMotor;

    public const float walkSpeed = 0.5f;
    public const float runningSpeed = 1f;
    public const float sprintSpeed = 1.5f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        charMotor = GetComponent<CharacterLocomotion>();
    }

    void Start()
    {
        animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
    }

    public void UpdateAnimator()
    {
        if (animator == null || !animator.enabled) return;

        animator.SetBool(AnimatorParameters.IsStrafing, charMotor.isStrafing);
        animator.SetBool(AnimatorParameters.IsSprinting, charMotor.isSprinting);
        animator.SetBool(AnimatorParameters.IsGrounded, charMotor.isGrounded);
        animator.SetFloat(AnimatorParameters.GroundDistance, charMotor.groundDistance);

        if (charMotor.isStrafing)
        {
            animator.SetFloat(AnimatorParameters.InputHorizontal, charMotor.stopMove ? 0f : charMotor.horizontalSpeed, charMotor.strafeSpeed.animationSmooth, Time.deltaTime);
            animator.SetFloat(AnimatorParameters.InputVertical, charMotor.stopMove ? 0f : charMotor.verticalSpeed, charMotor.strafeSpeed.animationSmooth, Time.deltaTime);
        }
        else
        {
            animator.SetFloat(AnimatorParameters.InputVertical, charMotor.stopMove ? 0f : charMotor.verticalSpeed, charMotor.freeSpeed.animationSmooth, Time.deltaTime);
        }

        animator.SetFloat(AnimatorParameters.InputMagnitude, charMotor.stopMove ? 0f : charMotor.inputMagnitude, charMotor.isStrafing ? charMotor.strafeSpeed.animationSmooth : charMotor.freeSpeed.animationSmooth, Time.deltaTime);
    }

    public void SetAnimatorMoveSpeed(CharacterLocomotion.MovementSpeed speed)
    {
        Vector3 relativeInput = transform.InverseTransformDirection(charMotor.moveDirection);
        charMotor.verticalSpeed = relativeInput.z;
        charMotor.horizontalSpeed = relativeInput.x;

        var newInput = new Vector2(charMotor.verticalSpeed, charMotor.horizontalSpeed);

        if (speed.walkByDefault)
        {
            charMotor.inputMagnitude = Mathf.Clamp(newInput.magnitude, 0f, charMotor.isSprinting ? runningSpeed : walkSpeed);
        }
        else
        {
            charMotor.inputMagnitude = Mathf.Clamp(charMotor.isSprinting ? newInput.magnitude + 0.5f : newInput.magnitude, 0f, charMotor.isSprinting ? sprintSpeed : runningSpeed);
        }
    }
}
