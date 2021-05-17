using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public static class Board
{
    public const int StatusWaitingInput = 0;
    public const int StatusInMovingAnimation = 1;
    public const int StatusInCreateAnimation = 2;
    public const int StatusFinish = 3;

    public static int Status = StatusWaitingInput;

    private static GameObject _srcObj;

    private static readonly GameObject Canvas = GameObject.Find("Canvas");


    public static int[][] _board;

    private static GameObject _canvas;
    private static Vector2[][] PosArray;

    private static Dictionary<int, float> _panelMap;

    private static PanelManager _pm;

    public static int[][] IsNewBoard;

    private static int MoveFrames = 5;
    private static int CountMoveFrames = 0;

    private static int CreateFrames = 6;
    private static int CountCreateFrames = 0;

    public static int[][] moveBoard;
    public static int[][] deleteAfterMoveBoard;
    private static string directionInAnimation;

    public static GameObject[][] _instances;
    public static int movesCount;

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

        InitPosArray(parentPos, cellSize, spacing);

        // Debug.Log("Board Init");
        _board = new int[][]
        {
            new int[] {0, 0, 0, 0},
            new int[] {0, 0, 0, 0},
            new int[] {0, 0, 0, 0},
            new int[] {0, 0, 0, 0},
        };
        IsNewBoard = new int[][]
        {
            new int[] {0, 0, 0, 0},
            new int[] {0, 0, 0, 0},
            new int[] {0, 0, 0, 0},
            new int[] {0, 0, 0, 0},
        };
        deleteAfterMoveBoard = new int[][]
        {
            new int[] {0, 0, 0, 0},
            new int[] {0, 0, 0, 0},
            new int[] {0, 0, 0, 0},
            new int[] {0, 0, 0, 0},
        };
        _instances = new GameObject[][]
        {
            new GameObject[4],
            new GameObject[4],
            new GameObject[4],
            new GameObject[4],
        };


        PUT(2, 0);
        PUT(2, 1);
        StartCreatingAnimation();
    }

    private static Vector2 CellSize;
    private static Vector2 Spacing;

    private static void InitPosArray(Vector2 parentPos, Vector2 cellSize, Vector2 spacing)
    {
        // Debug.Log("position:" + parentPos);
        // Debug.Log("cellSize:" + cellSize);
        // Debug.Log("spacing:" + spacing);

        CellSize = cellSize;
        Spacing = spacing;

        // var colcount = 0;
        // var rowcount = 0;

        var topLeftPos = new Vector2(
            parentPos.x - (cellSize.x + spacing.x) * 1.5f,
            parentPos.y - (cellSize.y + spacing.y) * 1.5f
        );
        PosArray = new Vector2[4][];
        for (int i = 0; i < 4; i++)

        {
            PosArray[i] = new Vector2[4];
        }

        for (int row = 0; row < 4; row++)
        for (int col = 0; col < 4; col++)
        {
            {
                var vec = new Vector2(
                    topLeftPos.x + (cellSize.x + spacing.x) * col,
                    topLeftPos.y + (cellSize.y + spacing.y) * row
                );
                // rowcount = 3 - rowcount; // todo
                // var count = colcount + (rowcount * 4);
                row = 3 - row;
                PosArray[row][col] = vec;
                // Debug.Log("vec: " + vec);

                // if (colcount == 3)
                // {
                //     colcount = 0;
                //     rowcount++;
                // }
                // else
                // {
                //     colcount++;
                // }
            }
        }

        Util.ListDebugLog("PosArray", PosArray);
    }

    private static List<int> GetEmptyIndices(int[][] board)
    {
        return GetIndices(0, board);
    }

    private static List<int> GetIndices(int p, int[][] board)
    {
        var result = new List<int>();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (board[i][j] == p)
                {
                    result.Add(i * 4 + j);
                }
            }
        }

        return result;
    }


    // static void RandPut()
    // {
    //     // select panel
    //     var p = Util.RandomWithWeight(_panelMap);
    //     var emptyIndices = GetEmptyIndices();
    //     if (emptyIndices.Count == 0)
    //     {
    //         // do nothing if cannot move
    //         return;
    //     }
    //
    //     var randIdx = emptyIndices[Random.Range(0, emptyIndices.Count)];
    //     PUT(p, randIdx);
    // }
    static void RandPut()
    {
        // select panel
        // var p = Util.RandomWithWeight(_panelMap);
        var p = 2;
        var emptyIndices = GetEmptyIndices(_board);
        if (emptyIndices.Count == 0)
        {
            // do nothing if cannot move
            return;
        }

        // var randIdx = emptyIndices[Random.Range(0, emptyIndices.Count)];
        var randIdx = emptyIndices[0]; //TODO
        PUT(p, randIdx);
    }


    static (int, int) NumToIndex(int n)
    {
        int row = (int) Math.Floor((float) (n / 4));
        int col = n % 4;
        return (row, col);
    }

    // PUT
    private static void PUT(int panelNum, int idx)
    {
        var (row, col) = NumToIndex(idx);
        Debug.Log("[PUT] idx: " + idx + ", row" + row + ", col" + col);
        _board[row][col] = panelNum;
        IsNewBoard[row][col] = 1;

        // var p = _pm.Panels[panelNum];
        // Debug.Log("Put");
        // var instance = Object.Instantiate(p, PosArray[row][col], Quaternion.identity);
        // instance.transform.SetParent(_canvas.transform);
        // _instances[idx] = instance;
    }

    private static void Finish()
    {
        Status = StatusFinish;
        // Debug.Log("Finish");
    }

    private static (bool, int[][], int[][], int[][], int[][]) CalcMove(int[][] rows)
    {
        var isMove = false;
        var moveBoard = Enumerable.Repeat<int[]>(null, 4).ToArray();
        var deleteAfterMoveBoard = Enumerable.Repeat<int[]>(null, 4).ToArray();

        var mergedBoard = Enumerable.Repeat<int[]>(null, 4).ToArray();
        var isNewBoard = Enumerable.Repeat<int[]>(null, 4).ToArray();

        var rowCount = 0;
        foreach (var row in rows)
        {
            var moveRow = Enumerable.Repeat(0, 4).ToArray(); // 移動するマス数。rowに対応
            var deleteAfterMoveRow = Enumerable.Repeat(0, 4).ToArray(); // 移動後削除フラグ。moveRowに対応
            var mergedRow = Enumerable.Repeat(0, 4).ToArray(); // マージ後状態
            var isNewRow = Enumerable.Repeat(0, 4).ToArray(); //マージフラグ。mergedに対応。1がマージあり

            var colCount = 0;
            var mergedColCount = 0;

            var deleteIdxPool = 0;
            var before = 0; // マージ確認用 1つ前のempty以外のnumの値
            foreach (var num in row)
            {
                // empty 
                if (num == 0)
                {
                    moveRow[colCount] = 0;
                    colCount++;
                    continue;
                }

                if (before == 0) //before考慮不要
                {
                    before = num;
                    moveRow[colCount] = colCount - mergedColCount;
                    if (mergedColCount < colCount)
                    {
                        isMove = true;
                    }

                    deleteIdxPool = colCount;
                    // mergedColCount++;
                    // mergedへのAddは保留
                }
                else if (before == num) // beforeと同パネル -> マージ
                {
                    moveRow[colCount] = colCount - mergedColCount;
                    deleteAfterMoveRow[colCount] = 1;
                    deleteAfterMoveRow[deleteIdxPool] = 1;
                    if (mergedColCount < colCount)
                    {
                        isMove = true;
                    }

                    mergedRow[mergedColCount] = num * 2;
                    isNewRow[mergedColCount] = 1;
                    mergedColCount++;
                    before = 0;
                }
                else // beforeと別パネル
                {
                    mergedRow[mergedColCount] = before;
                    mergedColCount++;

                    moveRow[colCount] = colCount - mergedColCount;
                    if (mergedColCount < colCount)
                    {
                        isMove = true;
                    }

                    // mergedColCount++;
                    // mergedへのAddは保留
                    before = num;
                    deleteIdxPool = colCount;
                }

                // if (mergedColCount == colCount)
                // {
                //     // moveRow[colCount] = 0;
                // }
                // else
                // {
                //     isMove = true;
                // }

                colCount++;
            }

            // Util.ListDebugLog(rowCount + "row", row); //todo
            // Util.ListDebugLog(rowCount + "idxAfterMove", moveRow); //todo
            // Util.ListDebugLog(rowCount + "merged", mergedRow); //todo
            // Util.ListDebugLog(rowCount + "isNew", isNewRow); //todo


            mergedRow[mergedColCount] = before;

            moveBoard[rowCount] = moveRow;
            deleteAfterMoveBoard[rowCount] = deleteAfterMoveRow;
            mergedBoard[rowCount] = mergedRow;
            isNewBoard[rowCount] = isNewRow;

            rowCount++;
        }

        return (isMove, moveBoard, deleteAfterMoveBoard, mergedBoard, isNewBoard);
    }

    private static void FlushInstances()
    {
        for (var i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (_instances[i][j] == null) continue;
                Object.Destroy(_instances[i][j]);
                _instances[i][j] = null;
            }
        }
    }

    static void UpdatePanels()
    {
        FlushInstances();
        int col = 0;
        int row = 0;
        for (var i = 0; i < 16; i++)
        {
            // Debug.Log("row: " + row);
            // Debug.Log("col: " + col);
            PUT(_board[row][col], i);
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


    static (bool, int[][], int[][], int[][], int[][]) CalcMoveWithConvert(int[][] jagBoard,
        Func<int[][], int[][]> convertFunc,
        Func<int[][], int[][]> reverseFunc)
    {
        int[][] moveBoard;
        int[][] deleteAfterMoveBoard;
        int[][] mergedBoard;
        int[][] isNewBoard;

        // int[][] tmpMergedBoard;
        // int[][] tmpMoveBoard;
        // int[][] tmpIsNewBoard;
        var rotate270Board = convertFunc(jagBoard);
        var (isMove, tmpMoveBoard, tmpDeleteAfterMoveBoarD, tmpMergedBoard, tmpIsNewBoard) = CalcMove(rotate270Board);

        mergedBoard = reverseFunc(tmpMergedBoard);
        moveBoard = reverseFunc(tmpMoveBoard);
        deleteAfterMoveBoard = reverseFunc(tmpDeleteAfterMoveBoarD);
        isNewBoard = reverseFunc(tmpIsNewBoard);
        return (isMove, moveBoard, deleteAfterMoveBoard, mergedBoard, isNewBoard);
    }

    private static (bool, int[][], int[][], int[][], int[][]) CalcMoveByDirection(int[][] jagBoard, string direction)
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
                // Debug.Log("invalid direction");
                return (false, new int[][] { }, new int[][] { }, new int[][] { }, new int[][] { });
        }

        return CalcMoveWithConvert(jagBoard, convertFunc, reverseFunc);
    }

    // public static int[][] MergedBoard;

    public static void Update(string direction)
    {
        IsNewBoard = new int[][]
        {
            new int[] {0, 0, 0, 0},
            new int[] {0, 0, 0, 0},
            new int[] {0, 0, 0, 0},
            new int[] {0, 0, 0, 0},
        };
        directionInAnimation = direction;

        if (Status == StatusFinish)
        {
            return;
        }

        Util.ListDebugLog("board: ", _board);

        bool isMove;
        (isMove, moveBoard, deleteAfterMoveBoard, _board, IsNewBoard) = CalcMoveByDirection(_board, direction);
        if (!isMove)
        {
            return;
        }

        directionInAnimation = direction;
        // moveBoardに従って移動アニメーション
        StartMovingAnimation();
        movesCount++;
    }

    private static bool CheckFinish()
    {
        var jagBoard = new[]
        {
            _board[0],
            _board[1],
            _board[2],
            _board[3],
        };
        // if (GetEmptyIndices().Count > 1)
        // {
        // return;
        // }

        var (_, _, _, uBoard, _) = CalcMoveByDirection(jagBoard, "up");
        var (_, _, _, dBoard, _) = CalcMoveByDirection(jagBoard, "down");
        var (_, _, _, lBoard, _) = CalcMoveByDirection(jagBoard, "left");
        var (_, _, _, rBoard, _) = CalcMoveByDirection(jagBoard, "right");

        //flatten
        var array = uBoard.SelectMany(x => x).ToArray();
        var uArray = uBoard.SelectMany(x => x).ToArray();
        var dArray = dBoard.SelectMany(x => x).ToArray();
        var lArray = lBoard.SelectMany(x => x).ToArray();
        var rArray = rBoard.SelectMany(x => x).ToArray();

        if (array.SequenceEqual(uArray) && array.SequenceEqual(dArray) &&
            array.SequenceEqual(lArray) && array.SequenceEqual(rArray))
        {
            return true;
        }

        return false;
    }


    static void MovingAnimation(bool delete = false)
    {
        if (delete)
        {
            return;
        }

        int[][] tmpBoard = new int[4][];

        if (MoveFrames == CountMoveFrames)
        {
            for (var row = 0; row < 4; row++)
            {
                tmpBoard[row] = new int[4];
                for (var col = 0; col < 4; col++)
                {
                    var moveSquare = moveBoard[row][col];
                    var instance  = _instances[row][col];
                    if (instance == null)
                    {
                        tmpBoard[row][col] = 0;
                    }
                    else
                    {
                    tmpBoard[row][col] = int.Parse(_instances[row][col].name[0].ToString());

                    }
                    //移動せずマージする場合はオブジェクト削除
                    if (moveSquare == 0 && deleteAfterMoveBoard[row][col] == 1)
                    {
                        Object.Destroy(_instances[row][col]);
                        // _instances[row][col] = null;
                    }
                }
            }
        }

        for (var row = 0; row < 4; row++)
        {
            for (var col = 0; col < 4; col++)
            {
                var moveSquare = moveBoard[row][col];
                if (moveSquare == 0)
                {
                    continue;
                }


                var distance = moveSquare * (CellSize.x + Spacing.x) / (MoveFrames + 1);
                Debug.Log("col: " + col + ", row: " + row + ", Move Frames: " + MoveFrames + ", countFrames: " +
                          CountMoveFrames);
                var nextRow = 0;
                var nextCol = 0;
                switch (directionInAnimation)
                {
                    case "left":
                    {
                        _instances[row][col].transform.Translate(-distance, 0, 0);
                        nextRow = row;
                        nextCol = col - moveSquare;
                        break;
                    }
                    case "right":
                    {
                        _instances[row][col].transform.Translate(distance, 0, 0);
                        nextRow = row;
                        nextCol = col + moveSquare;
                        break;
                    }
                    case "up":
                    {
                        _instances[row][col].transform.Translate(0, distance, 0);
                        nextRow = row - moveSquare;
                        nextCol = col;
                        break;
                    }
                    case "down":
                    {
                        _instances[row][col].transform.Translate(0, -distance, 0);
                        nextRow = row + moveSquare;
                        nextCol = col;
                        break;
                    }
                }

                if (MoveFrames == CountMoveFrames)
                {
                    if (deleteAfterMoveBoard[row][col] == 1)
                    {
                        Object.Destroy(_instances[row][col]);
                        _instances[row][col] = null;
                        continue;
                    }

                }
            }
        }
        if (MoveFrames == CountMoveFrames)
        {
            for (var row = 0; row < 4; row++)
            {
                for (var col = 0; col < 4; col++)
                {
                    var moveSquare = moveBoard[row][col];
                    if (moveSquare == 0)
                    {
                        continue;
                    } 
                    var nextRow = 0;
                    var nextCol = 0;
                    switch (directionInAnimation)
                    {
                        case "left":
                        {
                            nextRow = row;
                            nextCol = col - moveSquare;
                            break;
                        }
                        case "right":
                        {
                            nextRow = row;
                            nextCol = col + moveSquare;
                            break;
                        }
                        case "up":
                        {
                            nextRow = row - moveSquare;
                            nextCol = col;
                            break;
                        }
                        case "down":
                        {
                            nextRow = row + moveSquare;
                            nextCol = col;
                            break;
                        }
                    }
                    var num = tmpBoard[row][col];
                    var p = _pm.Panels[num];

                    var clone = Object.Instantiate(p, PosArray[nextRow][nextCol], Quaternion.identity);
                    Debug.Log("move row: " + row + ", col:" + col);
                    Util.JagListDebugLog("move board", _board);
                    Debug.Log("move value: " + _board[row][col]);
                    clone.name = p.ToString();
                    
                    clone.transform.SetParent(_canvas.transform);

                    Object.Destroy(_instances[row][col]);
                    _instances[nextRow][nextCol] = clone;

                    _instances[row][col] = null;

                }
            }
        }

        return;
    }

    public static void StartMovingAnimation()
    {
        MovingAnimation();
        Status = StatusInMovingAnimation;
    }


    public static void ContinueMovingAnimation()
    {
        CountMoveFrames++;
        // Debug.Log("CountMoveFrames: " + CountMoveFrames);

        // finish animation
        // 移動したinstanceを削除
        MovingAnimation();
        if (CountMoveFrames != MoveFrames)
        {
            return;
        }

        CountMoveFrames = 0;
        // Status = StatusWaitingInput;

        // MergedBoard に従って画面更新
        // UpdatePanels();
        RandPut();

        // isNewBoardに従って新規作成アニメーション
        StartCreatingAnimation();
    }

    static void CreatingAnimation()
    {
        Util.JagListDebugLog("IsNewBoard", IsNewBoard);
        for (var row = 0; row < 4; row++)
        {
            for (var col = 0; col < 4; col++)
            {
                var IsNewSquare = IsNewBoard[row][col];
                if (IsNewSquare == 0)
                {
                    continue;
                }

                Debug.Log("row:" + row + ", col:" + col + ", isNewSquare:" + IsNewSquare);
                float scalePerFrame = 0.1f;
                float scale = 1f;
                if (CountCreateFrames <= CreateFrames / 2)
                {
                    scale += (CountCreateFrames + 1) * scalePerFrame;
                }
                else
                {
                    scale += (CreateFrames - CountCreateFrames - 1) * scalePerFrame;
                }

                // var (row, col) = NumToIndex(idx);
                if (CountCreateFrames == 0)
                {
                    var panelNum = _board[row][col];
                    if (panelNum == 0)
                    {
                        Debug.Log("---- ERROR empty panel put ----");
                    }

                    var p = _pm.Panels[panelNum];
                    // Debug.Log("Put");
                    var instance = Object.Instantiate(p, PosArray[row][col], Quaternion.identity);
                    instance.transform.SetParent(_canvas.transform);
                    _board[row][col] = panelNum;
                    IsNewBoard[row][col] = 1;
                    _instances[row][col] = instance;
                }

                Debug.Log(CountCreateFrames);
                _instances[row][col].transform.localScale = new Vector3(scale, scale, 1);
                _instances[row][col].transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }

        return;
    }

    public static void StartCreatingAnimation()

    {
        Status = StatusInCreateAnimation;
        CreatingAnimation();
    }

    public static void ContinueCreatingAnimation()
    {
        CountCreateFrames++;
        // Debug.Log("CountMoveFrames: " + CountMoveFrames);
        if (CountCreateFrames != CreateFrames)
        {
            CreatingAnimation();
            return;
        }

        // finish animation
        CountCreateFrames = 0;

        var isFinish = CheckFinish();
        Status = isFinish ? StatusFinish : StatusWaitingInput;
    }

    public static void Reset()
    {
    }
}