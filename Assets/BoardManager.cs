﻿using System;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public GameObject debugTextBoxUp1; // Textオブジェクト
    public GameObject debugTextBoxUp2; // Textオブジェクト
    public GameObject debugTextBoxUp3; // Textオブジェクト
    public GameObject debugTextBoxUp4; // Textオブジェクト
    public GameObject debugTextBoxDown; // Textオブジェクト
    public GameObject debugTextBoxDown2; // Textオブジェクト
    public bool debug;

    public GameObject[] objects; // Textオブジェクト

    private Vector3 _touchStartPos;
    private Vector3 _touchEndPos;


    // Start is called before the first frame update
    private void Start()
    {
        var glGroup = GetComponent<GridLayoutGroup>();
        var parentPos = transform.position;
        Board.Init(parentPos, glGroup.cellSize, glGroup.spacing);
        if (debug)
        {
            foreach (var obj in objects)
            {
                obj.SetActive(false);
            }
        }
        else
        {
            debugTextBoxUp1.SetActive(false);
            debugTextBoxUp2.SetActive(false);
            debugTextBoxUp3.SetActive(false);
            debugTextBoxUp4.SetActive(false);
            debugTextBoxDown.SetActive(false);
            debugTextBoxDown2.SetActive(false);
        }
    }


    // Update is called once per frame
    private void Update()
    {
        UnityEngine.Debug.Log("board status: " + Board.Status);
        switch (Board.Status)
        {
            case Board.StatusInMovingAnimation:
                Board.ContinueMovingAnimation();
                return;
            case Board.StatusInCreateAnimation:
                Board.ContinueCreatingAnimation();
                return;
            case Board.StatusFinish:
                CheckKeyDown();
                break;
        }

        Direction direction = null;
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
            direction = GetFlickDirection();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            direction = Direction.up;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            direction = Direction.down;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            direction = Direction.left;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            direction = Direction.right;
        }

        if (ReferenceEquals(null, direction)) return;


        // Debug.Log(direction);
        debugTextBoxDown.GetComponent<Text>().text = direction.ToString();
        Board.Update(direction);
        debugTextBoxDown2.GetComponent<Text>().text = Board.MovesCount.ToString();

        //1: board
        //2: move
        //3: deleteAfterMove
        //4: instance
        var boardStr = "";
        for (var i = 0; i < 4; i++)
        {
            for (var j = 0; j < 4; j++)
            {
                try
                {
                    boardStr += Board.CurrentBoard[i][j];
                }
                catch (NullReferenceException)
                {
                    Util.JagListDebugLog("####### ERROR _board ######## i: " + i + ", j: " + j + ", board",
                        Board.CurrentBoard);
                }
            }

            boardStr += "\n";
        }

        debugTextBoxUp1.GetComponent<Text>().text = boardStr;

        var moveBoardStr = "";
        for (var i = 0; i < 4; i++)
        {
            for (var j = 0; j < 4; j++)
            {
                try
                {
                    moveBoardStr += Board.MoveBoard[i][j];
                }
                catch (NullReferenceException)
                {
                }
            }

            moveBoardStr += "\n";
        }

        debugTextBoxUp2.GetComponent<Text>().text = moveBoardStr;
        // Board.Update("left"); 
        var deleteAfterMoveStr = "";
        for (var i = 0; i < 4; i++)
        {
            for (var j = 0; j < 4; j++)
            {
                try
                {
                    deleteAfterMoveStr += Board.DeleteAfterMoveBoard[i][j];
                }
                catch (NullReferenceException)
                {
                }
            }

            deleteAfterMoveStr += "\n";
        }

        debugTextBoxUp3.GetComponent<Text>().text = deleteAfterMoveStr;
        var instancesStr = "";
        for (var i = 0; i < 4; i++)
        {
            for (var j = 0; j < 4; j++)
            {
                if (Board.Instances[i][j] == null)
                {
                    instancesStr += 0;
                }
                else
                {
                    instancesStr += Board.Instances[i][j].name[0];
                }
            }

            instancesStr += "\n";
        }

        debugTextBoxUp4.GetComponent<Text>().text = instancesStr;
    }

    void CheckKeyDown()
    {
    }

    Direction GetFlickDirection()
    {
        var directionX = _touchEndPos.x - _touchStartPos.x;
        var directionY = _touchEndPos.y - _touchStartPos.y;

        if (Mathf.Abs(directionY) < Mathf.Abs(directionX))
        {
            if (30 < directionX)
            {
                return Direction.right;
            }

            if (-30 > directionX)
            {
                return Direction.left;
            }
        }
        else if (Mathf.Abs(directionX) < Mathf.Abs(directionY))
        {
            if (30 < directionY)
            {
                return Direction.up;
            }

            if (-30 > directionY)
            {
                return Direction.down;
            }
        }

        return null;
    }

    void GetDirection()
    {
        var directionX = _touchEndPos.x - _touchStartPos.x;
        var directionY = _touchEndPos.y - _touchStartPos.y;
        Direction direction = null;

        if (Mathf.Abs(directionY) < Mathf.Abs(directionX))
        {
            if (30 < directionX)
            {
                direction = Direction.right;
            }
            else if (-30 > directionX)
            {
                direction = Direction.left;
            }
        }
        else if (Mathf.Abs(directionX) < Mathf.Abs(directionY))
        {
            if (30 < directionY)
            {
                direction = Direction.up;
            }
            else if (-30 > directionY)
            {
                direction = Direction.down;
            }
        }

        if (ReferenceEquals(null, direction)) return;

        // Debug.Log(direction);
        debugTextBoxDown.GetComponent<Text>().text = direction.ToString();
        Board.Update(direction);
        debugTextBoxDown2.GetComponent<Text>().text = Board.MovesCount.ToString();
    }
}