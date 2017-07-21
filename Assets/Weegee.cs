using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Weegee : MonoBehaviour {

    public bool facingRight = true;
    public int playerSpeed = 10;
    public int playerJumpPower = 30;
    private bool isMovingLeft = false;
    private bool isMovingRight = false;
    public int jumpCount = 0;
    private float lastY = 0.0f;
    private float thisY = 0.0f;
    public GameObject cam;
    public int level = 0;
    public int blockNum = 1;


    // Update is called once per frame
    void Update () {
        int lastLevel = level;
        level = (int) Math.Floor(gameObject.GetComponent<Rigidbody2D>().position.y)/4;
        //checks if it goes up a level. doesjnt check going down then going back up
        if(level > lastLevel)
            spawnBlock();
        DoubleJumpCheck();
        CameraMove();
      
        //Used for keyboard testing (can be removed)
        if (Input.GetButtonDown("JumpKeyboard")) Jump();
        if (Input.GetButtonDown("LeftKeyboard")) MoveLeft();
        if (Input.GetButtonDown("RightKeyboard")) MoveRight();
        if (Input.GetButtonUp("LeftKeyboard") || Input.GetButtonUp("RightKeyboard")) Stop();
        //Movement is the only thing requiring updates
        MovePlayer();
	}

    public void MovePlayer()
    {
        if (isMovingRight)
        {
            gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(playerSpeed, gameObject.GetComponent<Rigidbody2D>().velocity.y);
        }
        if (isMovingLeft)
        {
            gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(-playerSpeed, gameObject.GetComponent<Rigidbody2D>().velocity.y);
        }
    }

    public void DoubleJumpCheck()
    {
        lastY = thisY;
        thisY = gameObject.GetComponent<Rigidbody2D>().velocity.y;
        if (lastY == 0.0f && thisY == 0.0f)
        {
            jumpCount = 0;
        }
        if(thisY <0 && jumpCount == 0)
        {
            jumpCount = 1;
        }
    }

    public void CameraMove ()
    {
        cam = GameObject.Find("Main Camera");
        //Camera doesn't move down if Weegee is below
        //if(gameObject.GetComponent<Rigidbody2D>().position.y > camera.GetComponent<Transform>().position.y)
        cam.GetComponent<Transform>().position = new Vector3(cam.GetComponent<Transform>().position.x, gameObject.GetComponent<Rigidbody2D>().position.y, cam.GetComponent<Transform>().position.z);
        
    }

    public void MoveLeft()
    {
        isMovingLeft = true;
        if (facingRight)
        {
            FlipPlayer();
        }

    }
  
    public void Stop()
    {
        isMovingLeft = false;
        isMovingRight = false;
    }

    public void MoveRight()
    {
        isMovingRight = true;
        if (!facingRight)
        {
            FlipPlayer();
        }
    }

    public void spawnBlock()
    {
      //  if (Input.GetKeyDown(KeyCode.P))
      //  {
            GameObject oldBlock = GameObject.Find("mc_dirt (15)");
            GameObject mainBlock = GameObject.Find("mc_dirt");
            System.Random random = new System.Random();
            float randX = random.Next(-9, 9);
            float randY = random.Next(-1, 1);
            Vector3 oldBlockVector = new Vector3(randX, level*4 +randY, 0);
            GameObject block = (GameObject)Instantiate(oldBlock, oldBlockVector, Quaternion.identity, mainBlock.transform);
            block.name = "Block #" + blockNum;
            blockNum++;
          //  if(level >= 5)
              // Destroy( GameObject.Find("Block #" + level));

        //}
    }

    public void Jump()
    {
        if (jumpCount <2)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(gameObject.GetComponent<Rigidbody2D>().velocity.x, playerJumpPower);
            jumpCount++;
        }
      
        /*
        if (jumpCount < 2)
        {
            GetComponent<Rigidbody2D>().AddForce(Vector2.up * playerJumpPower);
            jumpCount++;
        }
        */
        
    }

    void FlipPlayer()
    {
        facingRight = !facingRight;
        Vector2 localScale = gameObject.transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }
}
