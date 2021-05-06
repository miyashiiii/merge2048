using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Timeline;
using UnityEngine.UI;

public static class Board
{
    private const int StatusInPlay = 0;
    private const int StatusFinish = 1;

    private static int _status = StatusInPlay;

    private static GameObject _srcObj;

    private static readonly GameObject Canvas = GameObject.Find("Canvas");


    private static PanelManager.Panel[,] _board;

    private static GameObject _canvas;
    private static readonly Vector2[,] PosArray = new Vector2[4, 4];

    public static void Init(Vector2 parentPos, Vector2 cellSize, Vector2 spacing)
    {
        PanelManager.SetNext();

        _canvas = GameObject.Find("Canvas");

        Debug.Log("position:" + parentPos);
        Debug.Log("cellsize:" + cellSize);
        Debug.Log("spacing:" + spacing);
        // foreach (var square in squares)
        // {
        //     Debug.Log(square.name+": "+square.transform.localPosition);
        // }
        int colcount = 0;
        int rowcount = 0;

        var topLeftPos = new Vector2(
            parentPos.x - (cellSize.x + spacing.x) * 1.5f,
            parentPos.y - (cellSize.y + spacing.y) * 1.5f
        );

        foreach (var pos in PosArray)
        {
            var vec = new Vector2(
                topLeftPos.x + (cellSize.x + spacing.x) * colcount,
                topLeftPos.y + (cellSize.y + spacing.y) * rowcount
            );

            PosArray[colcount, rowcount] = vec;
            // Debug.Log("vec: " + vec);

            if (colcount == 3)
            {
                colcount = 0;
                rowcount++;
            }
            else
            {
                colcount++;
            }
        }

        Debug.Log("Board Init");
        _board = new PanelManager.Panel[,]
        {
            {PanelManager.Empty, PanelManager.Empty, PanelManager.Empty, PanelManager.Empty},
            {PanelManager.Empty, PanelManager.Empty, PanelManager.Empty, PanelManager.Empty},
            {PanelManager.Empty, PanelManager.Empty, PanelManager.Empty, PanelManager.Empty},
            {PanelManager.Empty, PanelManager.Empty, PanelManager.Empty, PanelManager.Empty},
        };

        var randCol = Random.Range(0, 4);
        var randRow = Random.Range(0, 4);
        PUT(PanelManager.P2, randCol, randRow);
    }

    private static void PUT(PanelManager.Panel panel, int idxCol, int idxRow)
    {
        Debug.Log("Put");
        var instance = Object.Instantiate(panel.Obj, PosArray[idxCol, idxRow], Quaternion.identity);
        instance.transform.SetParent(_canvas.transform);
        _board[idxCol, idxRow] = panel;
    }

    private static void Finish()
    {
        Debug.Log("Finish");
    }

    private static void CheckFinish()
    {
        Debug.Log("CHECK FINISH");
    }

    public static void Update(string direction)
    {
    }

    public static void Reset()
    {
    }
}