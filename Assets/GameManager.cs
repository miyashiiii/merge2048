using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public const int StatusWaitingInput = 0;
    public const int StatusInMovingAnimation = 1;
    public const int StatusInCreateAnimation = 2;
    public const int StatusFinish = 3;

    public static int Status = StatusWaitingInput;

    InputManager inputManager = new InputManager();

    public void Start()
    {

        BoardView.Init(); //DataのInitより先に
        BoardData.Init();
        BoardView.StartCreatingAnimation();
 
    }

    public void Update()
    {
        var direction = inputManager.GetInput();
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

        Move(firstDirection);
    }

    public static void Reset()
    {
        
        OnClear.Invoke();
        OnRestart.Invoke();
    }


    private static readonly Queue<Direction> DirectionQue = new Queue<Direction>();

    public static void Move(Direction direction)
    {
        var isMove = BoardData.Move(direction);

        if (!isMove) return;


        BoardView.DirectionInAnimation = direction;

        // moveBoardに従って移動アニメーション
        BoardView.MovingAnimation();
        Status = StatusInMovingAnimation;
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
        Status = StatusWaitingInput;
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