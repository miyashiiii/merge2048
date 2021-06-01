using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public static class Board
{
    public const int StatusWaitingInput = 0;
    public const int StatusInMovingAnimation = 1;
    public const int StatusInCreateAnimation = 2;
    public const int StatusFinish = 3;

    public static int Status = StatusWaitingInput;

    public static int[][] CurrentBoard;

    private static GameObject _canvas;
    private static Vector2[][] _posArray;

    private static Dictionary<int, float> _panelMap;

    private static PanelManager _panelManager;

    public static int[][] IsNewBoard;

    private const int MoveFrames = 5;
    private static int _countMoveFrames;

    private const int CreateFrames = 6;
    private static int _countCreateFrames;

    public static int[][] MoveBoard;
    public static int[][] DeleteAfterMoveBoard;
    private static Direction _directionInAnimation;

    public static GameObject[][] Instances;
    public static int MovesCount;
    public static int Score;

    public static void Init(Vector2 parentPos, Vector2 cellSize, Vector2 spacing)
    {
        _panelManager = new PanelManager();

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
        CurrentBoard = new[]
        {
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
        };
        IsNewBoard = new[]
        {
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
        };
        DeleteAfterMoveBoard = new[]
        {
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
        };
        Instances = new[]
        {
            new GameObject[4],
            new GameObject[4],
            new GameObject[4],
            new GameObject[4],
        };


        RandPut();
        RandPut();
        Score = 0;
        StartCreatingAnimation();
    }

    private static Vector2 _cellSize;
    private static Vector2 _spacing;

    private static void InitPosArray(Vector2 parentPos, Vector2 cellSize, Vector2 spacing)
    {
        // Debug.Log("position:" + parentPos);
        // Debug.Log("cellSize:" + cellSize);
        // Debug.Log("spacing:" + spacing);

        _cellSize = cellSize;
        _spacing = spacing;

        var topLeftPos = new Vector2(
            parentPos.x - (cellSize.x + spacing.x) * 1.5f,
            parentPos.y - (cellSize.y + spacing.y) * 1.5f
        );
        _posArray = new Vector2[4][];
        for (int i = 0; i < 4; i++)

        {
            _posArray[i] = new Vector2[4];
        }

        for (int row = 0; row < 4; row++)
        for (int col = 0; col < 4; col++)
        {
            {
                var vec = new Vector2(
                    topLeftPos.x + (cellSize.x + spacing.x) * col,
                    topLeftPos.y + (cellSize.y + spacing.y) * row
                );
                row = 3 - row;
                _posArray[row][col] = vec;
            }
        }

        Util.ListDebugLog("PosArray", _posArray);
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


    static void RandPut()
    {
        // select panel
        // var p = Util.RandomWithWeight(_panelMap);
        var p = 2; // TODO DEBUG
        var emptyIndices = GetEmptyIndices(CurrentBoard);
        if (emptyIndices.Count == 0)
        {
            // do nothing if cannot move
            return;
        }

        // var randIdx = emptyIndices[Random.Range(0, emptyIndices.Count)];
        var randIdx = emptyIndices[0]; //TODO DEBUG
        PUT(p, randIdx);
    }


    private static (int, int) NumToIndex(int n)
    {
        var row = n / 4;
        var col = n % 4;
        return (row, col);
    }

    // PUT
    private static void PUT(int panelNum, int idx)
    {
        var (row, col) = NumToIndex(idx);
        Debug.Log("[PUT] idx: " + idx + ", row" + row + ", col" + col);
        CurrentBoard[row][col] = panelNum;
        IsNewBoard[row][col] = 1;
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


                colCount++;
            }


            mergedRow[mergedColCount] = before;

            moveBoard[rowCount] = moveRow;
            deleteAfterMoveBoard[rowCount] = deleteAfterMoveRow;
            mergedBoard[rowCount] = mergedRow;
            isNewBoard[rowCount] = isNewRow;

            rowCount++;
        }

        return (isMove, moveBoard, deleteAfterMoveBoard, mergedBoard, isNewBoard);
    }


    public delegate int[][] ConvertBoard(int[][] board);

    private static (bool, int[][], int[][], int[][], int[][]) CalcMoveWithConvert(int[][] jagBoard,
        ConvertBoard convertFunc,
        ConvertBoard reverseFunc)
    {
        // int[][] tmpMergedBoard;
        // int[][] tmpMoveBoard;
        // int[][] tmpIsNewBoard;
        var convertedBoard = convertFunc(jagBoard);
        var (isMove, tmpMoveBoard, tmpDeleteAfterMoveBoarD, tmpMergedBoard, tmpIsNewBoard) = CalcMove(convertedBoard);

        var mergedBoard = reverseFunc(tmpMergedBoard);
        var moveBoard = reverseFunc(tmpMoveBoard);
        var deleteAfterMoveBoard = reverseFunc(tmpDeleteAfterMoveBoarD);
        var isNewBoard = reverseFunc(tmpIsNewBoard);
        return (isMove, moveBoard, deleteAfterMoveBoard, mergedBoard, isNewBoard);
    }

    private static (bool, int[][], int[][], int[][], int[][]) CalcMoveByDirection(int[][] jagBoard, Direction direction)
    {
        return CalcMoveWithConvert(jagBoard, direction.ConvertFunc, direction.ReverseFunc);
    }

    // public static int[][] MergedBoard;

    public static void Move(Direction direction)
    {
        IsNewBoard = new[]
        {
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
            new[] {0, 0, 0, 0},
        };
        _directionInAnimation = direction;

        if (Status == StatusFinish)
        {
            return;
        }

        Util.ListDebugLog("board: ", CurrentBoard);

        bool isMove;
        (isMove, MoveBoard, DeleteAfterMoveBoard, CurrentBoard, IsNewBoard) =
            CalcMoveByDirection(CurrentBoard, direction);
        if (!isMove)
        {
            return;
        }

        _directionInAnimation = direction;
        // moveBoardに従って移動アニメーション
        StartMovingAnimation();
        MovesCount++;
    }

    private static bool CheckFinish()
    {
        var jagBoard = new[]
        {
            CurrentBoard[0],
            CurrentBoard[1],
            CurrentBoard[2],
            CurrentBoard[3],
        };
        // if (GetEmptyIndices().Count > 1)
        // {
        // return;
        // }

        var (_, _, _, uBoard, _) = CalcMoveByDirection(jagBoard, Direction.up);
        var (_, _, _, dBoard, _) = CalcMoveByDirection(jagBoard, Direction.down);
        var (_, _, _, lBoard, _) = CalcMoveByDirection(jagBoard, Direction.left);
        var (_, _, _, rBoard, _) = CalcMoveByDirection(jagBoard, Direction.right);

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


    private static void MovingAnimation(bool delete = false)
    {
        if (delete)
        {
            return;
        }

        int[][] tmpBoard = new int[4][];

        if (MoveFrames == _countMoveFrames)
        {
            for (var row = 0; row < 4; row++)
            {
                tmpBoard[row] = new int[4];
                for (var col = 0; col < 4; col++)
                {
                    var moveSquare = MoveBoard[row][col];
                    var instance = Instances[row][col];
                    tmpBoard[row][col] = ReferenceEquals(instance, null) ? 0 : int.Parse(instance.name[0].ToString());

                    //移動せずマージする場合はオブジェクト削除
                    if (moveSquare == 0 && DeleteAfterMoveBoard[row][col] == 1)
                    {
                        UnityEngine.Object.Destroy(Instances[row][col]);
                        // _instances[row][col] = null;
                    }
                }
            }
        }

        for (var row = 0; row < 4; row++)
        {
            for (var col = 0; col < 4; col++)
            {
                var moveSquare = MoveBoard[row][col];
                if (moveSquare == 0)
                {
                    continue;
                }


                var distance = moveSquare * (_cellSize.x + _spacing.x) / (MoveFrames + 1);
                Debug.Log("col: " + col + ", row: " + row + ", Move Frames: " + MoveFrames + ", countFrames: " +
                          _countMoveFrames);
                var (distanceX, distanceY) = _directionInAnimation.Get2dDistance(distance);
                Instances[row][col].transform.Translate(distanceX, distanceY, 0);

                if (MoveFrames != _countMoveFrames || DeleteAfterMoveBoard[row][col] != 1) continue;
                UnityEngine.Object.Destroy(Instances[row][col]);
                Instances[row][col] = null;
            }
        }

        if (MoveFrames == _countMoveFrames)
        {
            for (var row = 0; row < 4; row++)
            {
                for (var col = 0; col < 4; col++)
                {
                    var moveSquare = MoveBoard[row][col];
                    if (moveSquare == 0)
                    {
                        continue;
                    }

                    var (nextRow, nextCol) = _directionInAnimation.GETNext(moveSquare, row, col);

                    var num = tmpBoard[row][col];
                    var p = _panelManager.PanelMap[num];

                    var clone = UnityEngine.Object.Instantiate(p, _posArray[nextRow][nextCol], Quaternion.identity);
                    Debug.Log("move row: " + row + ", col:" + col);
                    Util.JagListDebugLog("move board", CurrentBoard);
                    Debug.Log("move value: " + CurrentBoard[row][col]);
                    clone.name = p.ToString();

                    clone.transform.SetParent(_canvas.transform);

                    UnityEngine.Object.Destroy(Instances[row][col]);
                    Instances[nextRow][nextCol] = clone;

                    Instances[row][col] = null;
                }
            }
        }
    }

    private static void StartMovingAnimation()
    {
        MovingAnimation();
        Status = StatusInMovingAnimation;
    }


    public static void ContinueMovingAnimation()
    {
        _countMoveFrames++;

        // finish animation
        // 移動したinstanceを削除
        MovingAnimation();
        if (_countMoveFrames != MoveFrames)
        {
            return;
        }

        _countMoveFrames = 0;
        // Status = StatusWaitingInput;

        // MergedBoard に従って画面更新
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
                var isNewSquare = IsNewBoard[row][col];
                if (isNewSquare == 0)
                {
                    continue;
                }

                Debug.Log("row:" + row + ", col:" + col + ", isNewSquare:" + isNewSquare);
                const float scalePerFrame = 0.1f;
                var scale = 1f;
                if (_countCreateFrames <= CreateFrames / 2)
                {
                    scale += (_countCreateFrames + 1) * scalePerFrame;
                }
                else
                {
                    scale += (CreateFrames - _countCreateFrames - 1) * scalePerFrame;
                }

                // var (row, col) = NumToIndex(idx);
                if (_countCreateFrames == 0)
                {
                    var panelNum = CurrentBoard[row][col];
                    if (panelNum == 0)
                    {
                        Debug.Log("---- ERROR empty panel put ----");
                    }

                    var p = _panelManager.PanelMap[panelNum];
                    // Debug.Log("Put");
                    var instance = UnityEngine.Object.Instantiate(p, _posArray[row][col], Quaternion.identity);
                    instance.transform.SetParent(_canvas.transform);
                    CurrentBoard[row][col] = panelNum;
                    IsNewBoard[row][col] = 1;
                    Instances[row][col] = instance;
                    Score += panelNum;
                }

                Debug.Log(_countCreateFrames);
                Instances[row][col].transform.localScale = new Vector3(scale, scale, 1);
                Instances[row][col].transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }
    }

    private static void StartCreatingAnimation()

    {
        Status = StatusInCreateAnimation;
        CreatingAnimation();
    }

    public static void ContinueCreatingAnimation()
    {
        _countCreateFrames++;
        // Debug.Log("CountMoveFrames: " + CountMoveFrames);
        if (_countCreateFrames != CreateFrames)
        {
            CreatingAnimation();
            return;
        }

        // finish animation
        _countCreateFrames = 0;

        var isFinish = CheckFinish();
        Status = isFinish ? StatusFinish : StatusWaitingInput;
    }

    public static void Reset()
    {
    }
}