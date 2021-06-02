using System;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public GameObject debugTextBoxUp1;
    public GameObject debugTextBoxUp2;
    public GameObject debugTextBoxUp3;
    public GameObject debugTextBoxUp4;
    public GameObject debugTextBoxDown;
    public GameObject debugTextBoxDown2;

    public GameObject scoreArea;
    public GameObject scoreText;
    public GameObject highScoreArea;
    public GameObject highScoreText;
    public GameObject movesArea;
    public GameObject movesText;
    public GameObject timeArea;
    public GameObject timeText;
    public GameObject undoButton;
    public GameObject redoButton;
    public GameObject resetButton;


    public bool debug;
    public bool fixPut;


    private Vector3 _touchStartPos;
    private Vector3 _touchEndPos;


    // Start is called before the first frame update
    private void Start()
    {
        var glGroup = GetComponent<GridLayoutGroup>();
        var parentPos = transform.position;
        Board.Init(parentPos, glGroup.cellSize, glGroup.spacing, fixPut);
        if (debug)
        {
            scoreArea.SetActive(false);
            highScoreArea.SetActive(false);
            movesArea.SetActive(false);
            timeArea.SetActive(false);
            undoButton.SetActive(false);
            redoButton.SetActive(false);
            // resetButton.SetActive(false);
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

        var highScore = PlayerPrefs.GetInt("HIGH_SCORE");

        highScoreText.GetComponent<Text>().text = highScore.ToString();
    }


    // Update is called once per frame
    private void Update()
    {
        UnityEngine.Debug.Log("board status: " + Board.Status);

        var mm = (Time.time / 60).ToString("00");
        var ss = (Time.time % 60).ToString("00");
        timeText.GetComponent<Text>().text = mm + ":" + ss;

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
                    moveBoardStr += Board.MoveNumBoard[i][j];
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

        movesText.GetComponent<Text>().text = Board.MovesCount.ToString();

        scoreText.GetComponent<Text>().text = Board.Score.ToString();
        var highScore = PlayerPrefs.GetInt("HIGH_SCORE");
        if (highScore < Board.Score)
        {
            PlayerPrefs.SetInt("HIGH_SCORE", Board.Score);
            PlayerPrefs.Save();

            highScoreText.GetComponent<Text>().text = Board.Score.ToString();
        }

        if (ReferenceEquals(null, direction)) return;

        // Debug.Log(direction);
        debugTextBoxDown.GetComponent<Text>().text = direction.ToString();
        Board.Move(direction);
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
        Board.Move(direction);
        debugTextBoxDown2.GetComponent<Text>().text = Board.MovesCount.ToString();
    }
}