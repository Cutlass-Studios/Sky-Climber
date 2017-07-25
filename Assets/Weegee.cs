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
    public Text countText, highScore, totalCoins, deathScore, deathHigh;
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
    public GameObject mainMenu;
    public GameObject deathMenu;
    public Sprite muted, soundOn;
    public bool sound;
    public GameObject bigJump;
    public float startTime;
    public Text timerText;

    private void Start()
    {
        Screen.orientation = ScreenOrientation.Landscape;
        gameButtons.SetActive(false);
        deathMenu.SetActive(false);
        mainMenu.SetActive(true);
        timerText.gameObject.SetActive(false);
        high = PlayerPrefs.GetInt("highscore", high);
        coins = PlayerPrefs.GetInt("coins", coins);
        highScore.text = "Highscore: " + high;
        totalCoins.text = "Coins: " + coins;
        setScore();
        falling = false;
    }

    public void toggleSound()
    {
        if (sound)
        {
            mainMenu.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = muted;
            
        }
        else
        {
            mainMenu.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = soundOn;
        }
        sound = !sound;
        audio1.mute = !audio1.mute;
        audio2.mute = !audio2.mute;
    }

    // Update is called once per frame
    public void Update()
    {

        if (startTime + 10 <= Time.realtimeSinceStartup && playerJumpPower > 30)
        {
            timerText.gameObject.SetActive(false);
            playerJumpPower = 30;
        }
        if (playerJumpPower > 30) timerText.text = ((int) (startTime + 10 - Time.realtimeSinceStartup)).ToString();

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

            if (gameButtons.activeInHierarchy)
            {
                Debug.Log("ACTIVE");
                spawnBlock(0);
            }

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
        deathMenu.SetActive(false);
        mainMenu.SetActive(false);
        spawnBlock(8);
        spawnBlock(4);
        spawnBlock(0);

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
                    Instantiate(coin, new Vector3((float)Math.Sin((double)i / 15.0) * 8, i, 0), Quaternion.identity);
                }


                Debug.Log("RUN");
            }
        }

    }

    public void spawnBigJump(float x, float y)
    {
        Instantiate(bigJump, new Vector3(x, y, 0) , Quaternion.identity);
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

        } while (randX > 8 || randX < -8);


        float randY = random.Next(-1, 0);

        Vector3 oldBlockVector = new Vector3(randX, level * 4 + randY - y, 0);
        GameObject block = (GameObject)Instantiate(oldBlock, oldBlockVector, Quaternion.identity, mainBlock.transform);
        block.name = "Block #" + blockNum;
        if (blockNum % 10 == 0)
        {
            spawnBigJump(block.GetComponent<Transform>().position.x, block.GetComponent<Transform>().position.y + 2);
        }
        if (levelCheck - 2 > 180)
        {
            block.GetComponent<SpriteRenderer>().sprite = blockTypes[8];
        }
        else
            block.GetComponent<SpriteRenderer>().sprite = blockTypes[(levelCheck) / 20];
        blockNum++;
        if (level >= 4)
            Destroy(GameObject.Find("Block #" + (level - 3)));
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
            deathScore.text = "Score: " + (levelCheck - 2);

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
            falling = false;

            GameObject[] objs = GameObject.FindGameObjectsWithTag("Coin");
            foreach (GameObject target in objs)
            {
                GameObject.Destroy(target);
            }
            deathMenu.SetActive(true);
            gameButtons.SetActive(false);
            deathHigh.text = "Highscore: " + (high);
            playerJumpPower = 30;
        }

    }


    //sets text for score
    public void setScore()
    {

        int tempScore = levelCheck - 2;
        countText.text = "Score: " + (tempScore).ToString();
        PlayerPrefs.SetInt("coins", coins);
        if (tempScore > high)
        {
            highScore.text = "Highscore: " + (tempScore);
            high = tempScore;
            PlayerPrefs.SetInt("highscore", high);
        }
        if ((tempScore) % 10 == 0 && (tempScore) != 0)
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

    public void goToMenu()
    {
        gameButtons.SetActive(false);
        deathMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.CompareTag("bigJumps"))
        {
            timerText.gameObject.SetActive(true);
            Destroy(collision.gameObject);
            startTime = Time.realtimeSinceStartup;
            playerJumpPower = 45;
            audio2.clip = coinSound;
            audio2.Play();

        }
        if (collision.gameObject.CompareTag("Coin"))
        {
            coins++;
            totalCoins.text = "Coins: " + coins.ToString();
            Destroy(collision.gameObject);
            audio2.clip = coinSound;
            audio2.Play();
        }
    }
}
