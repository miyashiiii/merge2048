using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public GameObject debugTextBox; // Textオブジェクト

    private Vector3 _touchStartPos;
    private Vector3 _touchEndPos;

    // Start is called before the first frame update
    private void Start()
    {
        var glGroup = gameObject.GetComponent<GridLayoutGroup>();
        var parentPos = gameObject.transform.position;
        Board.Init(parentPos, glGroup.cellSize, glGroup.spacing);
    }


    // Update is called once per frame
    private void Update()
    {
        Debug.Log("board status: "+Board.Status);
        if (Board.Status == Board.StatusInAnimation)
        {
            Board.ContinueAnimation();
            return;
        }

        if (Board.Status == Board.StatusFinish)
        {
            CheckKeyDown();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            _touchStartPos = new Vector3(Input.mousePosition.x,
                Input.mousePosition.y,
                Input.mousePosition.z);
        }

        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            _touchEndPos = new Vector3(Input.mousePosition.x,
                Input.mousePosition.y,
                Input.mousePosition.z);
            GetDirection();
        }
    }

    void CheckKeyDown()
    {
    }


    void GetDirection()
    {
        var directionX = _touchEndPos.x - _touchStartPos.x;
        var directionY = _touchEndPos.y - _touchStartPos.y;
        var direction = "";

        if (Mathf.Abs(directionY) < Mathf.Abs(directionX))
        {
            if (30 < directionX)
            {
                direction = "right";
            }
            else if (-30 > directionX)
            {
                direction = "left";
            }
        }
        else if (Mathf.Abs(directionX) < Mathf.Abs(directionY))
        {
            if (30 < directionY)
            {
                direction = "up";
            }
            else if (-30 > directionY)
            {
                direction = "down";
            }
        }
        else
        {
            direction = "touch";
        }

        // Debug.Log(direction);
        debugTextBox.GetComponent<Text>().text = direction;
        Board.Update(direction);
        // Board.Update("left");
    }
}