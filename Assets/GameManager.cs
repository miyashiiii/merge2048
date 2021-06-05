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
        CurrentBoard = GetEmptyBoard();
        IsNewBoard = GetEmptyBoard();
        DeleteAfterMoveBoard = GetEmptyBoard();

        MovesCount = 0;

        BoardData.Init();
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

    public static void Reset()
    {
        OnClear.Invoke();
        CurrentBoard = GetEmptyBoard();
        IsNewBoard = GetEmptyBoard();
        DeleteAfterMoveBoard = GetEmptyBoard();
        MovesCount = 0;
 
        BoardData.Init();
        OnRestart.Invoke();
    }


    private static readonly Queue<Direction> DirectionQue = new Queue<Direction>();

    public static void Move(Direction direction)
    {
        IsNewBoard = GetEmptyBoard();
        Util.ListDebugLog("board: ", GameManager.CurrentBoard);
         bool isMove;
        (isMove, GameManager.MoveNumBoard, GameManager.DeleteAfterMoveBoard, GameManager.CurrentBoard, GameManager.IsNewBoard) =
            BoardData.CalcMoveByDirection(GameManager.CurrentBoard, direction);
        if (!isMove)
        {
            return ;
        }

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