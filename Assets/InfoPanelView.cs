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


    // Start is called before the first frame update
    private void Start()
    {
        if (Config.debug)
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

        GameManager.AddFinishListener(OnFinish);
        GameManager.AddClearListener(OnClear);
    }

    void OnFinish()
    {
        var highScore = PlayerPrefs.GetInt("HIGH_SCORE");
            highScoreText.GetComponent<Text>().text = GameManager.Score.ToString();
        

        debugTextBoxDown.GetComponent<Text>().text = "GameOver";
        debugTextBoxDown.SetActive(true);
        InGame = false;
    }

    void OnClear()
    {
        debugTextBoxDown.SetActive(false);
        InGame = true;
    }

    private bool InGame = true;

    // Update is called once per frame
    private void Update()
    {
        UpdateDebugTexts();

        movesText.GetComponent<Text>().text = GameManager.MovesCount.ToString();

        scoreText.GetComponent<Text>().text = GameManager.Score.ToString();

        if (InGame)
        {
            var time = Time.time - GameManager.StartTime;
            var mm = ((int) time / 60).ToString("00");
            var ss = ((int) time % 60).ToString("00");
            timeText.GetComponent<Text>().text = mm + ":" + ss;
        }
    }

    void UpdateDebugTexts()
    {
        debugTextBoxDown2.GetComponent<Text>().text = GameManager.MovesCount.ToString();
        //1: board
        //2: move
        //3: deleteAfterMove
        //4: instance
        var boardStr = GameManager.CurrentBoard.ToStr();
        debugTextBoxUp1.GetComponent<Text>().text = boardStr;

        var moveBoardStr = GameManager.MoveNumBoard.ToStr();
        debugTextBoxUp2.GetComponent<Text>().text = moveBoardStr;
        
        // Board.Update("left"); 
        var deleteAfterMoveStr = GameManager.DeleteAfterMoveBoard.ToStr();
        debugTextBoxUp3.GetComponent<Text>().text = deleteAfterMoveStr;
        
        var instancesStr = BoardView.Instances.ToStr();
        debugTextBoxUp4.GetComponent<Text>().text = instancesStr;
    }
}