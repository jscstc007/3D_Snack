using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum MoveTo
{
    X = 0,
    Y,
    Z,
    X_,
    Y_,
    Z_,
}

public enum Face
{
    Face_X_Y_0,
    Face_X_Y_1,
    Face_X_0_Z,
    Face_X_1_Z,
    Face_0_Y_Z,
    Face_1_Y_Z,
}

public struct BlockPoint
{
    public int X;
    public int Y;
    public int Z;

    public BlockPoint(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public bool IsBlock(int x, int y, int z)
    {
        if (X == x && Y == y && Z == z)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public override string ToString()
    {
        return string.Format("{0},{1},{2}", X, Y, Z);
    }
}

public class Game : MonoBehaviour
{

    public Camera camera;

    public Button startB;
    public Button pauseB;
    public Button resetB;
    public Text scoreT;

    public Transform BlocksRoot;
    public GameObject CubeItem;
    private const int BLOCK_NUM = 6;
    private int score = 0;

    private GameObject[,,] Cubes = new GameObject[BLOCK_NUM, BLOCK_NUM, BLOCK_NUM];
    private MeshRenderer[,,] CubeMeshs = new MeshRenderer[BLOCK_NUM, BLOCK_NUM, BLOCK_NUM];
    private static Vector3 ORIGIN_POS = new Vector3(0, 0, 20);

    //Snack [head,...,end]
    private List<BlockPoint> SnackBlock = new List<BlockPoint>();
    private MoveTo NowMoveTo = MoveTo.Y;
    private Face NowFace = Face.Face_X_Y_0;

    private bool IsGameStart = false;
    private bool IsGamePause = false;

    private const float MOVE_TICK = 0.25f;
    private float MoveTick = 0.25f;

    // Use this for initialization
    void Start()
    {
        RegistUI();

        InitGame();
    }

    private void RegistUI()
    {
        startB.onClick.AddListener(Btn_Start);
        pauseB.onClick.AddListener(Btn_Pause);
        resetB.onClick.AddListener(Btn_Reset);
    }

    private void InitGame()
    {
        //CreateBlocks
        if (null != CubeItem && null != BlocksRoot)
        {
            for (int i = 0; i < BLOCK_NUM; i++)
            {
                for (int j = 0; j < BLOCK_NUM; j++)
                {
                    for (int k = 0; k < BLOCK_NUM; k++)
                    {
                        //only created at grim
                        bool createBlock = false;
                        if (i == 0 || i == BLOCK_NUM - 1 || j == 0 || j == BLOCK_NUM - 1 || k == 0 || k == BLOCK_NUM - 1)
                        {
                            createBlock = true;
                        }

                        if (createBlock)
                        {
                            GameObject item = GameObject.Instantiate(CubeItem);
                            item.name = string.Format("{0}_{1}_{2}", i, j, k);

                            item.transform.SetParent(BlocksRoot);
                            item.transform.localPosition = ORIGIN_POS + new Vector3(i * 1 - BLOCK_NUM / 2, j * 1 - BLOCK_NUM / 2, k * 1 - BLOCK_NUM / 2);
                            item.transform.localEulerAngles = Vector3.zero;
                            item.transform.localScale = Vector3.one;
                            item.SetActive(true);

                            Cubes[i, j, k] = item;
                            CubeMeshs[i, j, k] = item.GetComponent<MeshRenderer>();
                        }
                    }
                }
            }
            //set origin cube invisible
            CubeItem.SetActive(false);
        }
        else
        {
            Debug.LogError("Origin Cube Dont Exist!");
        }
    }

    private void InitSnack()
    {
        NowFace = Face.Face_X_Y_0;//FORCE
        NowMoveTo = MoveTo.Y;//excpet Z

        SnackBlock.Clear();
        int randomX = Random.Range(0, BLOCK_NUM);
        int randomY = Random.Range(0, BLOCK_NUM);
        int randomZ = 0;

        SnackBlock.Add(new BlockPoint(randomX, randomY, randomZ));

        UpdateColor();

        Debug.Log("Snack begin at:" + randomX + "," + randomY + "," + randomZ + "--Face:" + NowFace + "--Move:" + NowMoveTo);
    }

    private void Btn_Start()
    {
        //Debug.Log("Click Start");

        if (!IsGameStart)
        {
            IsGameStart = true;

            InitSnack();
        }
        if (IsGamePause)
        {
            IsGamePause = false;
        }
    }

    private void Btn_Pause()
    {
        if (IsGameStart && !IsGamePause)
        {
            IsGamePause = true;
        }
    }

    private void Btn_Reset()
    {
        IsGameStart = false;
        IsGamePause = false;

        SnackBlock.Clear();
        UpdateColor();
    }

    // Update is called once per frame
    void Update()
    {

        if (IsGameStart && !IsGamePause)
        {
            MoveTick -= Time.deltaTime;
            if (MoveTick <= 0)
            {
                MoveTick = MOVE_TICK;

                //move
                Move(NowMoveTo);
                //update color
                UpdateColor();
            }
        }
    }

    private void UpdateColor()
    {
        for (int i = 0; i < BLOCK_NUM; i++)
        {
            for (int j = 0; j < BLOCK_NUM; j++)
            {
                for (int k = 0; k < BLOCK_NUM; k++)
                {
                    if (null != CubeMeshs[i, j, k])
                    {
                        //check if snack
                        bool isSnack = false;

                        foreach (BlockPoint block in SnackBlock)
                        {
                            if (block.IsBlock(i, j, k))
                            {
                                isSnack = true;
                                break;
                            }
                        }

                        //if snack
                        if (isSnack)
                        {
                            CubeMeshs[i, j, k].material.color = Color.red;
                        }
                        //wall
                        else
                        {
                            CubeMeshs[i, j, k].material.color = Color.white;
                        }
                    }
                }
            }
        }
    }

    private void Move(MoveTo towards)
    {
        //move except head
        int count = SnackBlock.Count;
        for (int i = 1; i < SnackBlock.Count; i++)
        {
            SnackBlock[count - i] = SnackBlock[count - i - 1];
        }

        //move head
        BlockPoint oldPoint = SnackBlock[0];
        int oldX = oldPoint.X;
        int oldY = oldPoint.Y;
        int oldZ = oldPoint.Z;

        BlockPoint newPoint = new BlockPoint(oldX, oldY, oldZ);

        switch (towards)
        {
            case MoveTo.X:

                if (NowFace == Face.Face_X_Y_0)
                {
                    if (oldX == BLOCK_NUM - 1)
                    {
                        NowFace = Face.Face_1_Y_Z;
                        NowMoveTo = MoveTo.Z;

                        newPoint.Z += 1;
                    }
                    else
                    {
                        newPoint.X += 1;
                    }
                }
                else if (NowFace == Face.Face_X_Y_1)
                {
                    if (oldX == BLOCK_NUM - 1)
                    {
                        NowFace = Face.Face_1_Y_Z;
                        NowMoveTo = MoveTo.Z_;

                        newPoint.Z -= 1;
                    }
                    else
                    {
                        newPoint.X += 1;
                    }
                }
                else if (NowFace == Face.Face_X_0_Z)
                {
                    if (oldX == BLOCK_NUM - 1)
                    {
                        NowFace = Face.Face_1_Y_Z;
                        NowMoveTo = MoveTo.Y;

                        newPoint.Y += 1;
                    }
                    else
                    {
                        newPoint.X += 1;
                    }
                }
                else if (NowFace == Face.Face_X_1_Z)
                {
                    if (oldX == BLOCK_NUM - 1)
                    {
                        NowFace = Face.Face_1_Y_Z;
                        NowMoveTo = MoveTo.Y_;

                        newPoint.Y -= 1;
                    }
                    else
                    {
                        newPoint.X += 1;
                    }
                }
                break;
            case MoveTo.X_:
                if (NowFace == Face.Face_X_Y_0)
                {
                    if (oldX == 0)
                    {
                        NowFace = Face.Face_0_Y_Z;
                        NowMoveTo = MoveTo.Z;

                        newPoint.Z += 1;
                    }
                    else
                    {
                        newPoint.X -= 1;
                    }
                }
                else if (NowFace == Face.Face_X_Y_1)
                {
                    if (oldX == 0)
                    {
                        NowFace = Face.Face_0_Y_Z;
                        NowMoveTo = MoveTo.Z_;

                        newPoint.Z -= 1;
                    }
                    else
                    {
                        newPoint.X -= 1;
                    }
                }
                else if (NowFace == Face.Face_X_0_Z)
                {
                    if (oldX == 0)
                    {
                        NowFace = Face.Face_0_Y_Z;
                        NowMoveTo = MoveTo.Y;

                        newPoint.Y += 1;
                    }
                    else
                    {
                        newPoint.X -= 1;
                    }
                }
                else if (NowFace == Face.Face_X_1_Z)
                {
                    if (oldX == 0)
                    {
                        NowFace = Face.Face_0_Y_Z;
                        NowMoveTo = MoveTo.Y_;

                        newPoint.Y -= 1;
                    }
                    else
                    {
                        newPoint.X -= 1;
                    }
                }
                break;
            case MoveTo.Y:
                if (NowFace == Face.Face_X_Y_0)
                {
                    if (oldY == BLOCK_NUM - 1)
                    {
                        NowFace = Face.Face_X_1_Z;
                        NowMoveTo = MoveTo.Z;

                        newPoint.Z += 1;
                    }
                    else
                    {
                        newPoint.Y += 1;
                    }
                }
                else if (NowFace == Face.Face_X_Y_1)
                {
                    if (oldY == BLOCK_NUM - 1)
                    {
                        NowFace = Face.Face_X_1_Z;
                        NowMoveTo = MoveTo.Z_;

                        newPoint.Z -= 1;
                    }
                    else
                    {
                        newPoint.Y += 1;
                    }
                }
                else if (NowFace == Face.Face_0_Y_Z)
                {
                    if (oldY == BLOCK_NUM - 1)
                    {
                        NowFace = Face.Face_X_1_Z;
                        NowMoveTo = MoveTo.X;

                        newPoint.X += 1;
                    }
                    else
                    {
                        newPoint.Y += 1;
                    }
                }
                else if (NowFace == Face.Face_1_Y_Z)
                {
                    if (oldY == BLOCK_NUM - 1)
                    {
                        NowFace = Face.Face_X_1_Z;
                        NowMoveTo = MoveTo.X_;

                        newPoint.X -= 1;
                    }
                    else
                    {
                        newPoint.Y += 1;
                    }
                }
                break;
            case MoveTo.Y_:
                if (NowFace == Face.Face_X_Y_0)
                {
                    if (oldY == 0)
                    {
                        NowFace = Face.Face_X_0_Z;
                        NowMoveTo = MoveTo.Z;

                        newPoint.Z += 1;
                    }
                    else
                    {
                        newPoint.Y -= 1;
                    }
                }
                else if (NowFace == Face.Face_X_Y_1)
                {
                    if (oldY == 0)
                    {
                        NowFace = Face.Face_X_0_Z;
                        NowMoveTo = MoveTo.Z_;

                        newPoint.Z -= 1;
                    }
                    else
                    {
                        newPoint.Y -= 1;
                    }
                }
                else if (NowFace == Face.Face_0_Y_Z)
                {
                    if (oldY == 0)
                    {
                        NowFace = Face.Face_X_0_Z;
                        NowMoveTo = MoveTo.X;

                        newPoint.X += 1;
                    }
                    else
                    {
                        newPoint.Y -= 1;
                    }
                }
                else if (NowFace == Face.Face_1_Y_Z)
                {
                    if (oldY == 0)
                    {
                        NowFace = Face.Face_X_0_Z;
                        NowMoveTo = MoveTo.X_;

                        newPoint.X -= 1;
                    }
                    else
                    {
                        newPoint.Y -= 1;
                    }
                }
                break;
            case MoveTo.Z:
                if (NowFace == Face.Face_X_0_Z)
                {
                    if (oldZ == BLOCK_NUM - 1)
                    {
                        NowFace = Face.Face_X_Y_1;
                        NowMoveTo = MoveTo.Y;

                        newPoint.Y += 1;
                    }
                    else
                    {
                        newPoint.Z += 1;
                    }
                }
                else if (NowFace == Face.Face_X_1_Z)
                {
                    if (oldZ == BLOCK_NUM - 1)
                    {
                        NowFace = Face.Face_X_Y_1;
                        NowMoveTo = MoveTo.Y_;

                        newPoint.Y -= 1;
                    }
                    else
                    {
                        newPoint.Z += 1;
                    }
                }
                else if (NowFace == Face.Face_0_Y_Z)
                {
                    if (oldZ == BLOCK_NUM - 1)
                    {
                        NowFace = Face.Face_X_Y_1;
                        NowMoveTo = MoveTo.X;

                        newPoint.X += 1;
                    }
                    else
                    {
                        newPoint.Z += 1;
                    }
                }
                else if (NowFace == Face.Face_1_Y_Z)
                {
                    if (oldZ == BLOCK_NUM - 1)
                    {
                        NowFace = Face.Face_X_Y_1;
                        NowMoveTo = MoveTo.X_;

                        newPoint.X -= 1;
                    }
                    else
                    {
                        newPoint.Z += 1;
                    }
                }
                break;
            case MoveTo.Z_:
                if (NowFace == Face.Face_X_0_Z)
                {
                    if (oldZ == 0)
                    {
                        NowFace = Face.Face_X_Y_0;
                        NowMoveTo = MoveTo.Y;

                        newPoint.Y += 1;
                    }
                    else
                    {
                        newPoint.Z -= 1;
                    }
                }
                else if (NowFace == Face.Face_X_1_Z)
                {
                    if (oldZ == 0)
                    {
                        NowFace = Face.Face_X_Y_0;
                        NowMoveTo = MoveTo.Y_;

                        newPoint.Y -= 1;
                    }
                    else
                    {
                        newPoint.Z -= 1;
                    }
                }
                else if (NowFace == Face.Face_0_Y_Z)
                {
                    if (oldZ == 0)
                    {
                        NowFace = Face.Face_X_Y_0;
                        NowMoveTo = MoveTo.X;

                        newPoint.X += 1;
                    }
                    else
                    {
                        newPoint.Z -= 1;
                    }
                }
                else if (NowFace == Face.Face_1_Y_Z)
                {
                    if (oldZ == 0)
                    {
                        NowFace = Face.Face_X_Y_0;
                        NowMoveTo = MoveTo.X_;

                        newPoint.X -= 1;
                    }
                    else
                    {
                        newPoint.Z -= 1;
                    }
                }
                break;
        }

        SnackBlock[0] = newPoint;

        Debug.Log(string.Format("Move:{0}  Head:{1}  Face:{2}", towards, newPoint.ToString(), NowFace));
    }


    private int GetFaceID()
    {
        //TODO
        return 0;
    }
}
