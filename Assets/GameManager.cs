using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum Status
    {
        StatusWaitingInput,
        StatusInMovingAnimation,
        StatusInCreateAnimation,
        StatusFinish,
    }
    public static int MovesCount;
    public static int Score;
    public static float StartTime;

    private static int[][] GetEmptyBoard()
    {
        return new[]
        {
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
        };
    }

    public static Status status = Status.StatusWaitingInput;

    InputManager inputManager = new InputManager();

    public static int[][] CurrentBoard;


    public static int[][] IsNewBoard;

    public static int[][] MoveNumBoard;
    public static int[][] DeleteAfterMoveBoard;

    public void Start()
    {
        BoardView.Init(); //DataのInitより先に
        // Debug.Log("Board Init");
        InitBoard();
        OnRestart.Invoke();
    }

    public void Update()
    {
        var direction = inputManager.GetInput();
        if (direction != null)
        {
            DirectionQue.Enqueue(direction);
        }

        switch (status)
        {
            case Status.StatusInMovingAnimation:
                BoardView.ContinueMovingAnimation();
                return;
            case Status.StatusInCreateAnimation:
                BoardView.ContinueCreatingAnimation();
                return;
            case Status.StatusFinish:
                DirectionQue.Clear();
                return;
        }

        if (DirectionQue.Count == 0)
        {
            return;
        }

        var firstDirection = DirectionQue.Dequeue();
        if (status == Status.StatusFinish)
        {
            return;
        }

        Move(firstDirection);
    }

    private static void InitBoard()
    {
        CurrentBoard = GetEmptyBoard();
        IsNewBoard = GetEmptyBoard();
        DeleteAfterMoveBoard = GetEmptyBoard();
        MovesCount = 0;
        Score = 0;
        StartTime = Time.time;
        BoardData.RandPut();
        BoardData.RandPut();
    }
    public static void Reset()
    {
        OnClear.Invoke();
        InitBoard();
        OnRestart.Invoke();
    }


    private static readonly Queue<Direction> DirectionQue = new Queue<Direction>();

    public static void Move(Direction direction)
    {
        IsNewBoard = GetEmptyBoard();
        Util.ListDebugLog("board: ", CurrentBoard);
         bool isMove;
         int score;
        (isMove, score,MoveNumBoard, DeleteAfterMoveBoard, CurrentBoard, IsNewBoard) =
            BoardData.CalcMoveByDirection(CurrentBoard, direction);
        if (!isMove)
        {
            return ;
        }

        Score += score;
        MovesCount++;


        BoardView.DirectionInAnimation = direction;

        // moveBoardに従って移動アニメーション
        BoardView.MovingAnimation();
        status = Status.StatusInMovingAnimation;
    }

    private static UnityEvent OnFinish = null;

    public static void AddFinishListener(UnityAction a)
    {
        // FinishEventがnullなら作成
        OnFinish ??= new UnityEvent();
        OnFinish.AddListener(a);
    }

    public static void Finish()
    {
        status = Status.StatusWaitingInput;
        var highScore = PlayerPrefs.GetInt("HIGH_SCORE");
 
        if (highScore < Score)
        {
            PlayerPrefs.SetInt("HIGH_SCORE", Score);
            PlayerPrefs.Save();

        }
        OnFinish.Invoke();
    }


    private static UnityEvent OnClear = null;

    public static void AddClearListener(UnityAction a)
    {
        // ResetEventがnullなら作成
        OnClear ??= new UnityEvent();
        OnClear.AddListener(a);
    }


    // restart 
    private static UnityEvent OnRestart = null;

    public static void AddRestartListener(UnityAction a)
    {
        OnRestart ??= new UnityEvent();
        OnRestart.AddListener(a);
    }
}