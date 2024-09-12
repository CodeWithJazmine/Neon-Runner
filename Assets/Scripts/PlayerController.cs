using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]

public class PlayerController : MonoBehaviour
{

    private Vector2 input;
    private CharacterController characterController;
    private Vector3 direction;
    [SerializeField] private float smoothTime = 0.05f;
    private float currentVelocity;
    [SerializeField] private float speed;

    private float gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3.0f;
    private float velocity;

    [SerializeField] private float jumpPower;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        ApplyGravity();
        ApplyRotation();
        ApplyMovement();
    }

    private void ApplyGravity()
    {
        if (isGrounded() && velocity < 0)
        {
            velocity = -1f;
        }
        else
        {
            velocity += gravity * gravityMultiplier * Time.deltaTime;
        }
        direction.y = velocity;
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
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (!isGrounded()) return;

        velocity += jumpPower;
    }

    private bool isGrounded()
    {
        return characterController.isGrounded;
    }

    private Vector3 IsoVectorConvert(Vector3 vector)
    {
        Quaternion rotation = Quaternion.Euler(0, 45.0f, 0);
        Matrix4x4 isoMatrix = Matrix4x4.Rotate(rotation);
        Vector3 result = isoMatrix.MultiplyPoint3x4(vector);
        return result;
    }
}
