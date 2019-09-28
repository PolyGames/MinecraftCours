using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    float horizontal = 0;
    float vertical = 0;
    float mouseX = 0;
    float mouseY = 0;

    float rotationX = 0;
    float rotationY = 0;

    Vector3 movement;

    Rigidbody playerRigidbody;

    [SerializeField]
    float movementSpeed = 3;

    [SerializeField]
    float viewSpeed = 3;

    [SerializeField]
    Transform camera;

    // Start is called before the first frame update
    void Start()
    {
        movement = new Vector3();
        playerRigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        GetPlayerInput();
        UpdateMovement();
        UpdateRotation();
    }

    void UpdateRotation()
    {
        rotationX += mouseX * viewSpeed;
        rotationY += mouseY * viewSpeed;
        rotationY = Mathf.Clamp(rotationY, -90, 90);

        camera.transform.localRotation = Quaternion.Euler(-rotationY, 0, 0);
        transform.rotation = Quaternion.Euler(0, rotationX, 0);
    }

    void UpdateMovement()
    {
        movement = (vertical * transform.forward + horizontal * transform.right) * movementSpeed * Time.deltaTime;

        transform.Translate(movement, Space.World);

        //playerRigidbody.velocity = movement;
    }

    void GetPlayerInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
    }
}
