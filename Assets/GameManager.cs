using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public static class GameManager
{
    public const int StatusWaitingInput = 0;
    public const int StatusInMovingAnimation = 1;
    public const int StatusInCreateAnimation = 2;
    public const int StatusFinish = 3;

    public static int Status = StatusWaitingInput;


    public static void Init(Vector2 parentPos, Vector2 cellSize, Vector2 spacing)
    {
        BoardView.Init(parentPos, cellSize, spacing); //DataのInitより先に
        BoardData.Init();
    }

    public static void Reset()
    {
        ClearEvent.Invoke();
        RestartEvent.Invoke();
    }


    private static readonly Queue<Direction> DirectionQue = new Queue<Direction>();

    public static void Move(Direction direction)
    {
        if (direction != null)
        {
            DirectionQue.Enqueue(direction);
        }

        switch (Status)
        {
            case StatusInMovingAnimation:
                BoardView.ContinueMovingAnimation();
                return;
            case StatusInCreateAnimation:
                BoardView.ContinueCreatingAnimation();
                return;
            case StatusFinish:
                DirectionQue.Clear();
                return;
        }

        if (DirectionQue.Count == 0)
        {
            return;
        }

        var firstDirection = DirectionQue.Dequeue();
        if (Status == StatusFinish)
        {
            return;
        }

        var isMove = BoardData.Move(firstDirection);

        if (!isMove) return;


        BoardView.DirectionInAnimation = firstDirection;

        // moveBoardに従って移動アニメーション
        BoardView.MovingAnimation();
        Status = StatusInMovingAnimation;
    }

    private static UnityEvent FinishEvent = null;

    public static void AddFinishListener(UnityAction a)
    {
        // FinishEventがnullなら作成
        FinishEvent ??= new UnityEvent();
        FinishEvent.AddListener(a);
    }

    public static void OnFinish()
    {
        Status = StatusWaitingInput;
        FinishEvent.Invoke();
    }


    private static UnityEvent ClearEvent = null;

    public static void AddClearListener(UnityAction a)
    {
        // ResetEventがnullなら作成
        ClearEvent ??= new UnityEvent();
        ClearEvent.AddListener(a);
    }


    // restart 
    private static UnityEvent RestartEvent = null;

    public static void AddRestartListener(UnityAction a)
    {
        RestartEvent ??= new UnityEvent();
        RestartEvent.AddListener(a);
    }
}