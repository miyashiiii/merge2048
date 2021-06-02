using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    public bool fixPut;


    private Vector3 _touchStartPos;
    private Vector3 _touchEndPos;

    private static readonly Queue<Direction> DirectionQue = new Queue<Direction>();

    // Start is called before the first frame update
    private void Start()
    {
        var glGroup = GetComponent<GridLayoutGroup>();
        var parentPos = transform.position;
        Board.Init(parentPos, glGroup.cellSize, glGroup.spacing, fixPut);

        var highScore = PlayerPrefs.GetInt("HIGH_SCORE");
    }


    // Update is called once per frame
    private void Update()
    {
        Debug.Log("board status: " + Board.Status);

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

        if (!ReferenceEquals(null, direction))
        {
            DirectionQue.Enqueue(direction);
        }

        switch (Board.Status)
        {
            case Board.StatusInMovingAnimation:
                Board.ContinueMovingAnimation();
                return;
            case Board.StatusInCreateAnimation:
                Board.ContinueCreatingAnimation();
                return;
            case Board.StatusFinish:
                return;
        }

        if (DirectionQue.Count == 0) return;

        var firstDirection = DirectionQue.Dequeue();
        // Debug.Log(direction);
        Board.Move(firstDirection);
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
}