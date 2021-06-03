using UnityEngine;
using System.Collections.Generic;

public static class GameManager
{
    public const int StatusWaitingInput = 0;
    public const int StatusInMovingAnimation = 1;
    public const int StatusInCreateAnimation = 2;
    public const int StatusFinish = 3;

    public static int Status = StatusWaitingInput;


    public static void Init(Vector2 parentPos, Vector2 cellSize, Vector2 spacing, bool fixPut)
    {
        BoardView.Init(parentPos, cellSize, spacing); //DataのInitより先に
        BoardData.Init(fixPut);
    }

    public static void Reset()
    {
        BoardView.Reset(); // Dataのリセットより先に
        BoardData.Reset();
    }




    public static void Move(Direction direction)
    {
        if (Status == StatusFinish)
        {
            return;
        }

        var isMove = BoardData.Move(direction);

        if (!isMove) return;


        BoardView.DirectionInAnimation = direction;

        // moveBoardに従って移動アニメーション
        BoardView.MovingAnimation();
        Status = StatusInMovingAnimation;
    }
}