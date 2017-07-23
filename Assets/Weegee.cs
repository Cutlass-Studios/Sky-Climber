using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

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
    //checks to make sure level doesnt happen twice 
   public int levelCheck = 0;
    //lastX is position of previous block so next block is within jumping distance
    float lastX = 0;
    //Stores text for score and high score
    public Text countText,highScore;
    int high = 0;
	public Sprite jump;
	public Sprite idle;
	public AudioSource audio1;
	public AudioSource audio2;
	public AudioClip die;
	public AudioClip jumpSound;
	public AudioClip yahoo;

    private void Start()
    {
		
      high =  PlayerPrefs.GetInt("highscore", high);
        highScore.text = "Highscore: " + high;
        setScore();
    
    }


    // Update is called once per frame
    public void Update () {
		
		if (jumpCount > 0)
			gameObject.GetComponent<SpriteRenderer> ().sprite = jump;
		else
			gameObject.GetComponent<SpriteRenderer> ().sprite = idle;
        int lastLevel = level;
        level = (int) Math.Floor(gameObject.GetComponent<Rigidbody2D>().position.y)/4 +2;

        //checks if it goes up a level. doesjnt check going down then going back up
        if (level > lastLevel)
        {

            if (level > levelCheck)
            {
                levelCheck = level;
                spawnBlock();
            }
        }
        DoubleJumpCheck();
        CameraMove();
      
        //Used for keyboard testing (can be removed)
        if (Input.GetButtonDown("JumpKeyboard")) Jump();
        if (Input.GetButtonDown("LeftKeyboard")) MoveLeft();
        if (Input.GetButtonDown("RightKeyboard")) MoveRight();
        if (Input.GetButtonUp("LeftKeyboard") || Input.GetButtonUp("RightKeyboard")) Stop();
        //Movement is the only thing requiring updates
        MovePlayer();
        //resets when player dies
        Reset();
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

        float randX;

      
        do
        {
            randX = random.Next((int)(lastX - 7), (int)(lastX + 7));
           
        } while (randX > 9 || randX < -9);

        
        float randY = random.Next(-1, 0);
   
            Vector3 oldBlockVector = new Vector3(randX, level * 4 + randY, 0);
            GameObject block = (GameObject)Instantiate(oldBlock, oldBlockVector, Quaternion.identity, mainBlock.transform);
            block.name = "Block #" + blockNum;
            blockNum++;
         if(level >= 6)
        Destroy( GameObject.Find("Block #" + (level-5)));

       lastX = randX;
        setScore();
    }

    public void Jump()
    {
        if (jumpCount <2)
        {
			audio1.clip = jumpSound;
			audio1.Play ();
            GetComponent<Rigidbody2D>().velocity = new Vector2(gameObject.GetComponent<Rigidbody2D>().velocity.x, playerJumpPower);
            jumpCount++;
        }  
               
    }

    void FlipPlayer()
    {
        facingRight = !facingRight;
        Vector2 localScale = gameObject.transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }
    //reset
    public void Reset()
    {
       
       
        if (levelCheck > 3 && gameObject.GetComponent<Rigidbody2D>().position.y <= -2) {
			audio2.clip = die;
			audio2.Play ();
            while (levelCheck > 0)
            {
                Destroy(GameObject.Find("Block #" + (levelCheck)));
                levelCheck--;
            }
            levelCheck = 0;
            
            level = 0;
        	blockNum = 1;

    }
    }
    

    //sets text for score
    public void setScore()
    {
        countText.text = "Score: " +(blockNum-2).ToString();
        if (blockNum > high)
        {
            highScore.text = "Highscore: " + (blockNum - 2);
            high = blockNum - 2;
            PlayerPrefs.SetInt("highscore", high);
        }
		if ((blockNum - 2) % 10 == 0 && (blockNum - 2) != 0) {
			audio2.clip = yahoo;
			audio2.Play();
		}
    }
}
