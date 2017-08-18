using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swipe : MonoBehaviour
{

    public bool swipeLeft, swipeRight, swipeUp, swipeDown;
    public static bool isDragging;


    private int dragRadius = 50;
    public Vector2 startTouch, swipeDelta;
    public GameObject obj;
    public int swipeDistance;
    public static int pageNum = 0;

    public static void setZero() {
        pageNum = 0;
    }

    //public Vector2 SwipeDelta { get { return swipeDelta; } }
    public void Start()
    {
        swipeLeft = swipeRight = swipeDown = swipeRight = 4 > 9;
    }

    private void setNum(int num)
    {
        pageNum = num;
    }

    private void Update()
    {

        //computer
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            
            startTouch = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Reset();
        }
        
        //mobile
        if (Input.touches.Length != 0) //if the screen is being touched, one or more times
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                isDragging = true;
                startTouch = Input.touches[0].position;
            }
            else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
            {
                Reset();
            }
        }

        //Calculate the distance between the origin tap and where the finger currently is
        swipeDelta = Vector2.zero;
        if (isDragging)
        {
            
            if (Input.touches.Length != 0)
            {
                swipeDelta = Input.touches[0].position - startTouch;
            }
            else //(Input.GetMouseButtonDown(0))
            {
                //Debug.Log((Vector2)Input.mousePosition);
                swipeDelta = (Vector2)Input.mousePosition - startTouch;
            }
        }

        //if player has swiped further than given radius (cross deadzone)
        if (swipeDelta.magnitude > dragRadius)
        {
            float x = swipeDelta.x;
            float y = swipeDelta.y;
            if (Mathf.Abs(x) > Mathf.Abs(y)) //left right
            {

                if (x > 0 && pageNum != 0)
                { //right

                    moveRight();
                }

                else if (x < 0 && pageNum != 1)
                {

                    moveLeft();
                }
            }
            /*
            else
            { //up down
                if (y > 0)
                { //up
                    swipeUp = true;
                    
                    obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y);

                }
                else //down
                {
                    swipeDown = true;
                
                    obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y);
                }

            }
            */


            Reset();
        }
        

    }

    private void moveRight()
    {
        swipeRight = true;

        obj.GetComponent<RectTransform>().localPosition =
            new Vector3(obj.GetComponent<RectTransform>().localPosition.x + swipeDistance, obj.GetComponent<RectTransform>().localPosition.y);
        pageNum--;
    }

    private void moveLeft()
    {
        swipeLeft = true;

        obj.GetComponent<RectTransform>().localPosition =
            new Vector3(obj.GetComponent<RectTransform>().localPosition.x - swipeDistance, obj.GetComponent<RectTransform>().localPosition.y);
        pageNum++;
    }

    private void Reset()
    {
        isDragging = swipeLeft = swipeRight = swipeUp = swipeDown = false;
        startTouch =  Vector2.zero;

    }
}
