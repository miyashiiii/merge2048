using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public GameObject debugTextBoxUp1; // Textオブジェクト
    public GameObject debugTextBoxUp2; // Textオブジェクト
    public GameObject debugTextBoxUp3; // Textオブジェクト
    public GameObject debugTextBoxUp4; // Textオブジェクト
    public GameObject debugTextBoxDown; // Textオブジェクト

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
        Debug.Log("board status: " + Board.Status);
        if (Board.Status == Board.StatusInMovingAnimation)
        {
            Board.ContinueMovingAnimation();
            return;
        }

        if (Board.Status == Board.StatusInCreateAnimation)
        {
            Board.ContinueCreatingAnimation();
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

        //1: board
        //2: move
        //3: isNew
        //4: instance
        string boardStr = "";
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                try
                {
                    boardStr = boardStr + Board._board[i][j];
                }
                catch (NullReferenceException e)
                {
                
                    Util.JagListDebugLog("####### ERROR ######## i: " + i + ", j: " + j + ", board", Board._board);
                }
            }

            boardStr = boardStr + "\n";
        }

        debugTextBoxUp1.GetComponent<Text>().text = boardStr;

        string moveBOardStr = "";
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                moveBOardStr = moveBOardStr + Board.moveBoard[i][j];
            }

            moveBOardStr = moveBOardStr + "\n";
        }

        debugTextBoxUp2.GetComponent<Text>().text = moveBOardStr;
        // Board.Update("left"); 
        string isNewBoardStr = "";
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                isNewBoardStr = isNewBoardStr + Board.IsNewBoard[i][j];
            }

            isNewBoardStr = isNewBoardStr + "\n";
        }

        debugTextBoxUp3.GetComponent<Text>().text = isNewBoardStr;
        string instancesStr = "";
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (Board._instances[i][j] == null)
                {
                    instancesStr = instancesStr + 0;
                }
                else
                {
                    instancesStr = instancesStr + Board._instances[i][j].name[0];
                }
            }

            instancesStr = instancesStr + "\n";
        }

        debugTextBoxUp4.GetComponent<Text>().text = instancesStr;
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
        debugTextBoxDown.GetComponent<Text>().text = direction;
        Board.Update(direction);
    }
}