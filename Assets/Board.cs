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


    private static int[][] _board;

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

        // Debug.Log("Board Init");
        _board = new int[][]
        {
            new int[]{0, 0, 0, 0},
            new int[]{0, 0, 0, 0},
            new int[]{0, 0, 0, 0},
            new int[]{0, 0, 0, 0},
        };

        RandPut();
        RandPut();
    }

    private static Vector2 CellSize;
    private static Vector2 Spacing;

    private static void MakePosArray(Vector2 parentPos, Vector2 cellSize, Vector2 spacing)
    {
        // Debug.Log("position:" + parentPos);
        // Debug.Log("cellSize:" + cellSize);
        // Debug.Log("spacing:" + spacing);

        CellSize = cellSize;
        Spacing = spacing;

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

    private static List<int> GetEmptyIndices(int[][]board)
    {
        return GetIndices(0,board);
    }

    private static List<int> GetIndices(int p,int[][]board)
    {
    
        var result = new List<int>();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if( board[i][j]==p)
                {
                    result.Add(i*4+j);
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
        var p = Util.RandomWithWeight(_panelMap);
        
        var emptyIndices = GetEmptyIndices(_board);
        if (emptyIndices.Count == 0)
        {
            // do nothing if cannot move
            return;
        }

        var randIdx = emptyIndices[Random.Range(0, emptyIndices.Count)];
        PUT(p, randIdx);
    }

    static GameObject[] _instances = new GameObject[16];

    private static void PUT(int panelNum, int idx)
    {
        var p = _pm.Panels[panelNum];
        Debug.Log("Put");
        var instance = Object.Instantiate(p, PosArray[idx], Quaternion.identity);
        instance.transform.SetParent(_canvas.transform);
        int row = (int) Math.Floor((float) (idx / 4));
        int col = idx % 4;
        _board[row][col] = panelNum;
        _instances[idx] = instance;
    }

    private static void Finish()
    {
        Status = StatusFinish;
        // Debug.Log("Finish");
    }

    private static (bool, int[][], int[][], int[][]) CalcMove(int[][] rows)
    {
        var isMove = false;
        var moveBoard = Enumerable.Repeat<int[]>(null, 4).ToArray();

        var mergedBoard = Enumerable.Repeat<int[]>(null, 4).ToArray();
        var isNewBoard = Enumerable.Repeat<int[]>(null, 4).ToArray();

        var rowCount = 0;
        foreach (var row in rows)
        {
            var moveRow = Enumerable.Repeat(0, 4).ToArray(); // 移動するマス数。rowsに対応
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
                    moveRow[colCount] = 0;
                    colCount++;
                    continue;
                }

                if (before == 0) //before考慮不要
                {
                    before = num;
                    moveRow[colCount] = colCount - mergedColCount;
                    // mergedへのAddは保留
                }
                else if (before == num) // beforeと同パネル -> マージ
                {
                    moveRow[colCount] = colCount - mergedColCount;

                    mergedRow[mergedColCount] = num * 2;
                    isNewRow[mergedColCount] = 1;
                    mergedColCount++;
                    before = 0;
                }
                else // beforeと別パネル
                {
                    moveRow[colCount] = colCount - mergedColCount;
                    mergedRow[mergedColCount] = before;
                    mergedColCount++;
                    // mergedへのAddは保留
                    before = num;
                }

                if (mergedColCount == colCount)
                {
                    moveRow[colCount] = 0;
                }
                else
                {
                    isMove = true;
                }

                colCount++;
            }

            // Util.ListDebugLog(rowCount + "row", row); //todo
            // Util.ListDebugLog(rowCount + "idxAfterMove", moveRow); //todo
            // Util.ListDebugLog(rowCount + "merged", mergedRow); //todo
            // Util.ListDebugLog(rowCount + "isNew", isNewRow); //todo


            mergedRow[mergedColCount] = before;

            moveBoard[rowCount] = moveRow;
            mergedBoard[rowCount] = mergedRow;
            isNewBoard[rowCount] = isNewRow;

            rowCount++;
        }

        return (isMove, mergedBoard, moveBoard, isNewBoard);
    }

    private static void FlushInstances()
    {
        for (var i = 0; i < _instances.Length; i++)
        {
            if (_instances[i] == null) continue;
            Object.Destroy(_instances[i]);
            _instances[i] = null;
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
            PUT(MergedBoard[row][col], i);
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


    static (bool, int[][], int[][], int[][]) CalcMoveWithConvert(int[][] jagBoard, Func<int[][], int[][]> convertFunc,
        Func<int[][], int[][]> reverseFunc)
    {
        int[][] moveBoard;
        int[][] mergedBoard;
        int[][] isNewBoard;

        // int[][] tmpMergedBoard;
        // int[][] tmpMoveBoard;
        // int[][] tmpIsNewBoard;
        var rotate270Board = convertFunc(jagBoard);
        var (isMove, tmpMergedBoard, tmpMoveBoard, tmpIsNewBoard) = CalcMove(rotate270Board);

        mergedBoard = reverseFunc(tmpMergedBoard);
        moveBoard = reverseFunc(tmpMoveBoard);
        isNewBoard = reverseFunc(tmpIsNewBoard);
        return (isMove, mergedBoard, moveBoard, isNewBoard);
    }

    private static (bool, int[][], int[][], int[][]) CalcMoveByDirection(int[][] jagBoard, string direction)
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
                return (false, new int[][] { }, new int[][] { }, new int[][] { });
        }

        return CalcMoveWithConvert(jagBoard, convertFunc, reverseFunc);
    }

    private static int[][] MergedBoard;
    private static int[][] IsNewBoard;

    public static void Update(string direction)
    {
        directionInAnimation = direction;

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
            _board[0],
            _board[1],
            _board[2],
            _board[3],
        };
        bool isMove;
        (isMove, MergedBoard, moveBoardInAnimation, IsNewBoard) = CalcMoveByDirection(jagBoard, direction);

        if (!isMove)
        {
            return;
        }

        directionInAnimation = direction;
        // moveBoardに従って移動アニメーション
        StartMovingAnimation();
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

        var (_, uBoard, _, _) = CalcMoveByDirection(jagBoard, "up");
        var (_, dBoard, _, _) = CalcMoveByDirection(jagBoard, "down");
        var (_, lBoard, _, _) = CalcMoveByDirection(jagBoard, "left");
        var (_, rBoard, _, _) = CalcMoveByDirection(jagBoard, "right");

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

    private static int MoveFrames = 5;
    private static int CountMoveFrames = 0;

    private static int CreateFrames = 6;
    private static int CountCreateFrames = 0;
    
    private static int[][] moveBoardInAnimation;
    private static string directionInAnimation;

    static void MovingAnimation(bool delete = false)
    {
        if (directionInAnimation != "left")
        {
            // return false;
        }

        for (var i = 0; i < moveBoardInAnimation.Length; i++)
        {
            for (var j = 0; j < moveBoardInAnimation[i].Length; j++)
            {
                var moveSquare = moveBoardInAnimation[i][j];
                if (moveSquare == 0)
                {
                    continue;
                }

                if (directionInAnimation == "left")
                {
                    if (delete)
                    {
                        Object.Destroy(_instances[i * 4 + j]);
                        _instances[i * 4 + j] = null;
                        continue;
                    }

                    var distance = moveSquare * -(CellSize.x + Spacing.x) / MoveFrames;
                    _instances[i * 4 + j].transform.Translate(distance, 0, 0);
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
        if (CountMoveFrames != MoveFrames)
        {
            MovingAnimation();
            return;
        }

        // finish animation
        // 移動したinstanceを削除
        MovingAnimation(delete: true);
        CountMoveFrames = 0;
        // Status = StatusWaitingInput;

        // MergedBoard に従って画面更新
        UpdatePanels();
        RandPut();

        // isNewBoardに従って新規作成アニメーション
        StartCreatingAnimation();

    }

    static void CreatingAnimation()
    {
        for (var i = 0; i < IsNewBoard.Length; i++)
        {
            for (var j = 0; j < IsNewBoard[i].Length; j++)
            {
                var IsNewSquare = IsNewBoard[i][j];
                if (IsNewSquare == 0)
                {
                    continue;
                }

                float scalePerFrame = 0.1f;
                float scale = 1f;
                if (CountCreateFrames <= CreateFrames / 2)
                {
                    scale += (CountCreateFrames+1)*scalePerFrame;
                }
                else
                {
                    
                    scale += (CreateFrames-CountCreateFrames-1)*scalePerFrame;
                }
                Debug.Log(CountCreateFrames); 
                    _instances[i * 4 + j].transform.localScale = new Vector3(scale,scale, 1);
                    _instances[i * 4 + j].transform.rotation = Quaternion.Euler(0,0,0);

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
        // 移動したinstanceを削除
        CountCreateFrames = 0;
        
        var isFinish = CheckFinish();
        Status = isFinish ? StatusFinish : StatusWaitingInput;
        
    }
    public static void Reset()
    {
    }
}