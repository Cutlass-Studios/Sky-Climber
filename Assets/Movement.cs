using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    public float moveX;
    public bool facingRight = true;
    public int playerSpeed = 10;
    public int playerJumpPower = 2250;
    private bool isMovingLeft = false;
    private bool isMovingRight = false;

	
	// Update is called once per frame
	void Update () {
        //Movement is the only thing requiring updates
        if (isMovingRight)
        {
            gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(playerSpeed, gameObject.GetComponent<Rigidbody2D>().velocity.y);
        }
        if (isMovingLeft) {
            gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(-playerSpeed, gameObject.GetComponent<Rigidbody2D>().velocity.y);
        }
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

    public void Jump()
    {

        GetComponent<Rigidbody2D>().AddForce(Vector2.up * playerJumpPower);
    }

    void FlipPlayer()
    {
        facingRight = !facingRight;
        Vector2 localScale = gameObject.transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }
}
