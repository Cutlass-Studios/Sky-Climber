using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Swipe : MonoBehaviour
{

    public bool swipeLeft, swipeRight, swipeUp, swipeDown;
    public static bool isDragging;


    private int dragRadius = 150;
    public Vector2 startTouch, swipeDelta;
    public GameObject obj;
    //public int swipeDistance;
    public static int pageNum = 0;
    public float magicNumber = 4000f;

    public static void setZero() {
        pageNum = 0;
    }

    //public Vector2 SwipeDelta { get { return swipeDelta; } }
    public void Start()
    {
        GameObject.Find("Circle 1").GetComponent<Image>().color = Color.green;
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

            /*
            //MENU FOLLOWS TOUCH
            if (Input.touches.Length != 0)
            {
                if (pageNum == 0)
                {
                    obj.GetComponent<RectTransform>().localPosition = new Vector3(Input.touches[0].position.x - startTouch.x, obj.GetComponent<RectTransform>().localPosition.y);
                
                }
                else
                {
                    obj.GetComponent<RectTransform>().localPosition = new Vector3(Input.touches[0].position.x - startTouch.x - 807, obj.GetComponent<RectTransform>().localPosition.y);
                 

                }
            }
            //MENU FOLLOWS CURSOR
            else
            {
                if (pageNum == 0)
                {
                    obj.GetComponent<RectTransform>().localPosition = new Vector3(Input.mousePosition.x - startTouch.x, obj.GetComponent<RectTransform>().localPosition.y);
                }
                else
                {
                    obj.GetComponent<RectTransform>().localPosition = new Vector3(Input.mousePosition.x - startTouch.x - 807, obj.GetComponent<RectTransform>().localPosition.y);
                }
            }
            */

        }
        //RESETTING POSITION
        else if (pageNum == 0)
        {
            Vector2 currentPos = obj.GetComponent<RectTransform>().localPosition;
            Vector2 destination = new Vector2(0, 0);
            obj.GetComponent<RectTransform>().localPosition = Vector3.MoveTowards(currentPos, destination, magicNumber/2 * Time.deltaTime);
        }
        else {
            Vector2 currentPos = obj.GetComponent<RectTransform>().localPosition;
            Vector2 destination = new Vector2(-807, 0);
            obj.GetComponent<RectTransform>().localPosition = Vector3.MoveTowards(currentPos, destination, magicNumber/2 * Time.deltaTime);
        }

        //if player has swiped further than given radius (cross deadzone)
        if (Mathf.Abs(swipeDelta.x) > dragRadius)
        {
            float x = swipeDelta.x;
            float y = swipeDelta.y;
            //left right


            if (x > 0 && pageNum != 0)
            { //right

                moveRight();
            }

            else if (x < 0 && pageNum != 1)
            {

                moveLeft();
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
        //obj.GetComponent<RectTransform>().localPosition = new Vector3(0, 0);

        Vector2 currentPos = obj.GetComponent<RectTransform>().localPosition;
        Vector2 destination = new Vector2(0, 0);
        obj.GetComponent<RectTransform>().localPosition = Vector3.MoveTowards(currentPos, destination, magicNumber * 1* Time.deltaTime);

        //obj.GetComponent<RectTransform>().localPosition =
        //    new Vector3(obj.GetComponent<RectTransform>().localPosition.x + swipeDistance, obj.GetComponent<RectTransform>().localPosition.y);
        pageNum--;
        GameObject.Find("Circle 4").GetComponent<Image>().color = Color.white;
        GameObject.Find("Circle 1").GetComponent<Image>().color = Color.green;
    }

    private void moveLeft()
    {
        swipeLeft = true;
        //obj.GetComponent<RectTransform>().localPosition = new Vector3(-807, 0);

        Vector2 currentPos = obj.GetComponent<RectTransform>().localPosition;
        Vector2 destination = new Vector2(-807, 0);
        obj.GetComponent<RectTransform>().localPosition = Vector3.MoveTowards(currentPos, destination, magicNumber * 1 * Time.deltaTime);

        //obj.GetComponent<RectTransform>().localPosition =
        //new Vector3(obj.GetComponent<RectTransform>().localPosition.x - swipeDistance, obj.GetComponent<RectTransform>().localPosition.y);
        GameObject.Find("Circle 4").GetComponent<Image>().color = Color.green;
        GameObject.Find("Circle 1").GetComponent<Image>().color = Color.white;
        pageNum++;
    }

    private void Reset()
    {
        isDragging = swipeLeft = swipeRight = swipeUp = swipeDown = false;
        startTouch =  Vector2.zero;

    }
}
