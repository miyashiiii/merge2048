using System;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanelView : MonoBehaviour
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


    // Start is called before the first frame update
    private void Start()
    {
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
            undoButton.SetActive(false); //TODO
            redoButton.SetActive(false); //TODO
        }

        var highScore = PlayerPrefs.GetInt("HIGH_SCORE");

        highScoreText.GetComponent<Text>().text = highScore.ToString();
    }


    // Update is called once per frame
    private void Update()
    {
        UpdateDebugTexts();

        movesText.GetComponent<Text>().text = BoardData.MovesCount.ToString();

        scoreText.GetComponent<Text>().text = BoardData.Score.ToString();


        switch (GameManager.Status)
        {
            case GameManager.StatusFinish:
                var highScore = PlayerPrefs.GetInt("HIGH_SCORE");
                if (highScore < BoardData.Score)
                {
                    PlayerPrefs.SetInt("HIGH_SCORE", BoardData.Score);
                    PlayerPrefs.Save();

                    highScoreText.GetComponent<Text>().text = BoardData.Score.ToString();
                }

                debugTextBoxDown.GetComponent<Text>().text = "GameOver";
                debugTextBoxDown.SetActive(true);

                break;
            default:

                var time = Time.time - BoardData.StartTime;
                var mm = (time / 60).ToString("00");
                var ss = (time % 60).ToString("00");
                timeText.GetComponent<Text>().text = mm + ":" + ss;
                break;
        }
    }

    void UpdateDebugTexts()
    {
        debugTextBoxDown2.GetComponent<Text>().text = BoardData.MovesCount.ToString();
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
                    boardStr += BoardData.CurrentBoard[i][j];
                }
                catch (NullReferenceException)
                {
                    Util.JagListDebugLog("####### ERROR _board ######## i: " + i + ", j: " + j + ", board",
                        BoardData.CurrentBoard);
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
                    moveBoardStr += BoardData.MoveNumBoard[i][j];
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
                    deleteAfterMoveStr += BoardData.DeleteAfterMoveBoard[i][j];
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
                if (BoardView.Instances[i][j] == null)
                {
                    instancesStr += 0;
                }
                else
                {
                    instancesStr += BoardView.Instances[i][j].name[0];
                }
            }

            instancesStr += "\n";
        }

        debugTextBoxUp4.GetComponent<Text>().text = instancesStr;
    }
}