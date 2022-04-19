using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControllsInput : MonoBehaviour
{
    [Header("Controller Input")]
    public string horizontalInput = "Horizontal";
    public string verticallInput = "Vertical";
    public KeyCode jumpInput = KeyCode.Space;
    public KeyCode strafeInput = KeyCode.Tab;
    public KeyCode sprintInput = KeyCode.LeftShift;

    private CharacterLocomotion charMotor;
    private CharacterAnimator charAnim;
    private CameraControllsInput camControll;

    void Awake()
    {
        charMotor = GetComponent<CharacterLocomotion>();
        charAnim = GetComponent<CharacterAnimator>();
    }

    void Start()
    {
        charMotor.Init();
        InitializeTpCamera();
    }

    void FixedUpdate()
    {
        charMotor.UpdateMotor();               // updates the ThirdPersonMotor methods
        charMotor.ControlLocomotionType();     // handle the controller locomotion type and movespeed
        charMotor.ControlRotationType();       // handle the controller rotation type
    }

    void Update()
    {
        InputHandle();                  // update the input methods
        charAnim.UpdateAnimator();            // updates the Animator Parameters
    }

    void OnAnimatorMove()
    {
        charMotor.ControlAnimatorRootMotion(); // handle root motion animations 
    }

    private void InitializeTpCamera()
    {
        if (camControll == null)
        {
            camControll = FindObjectOfType<CameraControllsInput>();
            if (camControll == null)
                return;
            if (camControll)
            {
                camControll.SetMainTarget(this.transform);
                camControll.Init();
            }
        }
    }

    private void InputHandle()
    {
        MoveInput();
        CameraInput();
        SprintInput();
        StrafeInput();
        JumpInput();
    }

    public void MoveInput()
    {
        charMotor.input = new Vector3(Input.GetAxis(horizontalInput), charMotor.input.y, Input.GetAxis(verticallInput));
    }

    private void CameraInput()
    {
        if (Camera.main == null)
        {
            return;
        }

        charMotor.UpdateMoveDirection(Camera.main.transform);

        if (camControll == null)
        {
            return;
        }

        float y = Input.GetAxis("Mouse Y");
        float x = Input.GetAxis("Mouse X");

        camControll.RotateCamera(x, y);
    }

    private void StrafeInput()
    {
        if (Input.GetKeyDown(strafeInput))
        {
            charMotor.Strafe();
        }
    }

    private void SprintInput()
    {
        if (Input.GetKeyDown(sprintInput))
        {
            charMotor.Sprint(true);
        }
        else if (Input.GetKeyUp(sprintInput))
        {
            charMotor.Sprint(false);
        }
    }

    /// <summary>
    /// Conditions to trigger the Jump animation & behavior
    /// </summary>
    /// <returns></returns>
    private bool JumpConditions()
    {
        return charMotor.isGrounded && charMotor.GroundAngle() < charMotor.slopeLimit && !charMotor.isJumping && !charMotor.stopMove;
    }

    /// <summary>
    /// Input to trigger the Jump 
    /// </summary>
    private void JumpInput()
    {
        if (Input.GetKeyDown(jumpInput) && JumpConditions())
        {
            charMotor.Jump();
        }
    }
}
