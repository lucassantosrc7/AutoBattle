using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Grid : MonoBehaviour
{
    #region Fisrt/last point
    //Value of the first point in X on the grid
    public float firstXPoint
    {
        get
        {
            return transform.position.x;
        }
    }

    //Value of the last point in X on the grid
    public float lastXPoint
    {
        get
        {
            return transform.position.x + ((createGrid.Length - 1) * size);
        }
    }
    #endregion

    public float size = 1f; //Size of tile grid
    public CreateGrid[] createGrid; //gridValues.length == Column //values == Lines

    public List<GridSlot> gridSlots; //All gridSlots in this grid
    public List<GridSlot> FinalPath;//The completed path that the red line will be drawn along
    public Sprite sprite; //Sprite of tile grid

    [SerializeField]
    bool drawGizmo = true, createOnStart = false; //If you want a preview, if this grid is create on start

    void Awake()
    {
        //Disable the drawGizmo on start
        drawGizmo = false;

        //Create slot
        if (createOnStart) CreateSlots();
    }

    //Get the nearest point on grid by a position
    public Vector2 GetNearestPointOnGrid(Vector2 position)
    {
        //Get the point relative to grid
        position = transform.InverseTransformPoint(position);

        //Divide by size and return a Int
        int xCount = Mathf.RoundToInt(position.x / size);
        int yCount = Mathf.RoundToInt(position.y / size);

        //Get the point in grid
        Vector2 result = new Vector2(
            (float)xCount * size,
            (float)yCount * size);

        //Get the point relative to world
        result = transform.TransformPoint(result);

        //And return
        return result;
    }

    public GridSlot GetGridSlot(Vector2 point)
    {
        //Return if the gridSlots is not set yet
        if (gridSlots == null || gridSlots.Count <= 0)
        {
            return null;
        }

        GridSlot toReturn = null;//Who will return
        float minDis = float.MaxValue;//Get the min distance

        for (int i = 0; i < gridSlots.Count; i++)
        {
            //Get distance
            float dis = Vector2.Distance(point, gridSlots[i].position);

            //If is less than the last distance
            if (dis < minDis)
            {
                //Set a new minDis
                minDis = dis;
                //Save to return
                toReturn = gridSlots[i];

                //Since we won't have anything smaller than zero, then we return
                if (dis == 0) break;
            }
        }

        return toReturn;
    }

    //Find the up, left, down and rigth gridSlot of a gridSlot
    void GetNeighboring(GridSlot grid)
    {
        //First we take the up neighbor, to do this we have add 1 in matrix X
        int pointM = grid.matrix.x + 1;
        grid.upNeighbor = GetNeighborByX(grid.position, pointM, createGrid[grid.matrix.y].numX);

        //Now the down neighbor, to do this we have subtract 1 in matrix X
        pointM = grid.matrix.x - 1;
        grid.downNeighbor = GetNeighborByX(grid.position, pointM);

        //Now the rigth neighbor, to do this we have add 1 in matrix Y
        pointM = grid.matrix.y + 1;
        grid.rightNeighbor = GetNeighborByY(grid.position, pointM, createGrid.Length);

        //Now the left neighbor, to do this we have subtract 1 in matrix Y
        pointM = grid.matrix.y - 1;
        grid.leftNeighbor = GetNeighborByY(grid.position, pointM);
    }

    #region GetNeighbor Functions
    //Get the Neighbor in X by receiving the pos, the X point on matrix and the maxPoint that he can be go
    GridSlot GetNeighborByX(Vector2 currentPos, int xPointMatrix, int maxPoint)
    {
        if (xPointMatrix < maxPoint)
        {
            //Add size and get the gridSlot
            return GetGridSlot(AddSize(currentPos, false));
        }

        return null;
    }

    //Get the Neighbor in X by receiving the pos, the X point on matrix, if dont have maxPoint, it just needs to be greater than zero.
    GridSlot GetNeighborByX(Vector2 currentPos, int xPointMatriz)
    {
        if (xPointMatriz >= 0)
        {
            //Subtract size and get the gridSlot
            return GetGridSlot(SubtractSize(currentPos, false));
        }

        return null;
    }

    //Get the Neighbor in Y by receiving the pos, the X point on matrix and the maxPoint that he can be go
    GridSlot GetNeighborByY(Vector2 currentPos, int yPointMatriz, int maxPoint)
    {
        if (yPointMatriz < maxPoint)
        {
            //Add size and get the gridSlot
            return GetGridSlot(AddSize(currentPos, true));
        }

        return null;
    }

    //Get the Neighbor in X by receiving the pos, the Y point on matrix, if dont have maxPoint, it just needs to be greater than zero.
    GridSlot GetNeighborByY(Vector2 currentPos, int yPointMatriz)
    {
        if (yPointMatriz >= 0)
        {
            //Subtract size and get the gridSlot
            return GetGridSlot(SubtractSize(currentPos, true));
        }

        return null;
    }

    //Add size in x or y
    Vector2 AddSize(Vector2 currentPos, bool xAxis)
    {
        if (xAxis)
        {
            currentPos.x += size;
        }
        else
        {
            currentPos.y += size;
        }

        return currentPos;
    }

    //Subtract size in x or y
    Vector2 SubtractSize(Vector2 currentPos, bool xAxis)
    {
        if (xAxis)
        {
            currentPos.x -= size;
        }
        else
        {
            currentPos.y -= size;
        }

        return currentPos;
    }
    #endregion

    /*public Vector2 ClampGrid(Vector2 pos)
    {
        pos.x = Mathf.Clamp(pos.x, firstXPoint, lastXPoint);
        pos.x = GetNearestPointOnGrid(pos).x;

        float iByPos = Mathf.InverseLerp(firstXPoint - size, lastXPoint, pos.x);
        iByPos *= createGrid.Length;
        int i = Mathf.RoundToInt(iByPos) - 1;

        float firstYPoint = transform.position.y + createGrid[i].startY;
        float lastYPoint = firstYPoint + ((createGrid[i].numX - 2) + size);

        pos.y = Mathf.Clamp(pos.y, firstYPoint, lastYPoint);
        pos.y = GetNearestPointOnGrid(pos).y;

        return pos;
    }*/

    //Here we create the slots
    public void CreateSlots()
    {
        gridSlots = new List<GridSlot>();//Reset the gridSlots

        //Get the pos where start
        Vector2 pos = transform.position;
        //Save the Y
        float iniPosY = pos.y;

        for (int i = 0; i < createGrid.Length; i++)
        {
            //Change the initial position in Y defined by startY
            pos.y = iniPosY + (createGrid[i].startY * size);
            //Get the X positions
            for (int j = 0; j < createGrid[i].numX; j++)
            {
                //Get a point in the grid
                Vector2 point = GetNearestPointOnGrid(pos);
                //Create the SlotGrid
                GridSlot slot = new GridSlot(point, new Vector2Int(j, i), this);
                //Then add to the list
                gridSlots.Add(slot);
                //And finally add size in Y
                pos += (Vector2)transform.TransformDirection(Vector2.up) * size;
            }
            //Add size in X
            pos += (Vector2)transform.TransformDirection(Vector2.right) * size;
        }

        //Get the neighbors of all gridSlots
        for (int i = 0; i < gridSlots.Count; i++)
        {
            GetNeighboring(gridSlots[i]);
        }
    }

    //Create a preview grid
    void OnDrawGizmos()
    {
        //Check size and if we can be create a grid
        if (size < 1 || !drawGizmo || createGrid == null)
        {
            return;
        }

        //Get the pos where start
        Vector2 pos = transform.position;
        //Save the Y
        float iniPosY = pos.y;
        //Set the cube to black
        bool firstWhite = false;

        for (int i = 0; i < createGrid.Length; i++)
        {
            //Change the initial position in Y defined by startY
            pos.y = iniPosY + (createGrid[i].startY * size);
            //Get the X positions
            for (int j = 0; j < createGrid[i].numX; j++)
            {
                //Get a point in the grid
                Vector2 point = GetNearestPointOnGrid(pos);
                //Set tile size
                Vector2 c_Size = new Vector2(size, size);

                //Get a color
                #region Gizmo Color
                if (i == 0 && firstWhite)
                {
                    Gizmos.color = Color.black;
                    firstWhite = false;
                }
                else if (i == 0)
                {
                    Gizmos.color = Color.white;
                    firstWhite = true;
                }
                else if (Gizmos.color != Color.white)
                {
                    Gizmos.color = Color.white;
                }
                else
                {
                    Gizmos.color = Color.black;
                }
                #endregion

                //Finally, create a cube
                Gizmos.DrawCube(point, c_Size);

                //Add size in Y
                pos += (Vector2)transform.TransformDirection(Vector2.up) * size;
            }
            //Add size in Y
            pos += (Vector2)transform.TransformDirection(Vector2.right) * size;
        }
    }
}

//This class is to set the grid parameters
[System.Serializable]
public class CreateGrid
{
    public int numX;//How many tile
    public int startY;//Where it start
}

public class GridSlot
{
    Grid grid;//Get a grid where it belongs

    public Character character { get; private set; } //The character that's on top of me
    public Vector2 position { get; private set; }//My position
    public Vector2Int matrix { get; private set; }//My positions on matrix
    public SpriteRenderer sprite { get; private set; }//My sprite

    //My Neighbors
    public GridSlot upNeighbor, downNeighbor, rightNeighbor, leftNeighbor;
    #region CheckNeighbors
    //If I have a Up Neighbor
    public bool haveUp
    {
        get
        {
            return upNeighbor != null;
        }
    }
    //If I have a Down Neighbor
    public bool haveDown
    {
        get
        {
            return downNeighbor != null;
        }
    }
    //If I have a Rigth Neighbor
    public bool haveRight
    {
        get
        {
            return rightNeighbor != null;
        }
    }
    //If I have a Left Neighbor
    public bool haveLeft
    {
        get
        {
            return leftNeighbor != null;
        }
    }
    #endregion

    /*For the AStar algoritm, will store what grid it previously came from so it can trace
      the shortest path.*/
    public GridSlot parentGrid;

    public int igCost;//The cost of moving to the next square.
    public int ihCost;//The distance to the goal from this node.

    /*Quick get function to add G cost and H Cost, and since we'll never need to edit FCost,
      we dont need a set function.*/
    public int fCost { get { return igCost + ihCost; } }

    //Constructor, we need receive a position, a matrix position and the grid
    public GridSlot(Vector2 pos, Vector2Int matrix, Grid grid)
    {
        //Use the parameters the we receive
        position = pos;
        this.grid = grid;
        this.matrix = matrix;

        //Create a Sprite
        #region Sprite
        //Create object, that contains Sprite Renderer
        sprite = new GameObject("Grid" + matrix.ToString()).AddComponent<SpriteRenderer>();
        //Set as child of Grid
        sprite.transform.SetParent(grid.transform);
        //Set position
        sprite.transform.position = pos;
        //Set the scale by grid size
        sprite.transform.localScale = (Vector2.one * grid.size);
        //Set the sprite by frid sprite
        sprite.sprite = grid.sprite;
        #endregion
    }

    public void ReceiveCharacter(Character character)
    {
        /*
		character = characterConfig.gameObject.AddComponent<Character>();
        character.transform.SetParent(grid.room.h_Parent);
        character.transform.position = position;
        character.transform.localEulerAngles = Vector3.up * 180;
        character.InitCharacter(grid.room);

        string room = grid.room.room.ToString();
		string c_Name = character.characterConfig.characterName;
        string slotName = room + "Slot" + index;
        PlayerPrefs.SetString(slotName, c_Name);

        character.gameObject.AddComponent<Player>().slotName = slotName;
        */
    }
}