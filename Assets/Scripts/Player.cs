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
    bool jumpRequest = false;
    bool isGrounded = false;

    float rotationX = 0;
    float rotationY = 0;
    float verticalMomentum = 0;
    float colliderWidth = 0.3f;

    Vector3 movement;

    [SerializeField]
    float movementSpeed = 3;

    [SerializeField]
    float viewSpeed = 3;

    [SerializeField]
    Transform camera;

    [SerializeField]
    Transform blockHighlight;

    Vector3 placeBlockPosition;

    float checkIncrement = 0.1f;
    float reach = 8;

    [SerializeField]
    World world;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        movement = new Vector3();
    }

    // Update is called once per frame
    void Update()
    {
        GetPlayerInput();
        if (blockHighlight.gameObject.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // destroy block
                world.GetChunkFromVector3(blockHighlight.position).EditVoxel(blockHighlight.position, 0);
            }

            if (Input.GetMouseButtonDown(1))
            {
                // place block
                world.GetChunkFromVector3(placeBlockPosition).EditVoxel(placeBlockPosition, 1);

            }
        }

        PlaceBlockHighlight();
    }

    void PlaceBlockHighlight()
    {
        float step = checkIncrement;
        Vector3 lastPlacePosition = new Vector3();

        while (step < reach)
        {
            Vector3 placePosition = camera.position + (camera.forward * step);

            if (world.CheckForVoxel(placePosition))
            {
                blockHighlight.gameObject.SetActive(true);
                blockHighlight.position = new Vector3(Mathf.FloorToInt(placePosition.x),
                                                      Mathf.FloorToInt(placePosition.y),
                                                      Mathf.FloorToInt(placePosition.z));
                placeBlockPosition = lastPlacePosition;
                return;
            }

            lastPlacePosition = new Vector3(Mathf.FloorToInt(placePosition.x),
                                            Mathf.FloorToInt(placePosition.y),
                                            Mathf.FloorToInt(placePosition.z));
            step += checkIncrement;
        }

        blockHighlight.gameObject.SetActive(false);
    }

    void FixedUpdate()
    {
        if (jumpRequest)
        {
            Jump();
        }

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

    void Jump()
    {
        verticalMomentum = 5f;
        jumpRequest = false;
        isGrounded = false;
    }

    void UpdateMovement()
    {
        movement = (vertical * transform.forward + horizontal * transform.right) * movementSpeed * Time.deltaTime;

        if (verticalMomentum > World.GRAVITY)
        {
            verticalMomentum += World.GRAVITY * Time.fixedDeltaTime;
        }
        movement += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if (movement.y < 0)
        {
            movement.y = CheckDownSpeed(movement.y);
        }

        if ((movement.z > 0 && CheckFrontCollision()) || (movement.z < 0 && CheckBackCollision()))
        {
            movement.z = 0;
        }
        if ((movement.x > 0 && CheckRightCollision()) || (movement.x < 0 && CheckLeftCollision()))
        {
            movement.x = 0;
        }

        transform.Translate(movement, Space.World);
    }

    float CheckDownSpeed(float currentMomentum)
    {
        Vector3 leftBack = new Vector3(transform.position.x - colliderWidth, transform.position.y + currentMomentum, transform.position.z - colliderWidth);
        Vector3 rightBack = new Vector3(transform.position.x + colliderWidth, transform.position.y + currentMomentum, transform.position.z - colliderWidth);
        Vector3 leftFront = new Vector3(transform.position.x - colliderWidth, transform.position.y + currentMomentum, transform.position.z + colliderWidth);
        Vector3 rightFront = new Vector3(transform.position.x + colliderWidth, transform.position.y + currentMomentum, transform.position.z + colliderWidth);

        if (world.CheckForVoxel(leftBack) || world.CheckForVoxel(rightBack) || world.CheckForVoxel(leftFront) || world.CheckForVoxel(rightFront))
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return currentMomentum;
        }
    }

    bool CheckFrontCollision()
    {
        return (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + colliderWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z + colliderWidth)));
    }

    bool CheckBackCollision()
    {
        return (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - colliderWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z - colliderWidth)));
    }

    bool CheckRightCollision()
    {
        return (world.CheckForVoxel(new Vector3(transform.position.x + colliderWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x + colliderWidth, transform.position.y + 1, transform.position.z)));
    }

    bool CheckLeftCollision()
    {
        return (world.CheckForVoxel(new Vector3(transform.position.x - colliderWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x - colliderWidth, transform.position.y + 1, transform.position.z)));
    }

    void GetPlayerInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }
    }
}
