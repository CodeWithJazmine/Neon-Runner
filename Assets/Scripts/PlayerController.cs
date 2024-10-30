using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement")]
    private Vector2 input;
    private CharacterController characterController;
    private Vector3 direction;
    [SerializeField] private float smoothTime = 0.05f;
    private float currentVelocity;
    [SerializeField] private float speed;
    Animator animator;

    public GameObject inputObject;
    private PlayerInput playerInput;
    private bool isOverriding = false;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if(inputObject != null)
        {
            playerInput = inputObject.GetComponent<PlayerInput>();

            if(playerInput != null)
            {
                playerInput.actions["Override"].performed += OverridePerformed;
                playerInput.actions["Override"].canceled += OverrideCanceled;
            }
        }
    }

    public void OverrideCanceled(InputAction.CallbackContext context)
    {
        isOverriding = false;
    }

    public void OverridePerformed(InputAction.CallbackContext context)
    {
        
        isOverriding = true;
    }

    public bool GetIsOverriding()
    {
        return isOverriding;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        ApplyRotation();
        ApplyMovement();
    }

    private void ApplyRotation()
    {
        if (input.sqrMagnitude == 0) return;
        var targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref currentVelocity, smoothTime);
        transform.rotation = Quaternion.Euler(0, angle, 0);
    }

    private void ApplyMovement()
    {
        characterController.Move(direction * speed * Time.deltaTime);
    }


    public void Move(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
        Vector3 toConvert = new Vector3(input.x, 0, input.y);
        direction = IsoVectorConvert(toConvert);

        if(input == Vector2.zero)
        {
            animator.SetBool("isWalking", false);
        }
        else
        {
            animator.SetBool("isWalking", true);
        }
    }

    private Vector3 IsoVectorConvert(Vector3 vector)
    {
        Quaternion rotation = Quaternion.Euler(0, 45.0f, 0);
        Matrix4x4 isoMatrix = Matrix4x4.Rotate(rotation);
        Vector3 result = isoMatrix.MultiplyPoint3x4(vector);
        return result;
    }
}
