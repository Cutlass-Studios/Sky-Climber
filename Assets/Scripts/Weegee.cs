using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Advertisements;

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

    //--- WORLD GAME OBJECTS ---
    public GameObject cam;
    //Stores text for score and high score which will appear on the User Interface
    public Text countText, highScore, totalCoins, deathScore, deathHigh, timerText, reviveText;
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
    public GameObject Ads;

    //Sounds

    public AudioSource audio1;
    //Responsible for Handling collectable sounds (coins, powerups) and sounds Weegee Makes                           
    public AudioSource audio2;
    public AudioClip die;
    public AudioClip jumpSound;
    public AudioClip coinSound;
    public AudioClip saleSound;
    
    //Textures
    public Sprite[] blockTypes;
    public Sprite[] powerUps;
    public Sprite muted, soundOn, shrink, expand;
    public Sprite skin;

    //--- GAME LOGIC ---
    public int level = 0;
    public int blockNum = 1;
    public bool falling;
    public int score = -2;
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
    //1 means unlocked
    public int[] unlockables;
    public int[] prices;

    //retarded variable cause i dont know why spawnblock acts differently when you press play again rather than play
    public bool firstTime;
    public int consecutiveRevives = 1;
    private bool skinChanged = false;
    private int toyStoryBlock = 0;
    private int expandedControls = 0;
    int tempNumber;
    private float newX;
    private float rnewX;
    private ColorBlock offColor;
    private GameObject currentHover;
    private int count = 0;



    //Method that is called when game begins
    private void Start()
    {
     
        Advertisement.Initialize("1490479");
        //PlayerPrefs.DeleteAll();
        //set store

        //
        
        blockChoice = PlayerPrefs.GetInt("BlockNumber", 0);

        //set sound
        if (PlayerPrefs.GetInt("Mute", 0) == 1)
            toggleSound();

        firstTime = true;
        //Force the Screen into landscape
        // Screen.orientation = ScreenOrientation.Landscape;

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
        expandedControls = PlayerPrefs.GetInt("controls", expandedControls);
        highScore.text = "Highscore: " + high;
        totalCoins.text = "" + coins;
        

        //Set the Score (will be zero because the game just started)
        setScore();
        falling = false; //NOTE: be set when instantiated

        //Debug.Log(blockChoice);

    }

    public void setFirstTime(bool b)
    {
        firstTime = b;
    }

    public void changeControls()
    {
        if (expandedControls == 0) PlayerPrefs.SetInt("controls", 1);
        else PlayerPrefs.SetInt("controls", 0);
    }

    public void playSound(AudioClip soundEffect) {

        count++;

        if (count == 69)
        {
            audio1.clip = soundEffect;
            audio1.Play();
        }
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
        GameObject.Find("BlockPage1").GetComponent<RectTransform>().localPosition = new Vector3(1, 0, 0);
        GameObject.Find("BlockPage2").GetComponent<RectTransform>().localPosition = new Vector3(808, 0, 0);
        Swipe.setZero();

        for (int i = 1; i < 14; i++)
        {
            if (PlayerPrefs.GetInt("Block" + i, 0) == 1)
            {
                    unlockBlockStart(i, 1);
            }
        }


        //page 2
        for (int i = 1; i < 14; i++)
        {
            if (PlayerPrefs.GetInt("Block2" + i, 0) == 1)
            {
            
               
                
                    unlockBlockStart(i, 2);
                

            }
        }

        //Vector3 newVector = new Vector3(GameObject.Find("Buy 1 (" + blockChoice + ")").GetComponent<Transform>().position.x, GameObject.Find("Buy 1 (" + blockChoice + ")").GetComponent<Transform>().position.y, GameObject.Find("Buy 1 (" + blockChoice + ")").GetComponent<Transform>().position.z);
        //GameObject.Find("GreenBorder").GetComponent<Transform>().position = newVector;
        purchaseBlock(blockChoice);

    }
    public void toggleSound()
    {
        //if sound is on, turn it off and vice versa
        if (sound)
        {
            mainMenu.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = muted;
            PlayerPrefs.SetInt("Mute", 1);
        }
        else
        {
            mainMenu.transform.GetChild(0).gameObject.GetComponent<Image>().sprite = soundOn;
            PlayerPrefs.SetInt("Mute", 0);
        }
        sound = !sound;
        audio1.mute = !audio1.mute;
        audio2.mute = !audio2.mute;
    }

    public bool IsFingerInBlock(int blockChoice) {
        Vector3 newVector;
        if (blockChoice <= 13)
        {
            newVector = new Vector3(GameObject.Find("Buy 1 (" + blockChoice + ")").GetComponent<Transform>().position.x, GameObject.Find("Buy 1 (" + blockChoice + ")").GetComponent<Transform>().position.y, GameObject.Find("Buy 1 (" + blockChoice + ")").GetComponent<Transform>().position.z);
        }
        else
        {
            newVector = new Vector3(GameObject.Find("Buy 2 (" + (blockChoice - 16) + ")").GetComponent<Transform>().position.x, GameObject.Find("Buy 2 (" + (blockChoice - 16) + ")").GetComponent<Transform>().position.y, GameObject.Find("Buy 2 (" + (blockChoice - 16) + ")").GetComponent<Transform>().position.z);
        }
        Vector3 touchPos;
        if (Input.touches.Length != 0)
        {
            touchPos = Input.touches[0].position;
        }
        else {
            touchPos = Input.mousePosition;
        }
        
        if (touchPos.x < newVector.x + 37 && touchPos.x > newVector.x - 37 
            && touchPos.y < newVector.y +37 && touchPos.y > newVector.y - 37)
        {
            //Debug.Log("true");
            return true;
        }
        
        //Debug.Log("FALSE");
        return false;
    }

    private void moveSelector()
    {

        
        if (shopMenu.activeInHierarchy)
        {
            
            if (blockChoice <= 13)
            {
                Vector3 newVector = new Vector3(GameObject.Find("Buy 1 (" + blockChoice + ")").GetComponent<Transform>().position.x, GameObject.Find("Buy 1 (" + blockChoice + ")").GetComponent<Transform>().position.y, GameObject.Find("Buy 1 (" + blockChoice + ")").GetComponent<Transform>().position.z);
                GameObject.Find("GreenBorder").GetComponent<Transform>().position = newVector;
            }
            else
            {
                Vector3 newVector = new Vector3(GameObject.Find("Buy 2 (" + (blockChoice - 16) + ")").GetComponent<Transform>().position.x, GameObject.Find("Buy 2 (" + (blockChoice - 16) + ")").GetComponent<Transform>().position.y, GameObject.Find("Buy 2 (" + (blockChoice - 16) + ")").GetComponent<Transform>().position.z);
                GameObject.Find("GreenBorder").GetComponent<Transform>().position = newVector;
            }
        }
        
    }


    // Update is called once per frame (60fps)
    public void Update()
    {

        moveSelector();
        
        expandedControls = PlayerPrefs.GetInt("controls", expandedControls);
        
        if (gameButtons.activeInHierarchy)
        {
            /*
            if (expandedControls == 0)
            {
                GameObject.Find("Left").GetComponent<RectTransform>().localPosition = new Vector3(-326, GameObject.Find("Left").GetComponent<RectTransform>().localPosition.y, GameObject.Find("Left").GetComponent<RectTransform>().localPosition.z);
                GameObject.Find("Right").GetComponent<RectTransform>().localPosition = new Vector3(-237, GameObject.Find("Right").GetComponent<RectTransform>().localPosition.y, GameObject.Find("Right").GetComponent<RectTransform>().localPosition.z);
                GameObject.Find("ControlsButton").GetComponent<Image>().sprite = expand;
            }
            else
            {
                GameObject.Find("Left").GetComponent<RectTransform>().localPosition = new Vector3(-346, GameObject.Find("Left").GetComponent<RectTransform>().localPosition.y, GameObject.Find("Left").GetComponent<RectTransform>().localPosition.z);
                GameObject.Find("Right").GetComponent<RectTransform>().localPosition = new Vector3(-217, GameObject.Find("Right").GetComponent<RectTransform>().localPosition.y, GameObject.Find("Right").GetComponent<RectTransform>().localPosition.z);
                GameObject.Find("ControlsButton").GetComponent<Image>().sprite = shrink;
            }
            
            ColorBlock rightButton = GameObject.Find("Right").GetComponent<Button>().colors;
            ColorBlock leftButton = GameObject.Find("Left").GetComponent<Button>().colors;
            if (isMovingRight || isMovingLeft)
            {
                if (isMovingRight)
                {
                    rightButton.normalColor = new Color(231, 0, 0, 0.4f);
                    rightButton.highlightedColor = new Color(231, 0, 0, 0.4f);
                    leftButton.normalColor = new Color(255, 255, 255, 0.4f);
                    leftButton.pressedColor = new Color(255, 255, 255, 0.4f);
                    leftButton.highlightedColor = new Color(255, 255, 255, 0.4f);
                }
                else
                {
                    leftButton.normalColor = new Color(231, 0, 0, 0.4f);
                    leftButton.highlightedColor = new Color(231, 0, 0, 0.4f);
                    rightButton.normalColor = new Color(255, 255, 255, 0.4f);
                    rightButton.pressedColor = new Color(255, 255, 255, 0.4f);
                    rightButton.highlightedColor = new Color(255, 255, 255, 0.4f);
                }
                
            }
            else
            {
                rightButton.normalColor = new Color(255, 255, 255, 0.4f);
                rightButton.pressedColor = new Color(255, 255, 255, 0.4f);
                rightButton.highlightedColor = new Color(255, 255, 255, 0.4f);
                leftButton.normalColor = new Color(255, 255, 255, 0.4f);
                leftButton.pressedColor = new Color(255, 255, 255, 0.4f);
                leftButton.highlightedColor = new Color(255, 255, 255, 0.4f);
            }
            GameObject.Find("Left").GetComponent<Button>().colors = leftButton;
            GameObject.Find("Right").GetComponent<Button>().colors = rightButton;
            */
        }



        if (jumpCount > 0)
        {
         
                anim.Play("jump1");

        }
        else if (isMovingLeft || isMovingRight)
        {
            anim.Play("walk1");
        }
        else
        {
            if (skinChanged) anim.Play("idleNib");
            else anim.Play("idle1");
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
        if (playerJumpPower > 30)
            timerText.text = ((int)(startTime + 6 - Time.realtimeSinceStartup)).ToString();
        if (jumpLimit > 2)
            timerText.text = ((int)(startTime + 5 - Time.realtimeSinceStartup)).ToString();

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
        consecutiveRevives = 1;
        //Deactivated all unneccesary buttons, activate play buttons
        gameButtons.SetActive(true);
        deathMenu.SetActive(false);
        mainMenu.SetActive(false);
        coinsText.SetActive(true);
        offColor = GameObject.Find("Left").GetComponent<Button>().colors;
        //spawn the first 3 blocks because we are incapable of doing this properly. higher number = lower in y position.
        //SPAGHETTi
        if (firstTime)
        {
            spawnBlock(8);
            spawnBlock(4);
            spawnBlock(0);
        }
        else
        {
            //Debug.Log("else");
            spawnBlock(0);
            spawnBlock(-4);
            //spawnBlock(-8);
        }

    }

    public void spawnCoin()
    {

        //Compares Weegee's position to lowest block (off screen)
        if (GameObject.Find("Block #" + (blockNum - 4)) != null)
        {
            float blockY = GameObject.Find("Block #" + (blockNum - 4)).GetComponent<Transform>().position.y;
            if (gameObject.GetComponent<Rigidbody2D>().position.y + 1 < blockY && !falling && levelCheck > 10 && consecutiveRevives == 1)
            {
                falling = true;
                if (consecutiveRevives == 1)
                {


                    for (int i = (int)blockY; i > 0; i--)
                    {
                        Instantiate(coin, new Vector3((float)Math.Sin((double)i / 20.0) * 7, i, 0), Quaternion.identity);
                    }
                    disablePowerUps();

                }
            }
            if (gameObject.GetComponent<Rigidbody2D>().position.y + 4 < blockY && !falling && levelCheck > 10 && consecutiveRevives > 1)
            {
                falling = true;
                disablePowerUps();
                Vector3 newVector = new Vector3(gameObject.GetComponent<Transform>().position.x, GameObject.Find("mc_dirt (6)EPIC").GetComponent<Transform>().position.y + 8, gameObject.GetComponent<Transform>().position.z);
                gameObject.GetComponent<Transform>().position = newVector;
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

        float tempRand = (int)random.Next(0, 3);
        // Debug.Log(tempRand);
        if (tempRand == 0)
            Instantiate(bigJump, new Vector3(x, y, 0), Quaternion.identity);
        if (tempRand == 1)
            Instantiate(unlimitedJump, new Vector3(x, y, 0), Quaternion.identity);
        if (tempRand == 2)
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
        if (gameButtons.activeInHierarchy)
        {
            ColorBlock cb = GameObject.Find("Left").GetComponent<Button>().colors;
            cb.normalColor = new Color(231, 0, 0, 0.4f);
            cb.highlightedColor = new Color(231, 0, 0, 0.4f);
            cb.pressedColor = new Color(231, 0, 0, 0.4f);
            GameObject.Find("Left").GetComponent<Button>().colors = cb;
            cb.normalColor = new Color(255, 255, 255, 0.4f);
            cb.highlightedColor = new Color(255, 255, 255, 0.4f);
            cb.pressedColor = new Color(255, 255, 255, 0.4f);
            GameObject.Find("Right").GetComponent<Button>().colors = cb;
        }
            
        isMovingLeft = true;
        isMovingRight = false;
        if (!facingRight)
        {
            FlipPlayer();
        }

    }

    //similar to moveLeft
    public void MoveRight()
    {
        if (gameButtons.activeInHierarchy)
        {
            ColorBlock cb = GameObject.Find("Right").GetComponent<Button>().colors;
            cb.normalColor = new Color(231, 0, 0, 0.4f);
            cb.highlightedColor = new Color(231, 0, 0, 0.4f);
            cb.pressedColor = new Color(231, 0, 0, 0.4f);
            GameObject.Find("Right").GetComponent<Button>().colors = cb;
            cb.normalColor = new Color(255, 255, 255, 0.4f);
            cb.highlightedColor = new Color(255, 255, 255, 0.4f);
            cb.pressedColor = new Color(255, 255, 255, 0.4f);
            GameObject.Find("Left").GetComponent<Button>().colors = cb;
        }
        
        isMovingLeft = false;
        isMovingRight = true;
        if (facingRight)
        {
            FlipPlayer();
        }

    }

    //Called when direcetional key is released, to stop weegee's position. assumes both directions are never pressed together.
    public void Stop()
    {
        
        isMovingRight = false;
        isMovingLeft = false;
    }
    public void Stopright()
    {
        ColorBlock cb = offColor;

        cb.normalColor = new Color(255, 255, 255, 0.4f);
        cb.highlightedColor = new Color(255, 255, 255, 0.4f);
        cb.pressedColor = new Color(255, 255, 255, 0.4f);

        GameObject.Find("Left").GetComponent<Button>().colors = cb;
        GameObject.Find("Right").GetComponent<Button>().colors = cb;
        isMovingRight = false;
    }
    public void Stopleft()
    {
        ColorBlock cb = offColor;

        cb.normalColor = new Color(255, 255, 255, 0.4f);
        cb.highlightedColor = new Color(255, 255, 255, 0.4f);
        cb.pressedColor = new Color(255, 255, 255, 0.4f);

        GameObject.Find("Left").GetComponent<Button>().colors = cb;
        GameObject.Find("Right").GetComponent<Button>().colors = cb;
        isMovingLeft = false;

    }

    //Spawns a block at a specified Y position (NOT coordinate) RELATIVE TO WEEGEE, not the world
    public void spawnBlock(int y)
    {
        toyStoryBlock++;
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

        if (blockChoice == 11)
        {
            if (toyStoryBlock == 1)
                
            {
            
                block.GetComponent<SpriteRenderer>().sprite = blockTypes[11];
            }
            if (toyStoryBlock == 2)
                
            {
              
                block.GetComponent<SpriteRenderer>().sprite = blockTypes[14];
            }
            if (toyStoryBlock == 3)
            {
                
                block.GetComponent<SpriteRenderer>().sprite = blockTypes[15];
                toyStoryBlock = 0;
            }
        }
        else
        {
            block.GetComponent<SpriteRenderer>().sprite = blockTypes[blockChoice];
        }


        //Set the sprites of the blocks, changes every 20 score. after 180, will always be the same texture.

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
            Ads.SetActive(true);

            toyStoryBlock = 0;
            deathScore.text = "Score: " + score;

            //play sound

            audio2.clip = die;
            audio2.Play();


            tempNumber = levelCheck;
            levelCheck = 0;
            Stop();
            //player now on ground. 
            falling = false;

            reviveText.text = "Cost: $" + ((100 + blockNum) * consecutiveRevives).ToString();

            //create an area for every game object with certain tag. destroys all these items. in this case destroys all coins (so player doesnt see them when going back up)
            deleteCollectibles("Coin");


            //enables menus and makes sure that all powerups are set off. 
            deathMenu.SetActive(true);
            if(!Advertisement.IsReady("rewardedVideo") || score <5)
           Ads.SetActive(false);
            //coinsText.SetActive(true);
            gameButtons.SetActive(false);
            deathHigh.text = "Highscore: " + (high);
            playerJumpPower = 30;
            timerText.gameObject.SetActive(false);
        }

    }

    public void Revive()
    {
        int cost = (100 + blockNum) * consecutiveRevives;
        if (coins >= cost)
        {
            loseMoney(cost);
            levelCheck = tempNumber;
            gameButtons.SetActive(true);
            deathMenu.SetActive(false);
            mainMenu.SetActive(false);
            coinsText.SetActive(true);
            GameObject reviveBlock = GameObject.Find("Block #" + (blockNum - 4));
            gameObject.GetComponent<Transform>().position = new Vector3(reviveBlock.GetComponent<Transform>().position.x, reviveBlock.GetComponent<Transform>().position.y + 2, gameObject.GetComponent<Transform>().position.z);
            consecutiveRevives++;
        }
    }

    public void ResetGame()
    {
        firstTime = false;

        //destroy all remaining blocks
        //change all variables so that game can be played from the start again
        levelCheck = blockNum;
        while (levelCheck > 0)
        {
            Destroy(GameObject.Find("Block #" + (levelCheck)));
            levelCheck--;
        }

        levelCheck = 0;
        level = 0;
        blockNum = 1;
        score = -1;
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

    public void loseMoney(int cost)
    {


        if (coins - cost >= 0)
        {
            coins -= cost;
            PlayerPrefs.SetInt("coins", coins);
            totalCoins.text = coins.ToString();
            audio2.clip = saleSound;
            audio2.Play();

        }
    }

    public void unlockBlock(int blockNumber)
    {
        if (coins - prices[blockNumber] >= 0 && PlayerPrefs.GetInt("Block" + blockNumber, 0) == 0)
        {
            for (int i = 1; i < 14; i++)
            {
                GameObject.Find("Buy 1 (" + i + ")").GetComponent<Outline>().effectColor = Color.black;
            }
            coins -= prices[blockNumber];
            PlayerPrefs.SetInt("coins", coins);
            totalCoins.text = coins.ToString();
            audio2.clip = saleSound;
            audio2.Play();
            GameObject.Find("Buy 1 (" + blockNumber + ")").GetComponent<Image>().color = Color.white;
            GameObject.Find("Buy 1 (" + blockNumber + ")").GetComponent<Outline>().effectColor = Color.green;
            GameObject.Find("QuestionMark (" + blockNumber + ")").GetComponent<Text>().text = "";
            PlayerPrefs.SetInt("Block" + blockNumber, 1);
            purchaseBlock(blockNumber);
        }
        if (PlayerPrefs.GetInt("Block" + blockNumber, 0) == 1)
        {
            blockChoice = blockNumber;
            PlayerPrefs.SetInt("BlockNumber", blockChoice);
        }

    }
    public void unlockBlock2(int blockNumber)
    {
        if (coins - prices[blockNumber + 16] >= 0 && PlayerPrefs.GetInt("Block2" + blockNumber, 0) == 0)
        {
            //Debug.Log("BOUGHT");
            for (int i = 0; i < 14; i++)
            {
                GameObject.Find("Buy 2 (" + blockNumber + ")").GetComponent<Outline>().effectColor = Color.black;
            }
            coins -= prices[blockNumber + 16];
            PlayerPrefs.SetInt("coins", coins);
            totalCoins.text = coins.ToString();
            audio2.clip = saleSound;
            audio2.Play();
            GameObject.Find("Buy 2 (" + blockNumber + ")").GetComponent<Image>().color = Color.white;
            GameObject.Find("Buy 2 (" + blockNumber + ")").GetComponent<Outline>().effectColor = Color.green;
            GameObject.Find("QuestionMark2 (" + blockNumber + ")").GetComponent<Text>().text = "";
            PlayerPrefs.SetInt("Block2" + blockNumber, 1);
            purchaseBlock(blockNumber+16);
        }
        if (PlayerPrefs.GetInt("Block2" + blockNumber, 0) == 1)
        {
            blockChoice = blockNumber+16;
            PlayerPrefs.SetInt("BlockNumber", blockChoice);
        }

    }

    public void PerformFingerCheck(int blockNumber) {
        if (IsFingerInBlock(blockNumber))
        {
            purchaseBlock(blockNumber);
            unlockBlock(blockNumber);
        }
    }

    public void PerformFingerCheck2(int blockNumber)
    {
        if (IsFingerInBlock(blockNumber))
        {
            purchaseBlock(blockNumber);
            unlockBlock2(blockNumber - 16);
        }
    }

    public void purchaseBlock(int blockNumber)
    {
        
            //GameObject.Find("BlockPage1").GetComponent<Swipe>().pageNum
            if (PlayerPrefs.GetInt("Block" + blockNumber, 0) == 1 || blockNumber == 0)
            {
                blockChoice = blockNumber;
                PlayerPrefs.SetInt("BlockNumber", blockChoice);
                Vector3 newVector;
                if (blockChoice <= 13)
                {
                    //Debug.Log("Page 1");
                    newVector = new Vector3(GameObject.Find("Buy 1 (" + blockNumber + ")").GetComponent<Transform>().position.x, GameObject.Find("Buy 1 (" + blockNumber + ")").GetComponent<Transform>().position.y, GameObject.Find("Buy 1 (" + blockNumber + ")").GetComponent<Transform>().position.z);
                    GameObject.Find("GreenBorder").GetComponent<Transform>().position = newVector;
                    //Debug.Log("MOVING GREEN");
                }
            }

            else if (PlayerPrefs.GetInt("Block2" + (blockNumber - 16), 0) == 1)
            {
                if (blockNumber > 14)
                {
                    //Debug.Log("Page 2");
                    Vector3 newVector1;
                    int nibChoice = blockNumber - 16;

                    newVector1 = new Vector3(GameObject.Find("Buy 2 (" + nibChoice + ")").GetComponent<Transform>().position.x, GameObject.Find("Buy 2 (" + nibChoice + ")").GetComponent<Transform>().position.y, GameObject.Find("Buy 2 (" + nibChoice + ")").GetComponent<Transform>().position.z);
                    GameObject.Find("GreenBorder").GetComponent<Transform>().position = newVector1;
                }


            }
        
    }
 
    //unlocks without losing money
    public void unlockBlockStart(int blockNumber, int blockPage)
    {
        if (blockPage == 1)
        {
            GameObject.Find("Buy 1 (" + blockNumber + ")").GetComponent<Image>().color = Color.white;
            GameObject.Find("QuestionMark (" + blockNumber + ")").GetComponent<Text>().text = "";
        }
        else if (blockPage == 2)
        {
            int blockNumber2 = blockNumber - 14;
            GameObject.Find("Buy 2 (" + blockNumber + ")").GetComponent<Image>().color = Color.white;
            GameObject.Find("QuestionMark2 (" + blockNumber + ")").GetComponent<Text>().text = "";
        }
        

    }

    public void changeSkin(Sprite skin)
    {
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

    public void ShowRewardedAd()
    {

        if (Advertisement.IsReady("rewardedVideo"))
        {

            var options = new ShowOptions { resultCallback = HandleShowResult };
            Advertisement.Show("rewardedVideo", options);
        }
        Debug.Log(Advertisement.IsReady("rewardedVideo"));

    }

    private void HandleShowResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                // Debug.Log("The ad was successfully shown.");4
                Ads.SetActive(false);
                coins = coins + 150;
                PlayerPrefs.SetInt("coins", coins);
                totalCoins.text = "" + coins;
                //Debug.Log("The ad was successfully shown.");

                break;
            case ShowResult.Skipped:
                //Debug.Log("The ad was skipped before reaching the end.");
                break;
            case ShowResult.Failed:
              //  Debug.LogError("The ad failed to be shown.");
                break;
        }
    }


}
