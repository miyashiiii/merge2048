using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public static class Board
{
    private const int StatusInPlay = 0;
    private const int StatusFinish = 1;

    public static int Status = StatusInPlay;

    private static GameObject _srcObj;

    private static readonly GameObject Canvas = GameObject.Find("Canvas");


    private static List<int> _board = new List<int>();

    private static GameObject _canvas;
    private static readonly Vector2[] PosArray = new Vector2[16];

    private static Dictionary<int, float> _panelMap;

    private static PanelManager _pm;

    public static void Init(Vector2 parentPos, Vector2 cellSize, Vector2 spacing)
    {
        _pm = new PanelManager();

        // panelMap= new Dictionary<PanelManager.Panel, float>
        _panelMap = new Dictionary<int, float>
        {
            {2, 0.9f},
            {4, 0.1f},
            {8, 0f},
        };
        
        _canvas = GameObject.Find("Canvas");

        MakePosArray(parentPos, cellSize, spacing);

        Debug.Log("Board Init");
        _board = new List<int>
        {
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
            0, 0, 0, 0,
        };

        RandPut();
        RandPut();
    }

    private static void MakePosArray(Vector2 parentPos, Vector2 cellSize, Vector2 spacing)
    {
        Debug.Log("position:" + parentPos);
        Debug.Log("cellSize:" + cellSize);
        Debug.Log("spacing:" + spacing);

        var colcount = 0;
        var rowcount = 0;

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
            rowcount = 3 - rowcount; // todo
            var count = colcount + (rowcount * 4);
            PosArray[count] = vec;
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

        Util.ListDebugLog("PosArray", PosArray);
    }

    private static List<int> GetEmptyIndices()
    {
        return GetIndices(0);
    }

    private static List<int> GetIndices(int p)
    {
        var idx = _board.IndexOf(p);
        Debug.Log("board length: " + _board.Count);
        Debug.Log("board: " + _board);
        Debug.Log("p: " + p);
        var result = new List<int>();
        if (idx < 0) return result;

        result.Add(idx);


        // IndexOfメソッドで見つからなくなるまで繰り返す
        while (idx > 0)
        {
            //見つかった位置の次の位置から検索
            idx = _board.IndexOf(p, idx + 1);
            if (idx > 0)
            {
                result.Add(idx);
            }
        }

        return result;
    }


    static void RandPut()
    {
        // select panel
        var p = Util.RandomWithWeight(_panelMap);
        var emptyIndices = GetEmptyIndices();
        if (emptyIndices.Count == 0)
        {
            // do nothing if cannot move
            return;
        }

        var randIdx = emptyIndices[Random.Range(0, emptyIndices.Count)];
        PUT(p, randIdx);
    }

    static List<GameObject> _instances = new List<GameObject>();

    private static void PUT(int panelNum, int idx)
    {
        var p = _pm.Panels[panelNum];
        Debug.Log("Put");
        var instance = Object.Instantiate(p, PosArray[idx], Quaternion.identity);
        instance.transform.SetParent(_canvas.transform);
        _board[idx] = panelNum;
        _instances.Add(instance);
    }

    private static void Finish()
    {
        Status = StatusFinish;
        Debug.Log("Finish");
    }

    private static (int[][], int[][], int[][]) CalcMove(int[][] rows)
    {
        var moveBoard = Enumerable.Repeat<int[]>(null, 4).ToArray();

        var mergedBoard = Enumerable.Repeat<int[]>(null, 4).ToArray();
        var isNewBoard = Enumerable.Repeat<int[]>(null, 4).ToArray();

        var rowCount = 0;
        foreach (var row in rows)
        {
            var moveRow = Enumerable.Repeat(-1, 4).ToArray(); // 移動後のidx。rowsに対応
            var mergedRow = Enumerable.Repeat(0, 4).ToArray(); // マージ後状態
            var isNewRow = Enumerable.Repeat(0, 4).ToArray(); //マージフラグ。mergedに対応。1がマージあり

            var colCount = 0;
            var mergedColCount = 0;

            var before = 0; // マージ確認用 1つ前のempty以外のnumの値
            foreach (var num in row)
            {
                // empty 
                if (num == 0)
                {
                    moveRow[colCount] = -1;
                    colCount++;
                    continue;
                }

                if (before == 0) //before考慮不要
                {
                    before = num;
                    moveRow[colCount] = mergedColCount;
                    // mergedへのAddは保留
                }
                else if (before == num) // beforeと同パネル -> マージ
                {
                    moveRow[colCount] = mergedColCount;

                    mergedRow[mergedColCount] = num * 2;
                    isNewRow[mergedColCount] = 1;
                    mergedColCount++;
                    before = 0;
                }
                else // beforeと別パネル
                {
                    moveRow[colCount] = mergedColCount;
                    mergedRow[mergedColCount] = before;
                    mergedColCount++;
                    // mergedへのAddは保留
                    before = num;
                }

                if (mergedColCount == colCount)
                {
                    moveRow[colCount] = -1;
                }

                colCount++;
            }

            Util.ListDebugLog(rowCount + "row", row); //todo
            Util.ListDebugLog(rowCount + "idxAfterMove", moveRow); //todo
            Util.ListDebugLog(rowCount + "merged", mergedRow); //todo
            Util.ListDebugLog(rowCount + "isNew", isNewRow); //todo
            mergedRow[mergedColCount] = before;

            moveBoard[rowCount] = moveRow;
            mergedBoard[rowCount] = mergedRow;
            isNewBoard[rowCount] = isNewRow;

            rowCount++;
        }

        return (mergedBoard, moveBoard, isNewBoard);
    }

    private static void FlushInstances()
    {
        foreach (var instance in _instances)
        {
            Object.Destroy(instance);
        }

        _instances = new List<GameObject>();
    }

    static void UpdatePanels(int[][] merged4)
    {
        FlushInstances();
        int col = 0;
        int row = 0;
        for (var i = 0; i < 16; i++)
        {
            Debug.Log("row: " + row);
            Debug.Log("col: " + col);
            PUT(merged4[row][col], i);
            if (col == 3)
            {
                col = 0;
                row++;
            }
            else
            {
                col++;
            }
        }
    }

    static int[][] FlipBoard(int[][] board)
    {
        var reverseBoard = new int[4][];
        for (var i = 0; i < 4; i++)
        {
            reverseBoard[i] = board[i].Reverse().ToArray();
        }

        return reverseBoard;
    }

    private static int[][] RotateBoardClockwise(int[][] board)
    {
        const int rows = 4;
        const int cols = 4;
        var t = new int[4][];
        for (var j = 0; j < cols; j++)
        {
            t[j] = new int[4];
        }

        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < cols; j++)
            {
                t[j][rows - i - 1] = board[i][j];
            }
        }

        return t;
    }

    private static int[][] RotateBoardAnticlockwise(int[][] board)
    {
        const int rows = 4;
        const int cols = 4;
        var t = new int[4][];
        for (var j = 0; j < cols; j++)
        {
            t[j] = new int[4];
        }

        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < cols; j++)
            {
                t[cols - j - 1][i] = board[i][j];
            }
        }

        return t;
    }

    private static int[][] NoRotate(int[][] board)
    {
        return board;
    }


    static (int[][], int[][], int[][]) CalcMoveWithConvert(int[][] jagBoard, Func<int[][], int[][]> convertFunc,
        Func<int[][], int[][]> reverseFunc)
    {
        int[][] moveBoard;
        int[][] mergedBoard;
        int[][] isNewBoard;

        int[][] tmpMergedBoard;
        int[][] tmpMoveBoard;
        int[][] tmpIsNewBoard;
        var rotate270Board = convertFunc(jagBoard);
        (tmpMergedBoard, tmpMoveBoard, tmpIsNewBoard) = CalcMove(rotate270Board);

        mergedBoard = reverseFunc(tmpMergedBoard);
        moveBoard = reverseFunc(tmpMoveBoard);
        isNewBoard = reverseFunc(tmpIsNewBoard);
        return (mergedBoard, moveBoard, isNewBoard);
    }

    private static (int[][], int[][], int[][]) CalcMoveByDirection(int[][] jagBoard, string direction)
    {
        Func<int[][], int[][]> convertFunc;
        Func<int[][], int[][]> reverseFunc;

        switch (direction)
        {
            case "up":
                convertFunc = RotateBoardAnticlockwise;
                reverseFunc = RotateBoardClockwise;
                break;
            case "down":
                convertFunc = RotateBoardClockwise;
                reverseFunc = RotateBoardAnticlockwise;
                break;
            case "left":
                convertFunc = NoRotate;
                reverseFunc = NoRotate;
                break;
            case "right":
                convertFunc = FlipBoard;
                reverseFunc = FlipBoard;
                break;
            default:
                Debug.Log("invalid direction");
                return (new int[][] { }, new int[][] { }, new int[][] { });
        }

        return CalcMoveWithConvert(jagBoard, convertFunc, reverseFunc);
    }

    public static void Update(string direction)
    {
        if (Status == StatusFinish)
        {
            return;
        }

        Util.ListDebugLog("board: ", _board);
        int[][] moveBoard;
        int[][] mergedBoard;
        int[][] isNewBoard;

        var jagBoard = new[]
        {
            _board.GetRange(0, 4).ToArray(),
            _board.GetRange(4, 4).ToArray(),
            _board.GetRange(8, 4).ToArray(),
            _board.GetRange(12, 4).ToArray(),
        };

        switch (direction)
        {
            case "up":
                (mergedBoard, moveBoard, isNewBoard) =
                    CalcMoveWithConvert(jagBoard, RotateBoardAnticlockwise, RotateBoardClockwise);
                break;

            case "down":
                (mergedBoard, moveBoard, isNewBoard) =
                    CalcMoveWithConvert(jagBoard, RotateBoardClockwise, RotateBoardAnticlockwise);
                break;

            case "left":
                (mergedBoard, moveBoard, isNewBoard) = CalcMove(jagBoard);
                break;

            case "right":
                (mergedBoard, moveBoard, isNewBoard) = CalcMoveWithConvert(jagBoard, FlipBoard, FlipBoard);

                break;
            default:
                return;
        }

        // moveBoardに従って移動アニメーション
        MoveAnimation(moveBoard, direction);
        // mergedBoard に従って画面更新
        UpdatePanels(mergedBoard);
        RandPut();

        // isNewBoardに従って新規作成アニメーション
        CreateAnimation(isNewBoard);

        CheckFinish(jagBoard);
    }

    private static void CheckFinish(int[][] jagBoard)
    {
        if (GetEmptyIndices().Count > 1)
        {
            return;
        }

        var (uBoard, _, _) = CalcMoveByDirection(jagBoard, "up");
        var (dBoard, _, _) = CalcMoveByDirection(jagBoard, "down");
        var (lBoard, _, _) = CalcMoveByDirection(jagBoard, "left");
        var (rBoard, _, _) = CalcMoveByDirection(jagBoard, "right");

        //flatten
        var array = uBoard.SelectMany(x => x).ToArray();
        var uArray = uBoard.SelectMany(x => x).ToArray();
        var dArray = dBoard.SelectMany(x => x).ToArray();
        var lArray = lBoard.SelectMany(x => x).ToArray();
        var rArray = rBoard.SelectMany(x => x).ToArray();

        if (array.SequenceEqual(uArray) && array.SequenceEqual(dArray) &&
            array.SequenceEqual(lArray) && array.SequenceEqual(rArray))
        {
            Finish();
        }
    }

    public static void MoveAnimation(int[][] moveBoard, string direction)
    {
    }

    public static void CreateAnimation(int[][] isNew)
    {
    }

    public static void Reset()
    {
    }
}