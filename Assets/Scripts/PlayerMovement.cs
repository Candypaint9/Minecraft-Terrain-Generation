using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Transform worldObject;
    private World world;
    Transform camera;
    public GameObject underWaterTint;

    public bool spectatorMode = true;
    bool isSprinting;

    public float walkSpeed;
    public float runSpeed;
    public float gravity;

    float mouseHorizontal;
    float mouseVertical;
    float horizontal;
    float vertical;
    float y;

    bool cursorLocked;


    private void Start()
    {
        world = worldObject.GetComponent<World>();
        camera = GetComponentInChildren<Camera>().transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cursorLocked = true;
    }


    private void Update()
    {
        CheckIfInWater();

        Inputs();

        transform.Rotate(Vector3.up * mouseHorizontal * world.settings.mouseSens);
        camera.Rotate(Vector3.right * -mouseVertical * world.settings.mouseSens);

        float speed = 0;

        if (isSprinting)
        {
            speed = runSpeed;
        }
        else if (!isSprinting)
        {
            speed = walkSpeed;
        }

        Vector3 move = new Vector3(horizontal * speed, y * speed, vertical * speed);
        transform.Translate(move * Time.deltaTime);
    }

    
    void CheckIfInWater()
    {
        Vector3 pos = camera.position;

        bool inWater = world.isAirPresent(pos, null, null)[2];

        if (inWater)
        {
            underWaterTint.SetActive(true);
        }
        else
        {
            underWaterTint.SetActive(false);
        }
    }

    
    void Inputs()
    {
        //get inputs
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isSprinting = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isSprinting = false;
        }

        if (spectatorMode)
        {
            y = 0;
            if (Input.GetKey(KeyCode.Space))
            {
                y = 1;
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                y = -1;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            cursorLocked = false;
        }
        if (!cursorLocked && Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cursorLocked = true;
        }
    }
}
