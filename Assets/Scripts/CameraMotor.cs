using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotor : MonoBehaviour
{
    private Transform target;
    private Vector3 startOffset;
    private Vector3 moveVector;

    private float transition = 0.0f;
    private float animationDuration = 3.0f;
    private Vector3 animationOffset = new Vector3(0, 5, 5);

    void Start() {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        startOffset = transform.position - target.position;
    }

    void Update() {
        moveVector = target.position + startOffset;
        // X
        moveVector.x = 0;
        // Y
        moveVector.y = Mathf.Clamp(moveVector.y, 2, 5);
        if (transition > 1.0f) {
            transform.position = moveVector;
        } else {
            //Animation at game start
            transform.position = Vector3.Lerp(moveVector + animationOffset,
                moveVector, transition);
            transition += Time.deltaTime / animationDuration;
            transform.LookAt(target.position + Vector3.up);
        }

    }
}
