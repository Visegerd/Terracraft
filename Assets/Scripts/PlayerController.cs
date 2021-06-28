using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed=3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.8f;
    public float playerWidth = 0.3f;
    public float boundTolerance = 0.05f;
    World world;
    Transform cam;
    float horizontal;
    float vertical;
    float mouseHorizontal;
    float mouseVertical;
    public Vector3 velocity;
    float verticalMomentum = 0;
    bool jumpRequest;
    bool isGrounded;
    bool isSprinting;
    public Transform highlightBlock;
    public Transform placeBlock;
    public float checkIncrement = 0.1f;
    public float reach = 8.0f;
    public byte selectedBlockIndex = 1;
    public Toolbar tb;
    //public Text selectedBlockText;

    private void Start()
    {
        cam = transform.GetChild(0).transform;
        world = GameObject.Find("World").GetComponent<World>();
        Cursor.lockState = CursorLockMode.Locked;
        //selectedBlockText.text = world.blockTypes[selectedBlockIndex].blockName + " block selected";
    }

    private void FixedUpdate()
    {
        if (!world.isInUI)
        {
            CalculateVelocity();
            if (jumpRequest)
                Jump();
            transform.Rotate(Vector3.up, mouseHorizontal);
            cam.Rotate(Vector3.right, -mouseVertical);
            transform.Translate(velocity, Space.World);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            world.isInUI = !world.isInUI;
        }
        if (!world.isInUI)
        {
            GetPlayerInput();
            PlaceCursorBlocks();
        }
    }

    void GetPlayerInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");
        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = true;
        }
        if(Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
        }
        if(isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }
        /*float scroll = Input.GetAxis("Mouse ScrollWheel");
        if(scroll != 0)
        {
            if(scroll>0)
            {
                selectedBlockIndex++;
            }
            else
            {
                selectedBlockIndex--;
            }
            if (selectedBlockIndex > (byte)(world.blockTypes.Length - 1))
            {
                selectedBlockIndex = 1;
            }
            else if(selectedBlockIndex<1)
            {
                selectedBlockIndex = (byte)(world.blockTypes.Length - 1);
            }
            selectedBlockText.text = world.blockTypes[selectedBlockIndex].blockName + " block selected";
        }*/
        if(highlightBlock.gameObject.activeSelf)
        {
            if(Input.GetMouseButtonDown(0))
            {
                world.GetChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position, 0);
            }
            else if (Input.GetMouseButtonDown(1))
            {
                if (tb.slots[tb.slotIndex].HasItem)
                {
                    world.GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, tb.slots[tb.slotIndex].itemSlot.stack.id);
                    tb.slots[tb.slotIndex].itemSlot.Take(1);
                }
            }
        }
    }

    void PlaceCursorBlocks()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();
        lastPos = cam.position;
        //Debug.DrawRay(cam.position, cam.forward*reach, Color.red, 5.0f);
        while(step<reach)
        {
            //Debug.Log("Cam pos: " + cam.position + " , Ray point: " + (cam.forward * step));
            Vector3 pos = cam.position + (cam.forward * step);
            if(world.CheckForVoxel(pos))
            {
                //Debug.Log("Voxel pos: " + pos + " , Floored values: " + Mathf.FloorToInt(pos.x) + " , " + Mathf.FloorToInt(pos.y) + " , " + Mathf.FloorToInt(pos.z));
                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;
                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);
                return;
            }
            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            step += checkIncrement;
        }
    }

    float CheckDownSpeed(float downSpeed)
    {
        if(world.CheckForVoxel(new Vector3(transform.position.x - playerWidth-boundTolerance, transform.position.y + downSpeed, transform.position.z - playerWidth - boundTolerance)) ||
           world.CheckForVoxel(new Vector3(transform.position.x + playerWidth + boundTolerance, transform.position.y + downSpeed, transform.position.z - playerWidth - boundTolerance)) ||
           world.CheckForVoxel(new Vector3(transform.position.x - playerWidth - boundTolerance, transform.position.y + downSpeed, transform.position.z + playerWidth + boundTolerance)) ||
           world.CheckForVoxel(new Vector3(transform.position.x + playerWidth + boundTolerance, transform.position.y + downSpeed, transform.position.z + playerWidth + boundTolerance)))
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }

    float CheckUpSpeed(float upSpeed)
    {
        if (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1.85f + upSpeed, transform.position.z - playerWidth)) ||
           world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1.85f + upSpeed, transform.position.z - playerWidth)) ||
           world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1.85f + upSpeed, transform.position.z + playerWidth)) ||
           world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1.85f + upSpeed, transform.position.z + playerWidth)))
        {
            return 0;
        }
        else
        {
            isGrounded = false;
            return upSpeed;
        }
    }

    public bool front
    {
        get
        {
            if ((world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1.0f, transform.position.z + playerWidth))))
                return true;
            else return false;
        }
    }

    public bool back
    {
        get
        {
            if ((world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1.0f, transform.position.z - playerWidth))))
                return true;
            else return false;
        }
    }

    public bool right
    {
        get
        {
            if ((world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1.0f, transform.position.z))))
                return true;
            else return false;
        }
    }

    public bool left
    {
        get
        {
            if ((world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1.0f, transform.position.z))))
                return true;
            else return false;
        }
    }

    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    void CalculateVelocity()
    {
        //velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.deltaTime * walkSpeed;
        //velocity += Vector3.up * gravity * Time.deltaTime;
        //velocity.y = CheckDownSpeed(velocity.y);
        if(verticalMomentum>gravity*10.0f && !isGrounded)
        {
            verticalMomentum += Time.fixedDeltaTime * gravity;
        }
        if(isSprinting)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.deltaTime * sprintSpeed;
        else
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.deltaTime * walkSpeed;
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;
        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;
        if (velocity.y < 0)
            velocity.y = CheckDownSpeed(velocity.y);
        else if (velocity.y > 0)
            velocity.y = CheckUpSpeed(velocity.y);
    }
}

