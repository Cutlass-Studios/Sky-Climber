using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Weegee : MonoBehaviour
{



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
    public Text countText, highScore, totalCoins;
    int high = 0;
    public Sprite jump;
    public Sprite idle;
    public AudioSource audio1;
    public AudioSource audio2;
    public AudioClip die;
    public AudioClip jumpSound;
    public AudioClip yahoo;
    public Sprite[] blockTypes;
    public AudioClip coinSound;
    public GameObject coin;
    public bool falling;
    public int coins;
    public GameObject gameButtons;
    public GameObject playButton;



    private void Start()
    {
        Screen.orientation = ScreenOrientation.Landscape;
        gameButtons.SetActive(false);
        high = PlayerPrefs.GetInt("highscore", high);
        coins = PlayerPrefs.GetInt("coins", coins);
        highScore.text = "Highscore: " + high;
        totalCoins.text = "Coins: " + coins;
        setScore();
        spawnBlock(0);
        spawnBlock(-4);
        falling = false;
        //put me out of ,my misderuyy54e[s uot5his
    }


    // Update is called once per frame
    public void Update()
    {
        if (jumpCount > 0)
            gameObject.GetComponent<SpriteRenderer>().sprite = jump;
        else
            gameObject.GetComponent<SpriteRenderer>().sprite = idle;


        int lastLevel = level;
        level = (int)Math.Floor(gameObject.GetComponent<Rigidbody2D>().position.y) / 4 + 2;

        //checks if it goes up a level. doesjnt check going down then going back up
        if (level > lastLevel && level > levelCheck)
        {
            levelCheck = level;
            spawnBlock();
            setScore();
            moveCloud();
        }
        DoubleJumpCheck();
        CameraMove();
        spawnCoin();
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

    public void Play()
    {
        gameButtons.SetActive(true);
        playButton.SetActive(false);
    }

    public void spawnCoin()
    {

        //Compares Weegee's position to lowest block (off screen)
        if (GameObject.Find("Block #" + (blockNum - 4)) != null)
        {
            float blockY = GameObject.Find("Block #" + (blockNum - 4)).GetComponent<Transform>().position.y;
            if (gameObject.GetComponent<Rigidbody2D>().position.y < blockY && !falling && levelCheck > 10)
            {
                falling = true;

                for (int i = (int)blockY; i > 0; i--)
                {
                    Instantiate(coin, new Vector3((float) Math.Sin((double)i / 15.0) * 9, i, 0), Quaternion.identity);
                }


                Debug.Log("RUN");
            }
        }

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
        if (thisY < 0 && jumpCount == 0)
        {
            jumpCount = 1;
        }
    }

    public void CameraMove()
    {
        cam = GameObject.Find("Main Camera");
        //Camera doesn't move down if Weegee is below
        //if(gameObject.GetComponent<Rigidbody2D>().position.y > cam.GetComponent<Transform>().position.y)
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
        GameObject oldBlock = GameObject.Find("mc_dirt (14)");
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
        if (blockNum > 180)
        {
            block.GetComponent<SpriteRenderer>().sprite = blockTypes[8];
        }
        else
            block.GetComponent<SpriteRenderer>().sprite = blockTypes[blockNum / 20];
        blockNum++;
        if (level >= 3)
            Destroy(GameObject.Find("Block #" + (level - 3)));

        lastX = randX;

    }
    //aids
    public void spawnBlock(int y)
    {
        //  if (Input.GetKeyDown(KeyCode.P))
        //  {
        GameObject oldBlock = GameObject.Find("mc_dirt (14)");
        GameObject mainBlock = GameObject.Find("mc_dirt");
        System.Random random = new System.Random();

        float randX;


        do
        {
            randX = random.Next((int)(lastX - 7), (int)(lastX + 7));

        } while (randX > 9|| randX < -9);


        float randY = random.Next(-1, 0);

        Vector3 oldBlockVector = new Vector3(randX, level * 4 + randY - y, 0);
        GameObject block = (GameObject)Instantiate(oldBlock, oldBlockVector, Quaternion.identity, mainBlock.transform);
        block.name = "Block #" + blockNum;
        if (blockNum - 2 > 180)
        {
            block.GetComponent<SpriteRenderer>().sprite = blockTypes[8];
        }
        else
            block.GetComponent<SpriteRenderer>().sprite = blockTypes[(blockNum - 2) / 20];
        blockNum++;
        if (level >= 6)
            Destroy(GameObject.Find("Block #" + (level - 5)));

        lastX = randX;

    }

    public void Jump()
    {
        if (jumpCount < 2)
        {
            audio1.clip = jumpSound;
            audio1.Play();
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
        if (levelCheck > 2 && gameObject.GetComponent<Rigidbody2D>().position.y <= -2.0f)
        {

            audio2.clip = die;
            audio2.Play();
            levelCheck = blockNum;
            while (levelCheck > 0)
            {
                Destroy(GameObject.Find("Block #" + (levelCheck)));
                levelCheck--;
            }
            levelCheck = 0;

            level = 0;
            blockNum = 1;
            spawnBlock(0);
            spawnBlock(-4);
            falling = false;

            GameObject[] objs = GameObject.FindGameObjectsWithTag("Coin");
            foreach (GameObject target in objs)
            {
                GameObject.Destroy(target);
            }
        }

    }


    //sets text for score
    public void setScore()
    {
        countText.text = "Score: " + (blockNum - 4).ToString();
        PlayerPrefs.SetInt("coins", coins);
        if (blockNum - 4 > high)
        {
            highScore.text = "Highscore: " + (blockNum - 4);
            high = blockNum - 4;
            PlayerPrefs.SetInt("highscore", high);
        }
        if ((blockNum - 4) % 10 == 0 && (blockNum - 4) != 0)
        {
            audio2.clip = yahoo;
            audio2.Play();
        }
    }
    //moves cloud above luigi for """"realism""""
    public void moveCloud()
    {
        if ((blockNum - 2) % 4 == 0 && (blockNum - 2) != 0)
        {
            System.Random random = new System.Random();
            int randX = random.Next(-8, 8);
            GameObject.Find("cloud").GetComponent<Transform>().position = new Vector3(randX, gameObject.GetComponent<Rigidbody2D>().position.y + 7, 1);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Coin")
        {
            coins++;
            totalCoins.text = "Coins: " + coins.ToString();
            Destroy(collision.gameObject);
            audio2.clip = coinSound;
            audio2.Play();
        }
    }
}
