using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 moveVector;

    private float speed = 5.0f;
    private float verticalSpeed = 0.0f;
    private float gravity = 10.0f;

    private float animationDuration = 3.0f;

    void Start() {
        controller = GetComponent<CharacterController>();
    }

    void Update() {
        if (Time.timeSinceLevelLoad < animationDuration) {
            controller.Move(Vector3.forward * speed * Time.deltaTime);
            return;
        }

        moveVector = Vector3.zero;

        if (controller.isGrounded) {
            verticalSpeed = -0.5f;
        } else {
            verticalSpeed -= gravity *Time.deltaTime;
        }

        // X - Left & Right
        moveVector.x = Input.GetAxisRaw("Horizontal") * speed;
        // Y - Up & Down
        moveVector.y = verticalSpeed;
        // Z - Forward & Backward
        moveVector.z = speed;
        controller.Move(moveVector * Time.deltaTime);
    }
}
