using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Weegee : MonoBehaviour
{
    //--- WEEGEE PROPERTIES ---
    //Movement
    public bool facingRight = true;
    public int playerSpeed = 10;
    public int playerJumpPower = 30;
    private bool isMovingLeft = false;
    private bool isMovingRight = false;
    //Weegee's current and previous Y Coordinates
    private float lastY = 0.0f;
    private float thisY = 0.0f;
    //Used to limit the amount of times Weegee can Jump
    public int jumpCount = 0;
    public int jumpLimit = 2;
    public int scoreMultiplier = 1;
    //Animations
    public Sprite jump;
    public Sprite idle;

    //--- WORLD GAME OBJECTS ---
    public GameObject cam;
    //Stores text for score and high score which will appear on the User Interface
    public Text countText, highScore, totalCoins, deathScore, deathHigh, timerText;
    //Collectables
    public GameObject bigJump;
    public GameObject coin;
    public GameObject unlimitedJump;
    public GameObject doubleScore;
    //Menus
    public GameObject gameButtons;
    public GameObject mainMenu;
    public GameObject deathMenu;
    public GameObject powerUp;
    public GameObject shopMenu;
    public GameObject coinsText;
    //Sounds
    //Responsible for fucking nothing, kys chris
    public AudioSource audio1;
    //Responsible for Handling collectable sounds (coins, powerups) and sounds Weegee Makes                           
    public AudioSource audio2;
    public AudioClip die;
    public AudioClip jumpSound;
    public AudioClip yahoo;
    public AudioClip coinSound;
    public AudioClip saleSound;
    //Textures
    public Sprite[] blockTypes;
    public Sprite[] powerUps;
    public Sprite muted, soundOn;
    public Sprite skin;

    //--- GAME LOGIC ---
    public int level = 0;
    public int blockNum = 1;
    public bool falling;
    protected int score = -2;
    //Used to make sure Weegee doesn't drop down to a previous level and go back up to a new level in attempt to increase the level
    public int levelCheck = 0;
    //lastX is position of previous block so next block is within jumping distance
    float lastX = 0;
    //Is Sound On
    public bool sound;
    //Timer - A float which indicates the beginning of an event (used for Powerups)
    public float startTime;
    //used to display the highscore and number of coins found in the PlayerPrefs (these values are stored in memory)
    public int high = 0;
    public int coins;
    public Animator anim;
    public int blockChoice;
    //shop variable
    //false means unlocked
    public bool[] unlockables;  

    private bool  skinChanged = false;



    //Method that is called when game begins
    private void Start()
    {
        //set store
        unlockables = new bool[9];
     

        //Force the Screen into landscape
        Screen.orientation = ScreenOrientation.Landscape;

        //Displays only main menu upon start
        gameButtons.SetActive(false);
        deathMenu.SetActive(false);
        mainMenu.SetActive(true);
        timerText.gameObject.SetActive(false);
        coinsText.SetActive(false);
        shopMenu.SetActive(false);
        //Retrieve highscore and coins from memory, and then display them
        high = PlayerPrefs.GetInt("highscore", high);
        coins = PlayerPrefs.GetInt("coins", coins);
        highScore.text = "Highscore: " + high;
        totalCoins.text = ""+ coins;

        //Set the Score (will be zero because the game just started)
        setScore();
        falling = false; //NOTE: be set when instantiated
    }
    public void OpenShop()
    {
        //Debug.Log("shop open");
        shopMenu.SetActive(true);
        gameButtons.SetActive(false);
        deathMenu.SetActive(false);
        mainMenu.SetActive(false);
        timerText.gameObject.SetActive(false);
        totalCoins.text = coins.ToString();
        coinsText.SetActive(true);

    }
    public void toggleSound()
    {
        //if sound is on, turn it off and vice versa
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

    // Update is called once per frame (60fps)
    public void Update()
    {

        if (jumpCount > 0)
        {
            anim.Play("jump");
        }
        else if (isMovingLeft || isMovingRight)
        {
            anim.Play("walk");
        }
        else
        {
            if (skinChanged) anim.Play("idleNib");
            else anim.Play("idle");
        }
        //If the time since start exceeds 10 seconds after a powerup is taken (10s limit), AND the BigJumps Powerup is active, remove timer and limit playerJumpPower
        if (startTime + 5 <= Time.realtimeSinceStartup && playerJumpPower > 30)
        {
            timerText.gameObject.SetActive(false);
            playerJumpPower = 30;
        }
        if (startTime + 4 <= Time.realtimeSinceStartup && jumpLimit > 2)
        {
            timerText.gameObject.SetActive(false);
            jumpLimit = 2;
        }
        if (startTime + 10 <= Time.realtimeSinceStartup && scoreMultiplier > 1)
        {
            timerText.gameObject.SetActive(false);
            scoreMultiplier = 1;
        }

        //If a player has a Powerup, enable a timer which countsdown from 10, only displays int
        if (scoreMultiplier > 1)
        {
            timerText.text = ((int)(startTime + 11 - Time.realtimeSinceStartup)).ToString();
        }
        if(playerJumpPower > 30)
            timerText.text = ((int)(startTime + 6 - Time.realtimeSinceStartup)).ToString();
        if (jumpLimit > 2)
            timerText.text = ((int)(startTime + 5 - Time.realtimeSinceStartup)).ToString();

        //Change sprites depending if Weegee is mid jump
        if (jumpCount > 0)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = jump;
        }
        else
        {
            if (skinChanged)
                gameObject.GetComponent<SpriteRenderer>().sprite = skin;
            else gameObject.GetComponent<SpriteRenderer>().sprite = idle;
        }

        //lastLevel becomes what level is. level updates
        int lastLevel = level;
        level = (int)Math.Floor(gameObject.GetComponent<Rigidbody2D>().position.y) / 4 + 2;

        //checks if Weegee goes up a level. doesn't check going down then going back up
        if (level > lastLevel && level > levelCheck)
        {
            levelCheck = level;

            //check if gameButtons setActive == true (isShowing). if the player is playing the game, spawn the next block.
            if (gameButtons.activeInHierarchy)
            {
                //Debug.Log("ACTIVE");
                spawnBlock(0);
            }

            setScore();
            moveCloud();
        }

        //Always check if the player is exceeding 2 jumps
        DoubleJumpCheck();
        //Camera moves Weegee's Y position
        CameraMove();
        //used to spawn the coins during the falling phase
        spawnCoin();

        //Used for keyboard testing (can be removed during android release)
        if (Input.GetButtonDown("JumpKeyboard")) Jump();
        if (Input.GetButtonDown("LeftKeyboard")) MoveLeft();
        if (Input.GetButtonDown("RightKeyboard")) MoveRight();
        //if any button is released, stop weegee
        if (Input.GetButtonUp("LeftKeyboard") || Input.GetButtonUp("RightKeyboard")) Stop();

        //moves the player
        MovePlayer();
        //resets level and game when player dies
        Reset();

    }

    //When the play button in the Main Menu is pressed
    public void Play()
    {
        //Deactivated all unneccesary buttons, activate play buttons
        gameButtons.SetActive(true);
        deathMenu.SetActive(false);
        mainMenu.SetActive(false);
        coinsText.SetActive(true);

        //spawn the first 3 blocks because we are incapable of doing this properly. higher number = lower in y position.
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
                disablePowerUps();
               
            }
        }

    }
    public void disablePowerUps()
    {
        deleteCollectibles("bigJumps");
        deleteCollectibles("doubleScore");
        deleteCollectibles("unlimitedJumps");
        timerText.gameObject.SetActive(false);
        playerJumpPower = 30;         
            jumpLimit = 2;          
            scoreMultiplier = 1;

    }

    //A BigJump powerup is instantiated at given coordinates.
    public void spawnBigJump(float x, float y)
    {
         System.Random random = new System.Random();

        float tempRand =  (int)random.Next(0, 3);
       // Debug.Log(tempRand);
       if(tempRand ==0)
            Instantiate(bigJump, new Vector3(x, y, 0), Quaternion.identity);
      if(tempRand==1)
            Instantiate(unlimitedJump, new Vector3(x, y, 0), Quaternion.identity);
        if (tempRand==2)
            Instantiate(doubleScore, new Vector3(x, y, 0), Quaternion.identity);
    }

    //constantly check if player is giving input, if so, move the player.
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

    //constantly checks (from update) if weegee is able to jump.
    public void DoubleJumpCheck()
    {
        lastY = thisY;
        thisY = gameObject.GetComponent<Rigidbody2D>().velocity.y;

        //If weegee has no y velocity for two frames (last and current frame), reset the jumpcount, so that he may jump again
        if (lastY == 0.0f && thisY == 0.0f)
        {
            jumpCount = 0;
        }

        //If weegee walks off a platform, count it as a first jump (weegee cant walk off a platform and jump twice midair).
        if (thisY < 0 && jumpCount == 0)
        {
            jumpCount = 1;
        }
    }

    //Camera follows Weegee in the Y Axis.
    public void CameraMove()
    {
        //Camera becomes the object in the Unity's hierarchy
        cam = GameObject.Find("Main Camera");

        //Camera doesn't move down if Weegee is below it's Y coordinate. unused, acts like mario's camera in NES super mario (never moves left)
        //if(gameObject.GetComponent<Rigidbody2D>().position.y > cam.GetComponent<Transform>().position.y)

        cam.GetComponent<Transform>().position = new Vector3(cam.GetComponent<Transform>().position.x, gameObject.GetComponent<Rigidbody2D>().position.y, cam.GetComponent<Transform>().position.z);

    }

    //Moves weegee left, flips his sprite given that he is facing right
    public void MoveLeft()
    {
        isMovingLeft = true;
        if (facingRight)
        {
            FlipPlayer();
        }
        
    }

    //similar to moveLeft
    public void MoveRight()
    {
        isMovingRight = true;
        if (!facingRight)
        {
            FlipPlayer();
        }
        
    }

    //Called when direcetional key is released, to stop weegee's position. assumes both directions are never pressed together.
    public void Stop()
    {
        isMovingLeft = false;
        isMovingRight = false;
    }

    //Spawns a block at a specified Y position (NOT coordinate) RELATIVE TO WEEGEE, not the world
    public void spawnBlock(int y)
    {
        //oldBlock points to a dirt texture found within unity
        GameObject oldBlock = GameObject.Find("mc_dirt (14)");
        //Mainblock is used when new blocks are instantiated. Mainblock becomes the parent for the newly instantiated blocks.
        GameObject mainBlock = GameObject.Find("mc_dirt");

        System.Random random = new System.Random();
        float randX;

        do
        {
            //randX, used for the X position of the next block placement, can't be too far away from the previous block (prevent impossible jumps)
            randX = random.Next((int)(lastX - 7), (int)(lastX + 7));

        } while (randX > 8 || randX < -8);

        //create a  random Y number, so that not all blocks are the same distance apart in the Y
        float randY = random.Next(-1, 0);

        //The position for the new block, not actually the vector for the old block
        Vector3 oldBlockVector = new Vector3(randX, level * 4 + randY - y, 0);
        //instantiate another block as a new gameobject (Object original, Vector3 position, Quaternion rotation, Transform parent)
        GameObject block = (GameObject)Instantiate(oldBlock, oldBlockVector, Quaternion.identity, mainBlock.transform);
        //the blocks new name increases so that no 2 blocks have the same name
        block.name = "Block #" + blockNum;

        //every 10 blocks, spawn a bigJump powerup 2 above the block's y
       if (blockNum % 20 == 0)
        {
         spawnBigJump(block.GetComponent<Transform>().position.x, block.GetComponent<Transform>().position.y + 2);
        }

        //Set the sprites of the blocks, changes every 20 score. after 180, will always be the same texture.
        block.GetComponent<SpriteRenderer>().sprite = blockTypes[blockChoice];

        //each time a block is instantiated, increase the number for the next block.
        blockNum++;

        //Destroys the block that was instantiated 4 blocks ago. This means only 4 blocks will ever be instantiated at any time.
        if (level >= 4)
            Destroy(GameObject.Find("Block #" + (level - 3)));

        //lastX becomes the X that was just used, so that the next X can be with respect to this one's.
        lastX = randX;

    }

    //called when space/jump is pressed
    public void Jump()
    {
        if (jumpCount < jumpLimit)
        {
            //play sound
            audio1.clip = jumpSound;
            audio1.Play();

            //sets weegee's Y velocity to something new (in this case, the playerJumpPower). also increase the number of jump's weegee has done (resets when he touches ground)
            GetComponent<Rigidbody2D>().velocity = new Vector2(gameObject.GetComponent<Rigidbody2D>().velocity.x, playerJumpPower);
            jumpCount++;
        }

    }

    //changes view of sprite. called by movement.
    void FlipPlayer()
    {
        facingRight = !facingRight;
        Vector2 localScale = gameObject.transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    //reset, always called by update()
    public void Reset()
    {
        //only execute if above level 2, AND weegee is on the ground (aka below y coord of -2)
        if (levelCheck > 2 && gameObject.GetComponent<Rigidbody2D>().position.y <= -2.0f)
        {
            deathScore.text = "Score: " + (levelCheck - 2);

            //play sound
            audio2.clip = die;
            audio2.Play();

            //destroy all remaining blocks
            levelCheck = blockNum;
            while (levelCheck > 0)
            {
                Destroy(GameObject.Find("Block #" + (levelCheck)));
                levelCheck--;
            }

            //change all variables so that game can be played from the start again
            levelCheck = 0;
            level = 0;
            blockNum = 1;
            score = -1;
            Stop();
            //player now on ground. 
            falling = false;

            //create an area for every game object with certain tag. destroys all these items. in this case destroys all coins (so player doesnt see them when going back up)
            deleteCollectibles("Coin");
         

            //enables menus and makes sure that all powerups are set off. 
            deathMenu.SetActive(true);
            coinsText.SetActive(false);
            gameButtons.SetActive(false);
            deathHigh.text = "Highscore: " + (high);
            playerJumpPower = 30;
            timerText.gameObject.SetActive(false);
        }

    }

    public void deleteCollectibles(String tag)
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject target in objs)
        {
            GameObject.Destroy(target);
        }
    }


    //sets text for score
    public void setScore()
    {
        //score is actually the player's highest level - 2. displays the score by changing the text
        score = score + scoreMultiplier;
        countText.text = (score).ToString();

        //store the new value of player's coins
        PlayerPrefs.SetInt("coins", coins);

        //if a player beats their highscore, set new highscore and store the data in playerprefs
        if (score > high)
        {
            highScore.text = "Highscore: " + (score);
            high = score;
            PlayerPrefs.SetInt("highscore", high);
        }

        //play a sound every 10 levels, aslong as you aren't on level 0
        if ((score) % 10 == 0 && (score) != 0)
        {
            audio2.clip = yahoo;
            audio2.Play();
        }
    }

    //moves cloud above luigi for """"realism""""
    public void moveCloud()
    {
        //Every number of blocks, assuming weegee isnt on the ground, move the cloud to a random pos
        if ((blockNum - 2) % 4 == 0 && (blockNum - 2) != 0)
        {
            System.Random random = new System.Random();
            int randX = random.Next(-8, 8);
            GameObject.Find("cloud").GetComponent<Transform>().position = new Vector3(randX, gameObject.GetComponent<Rigidbody2D>().position.y + 7, 1);
        }
    }

    //Called when returning to the main menu, toggles all other buttons off.
    public void goToMenu()
    {
        gameButtons.SetActive(false);
        deathMenu.SetActive(false);
        mainMenu.SetActive(true);
        shopMenu.SetActive(false);
        coinsText.SetActive(false);
    }

    public void loseMoney(int cost) {
        coins -= cost;
        PlayerPrefs.SetInt("coins", coins);
        totalCoins.text = coins.ToString();
        audio2.clip = saleSound;
        audio2.Play();
    }

    public void changeSkin(Sprite skin) {
        this.skin = skin;
        skinChanged = true;
        Debug.Log("skin");
    }

    //Whenever Weegee enters the trigger for another object, this method is called.
    void OnTriggerEnter2D(Collider2D collision)
    {
        //If the object weegee collides with has a certain tag (similar to an assigned property, given in unity), execute the if statement
        //Collectables are destroyed upon being touched. 
        if (collision.gameObject.CompareTag("bigJumps") || collision.gameObject.CompareTag("unlimitedJumps")
            || collision.gameObject.CompareTag("doubleScore"))
        {
            timerText.gameObject.SetActive(true);
            Destroy(collision.gameObject);
            startTime = Time.realtimeSinceStartup;
            audio2.clip = coinSound;
            audio2.Play();
        }
        if (collision.gameObject.CompareTag("bigJumps"))
        {
            powerUp.GetComponent<Image>().sprite = powerUps[0];
            playerJumpPower = 45;
        }
        if (collision.gameObject.CompareTag("unlimitedJumps"))
        {
            powerUp.GetComponent<Image>().sprite = powerUps[1];
            jumpLimit = 2147483647;
        }
        if (collision.gameObject.CompareTag("doubleScore"))
        {
            powerUp.GetComponent<Image>().sprite = powerUps[2];
            scoreMultiplier = 2;
        }
        if (collision.gameObject.CompareTag("Coin"))
        {
             Destroy(collision.gameObject);
            
            audio2.clip = coinSound;
            audio2.Play();
            coins++;
            totalCoins.text = coins.ToString();
        }
    }
}
