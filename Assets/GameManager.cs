using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public enum GameStatus
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

    public static GameStatus Status = GameStatus.StatusWaitingInput;

    private readonly InputManager _inputManager = new InputManager();

    public static int[][] CurrentBoard;
    public static int[][] IsNewBoard; // 新規作成フラグ。CreateAnimationに使用
    public static int[][] MoveNumBoard; //移動するマス数
    public static int[][] DeleteAfterMoveBoard;//移動後削除フラグ

    public void Start()
    {
        BoardView.Init(); //InitBoardより先に
        InitBoard();
        _onRestart.Invoke();
    }

    public void Update()
    {
        var direction = _inputManager.GetInput();
        if (direction != null)
        {
            DirectionQue.Enqueue(direction);
        }

        switch (Status)
        {
            case GameStatus.StatusInMovingAnimation:
                BoardView.ContinueMovingAnimation();
                return;
            case GameStatus.StatusInCreateAnimation:
                BoardView.ContinueCreatingAnimation();
                return;
            case GameStatus.StatusFinish:
                DirectionQue.Clear();
                return;
        }

        if (DirectionQue.Count == 0)
        {
            return;
        }

        var firstDirection = DirectionQue.Dequeue();
        if (Status == GameStatus.StatusFinish)
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
        _onClear.Invoke();
        InitBoard();
        _onRestart.Invoke();
    }


    private static readonly Queue<Direction> DirectionQue = new Queue<Direction>();

    private static void Move(Direction direction)
    {
        IsNewBoard = GetEmptyBoard();
        Util.ListDebugLog("board: ", CurrentBoard);
        bool isMove;
        int score;
        (isMove, score, MoveNumBoard, DeleteAfterMoveBoard, CurrentBoard, IsNewBoard) =
            BoardData.CalcMoveByDirection(CurrentBoard, direction);
        if (!isMove)
        {
            return;
        }

        Score += score;
        MovesCount++;

        BoardView.DirectionInAnimation = direction;

        // moveBoardに従って移動アニメーション
        BoardView.MovingAnimation();
        Status = GameStatus.StatusInMovingAnimation;
    }

    private static UnityEvent _onFinish;

    public static void AddFinishListener(UnityAction a)
    {
        // FinishEventがnullなら作成
        _onFinish ??= new UnityEvent();
        _onFinish.AddListener(a);
    }

    public static void Finish()
    {
        Status = GameStatus.StatusWaitingInput;
        var highScore = PlayerPrefs.GetInt("HIGH_SCORE");

        if (highScore < Score)
        {
            PlayerPrefs.SetInt("HIGH_SCORE", Score);
            PlayerPrefs.Save();
        }

        _onFinish.Invoke();
    }


    private static UnityEvent _onClear;

    public static void AddClearListener(UnityAction a)
    {
        // ResetEventがnullなら作成
        _onClear ??= new UnityEvent();
        _onClear.AddListener(a);
    }


    // restart 
    private static UnityEvent _onRestart;

    public static void AddRestartListener(UnityAction a)
    {
        _onRestart ??= new UnityEvent();
        _onRestart.AddListener(a);
    }
}