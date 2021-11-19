using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    GameController gameController
    {
        get
        {
            return GameController.instance;
        }
    }

    Grid grid
    {
        get
        {
            return gameController.grid;
        }
    }

    public List<GridSlot> FindPath(Vector3 a_StartPos, Vector3 a_TargetPos)
    {
        GridSlot start = grid.GetGridSlot(a_StartPos);//Gets the grid slot closest to the starting position
        GridSlot target = grid.GetGridSlot(a_TargetPos);//Gets the grid slot closest to the target position

        List<GridSlot> openList = new List<GridSlot>();//List of grid slot for the open list
        HashSet<GridSlot> closedList = new HashSet<GridSlot>();//Hashset of grid slot for the closed list

        //Add the starting grid slot to the open list to begin the program
        openList.Add(start);

        //While there is something in the open list
        while (openList.Count > 0)
        {
            //Create a node and set it to the first item in the open list
            GridSlot currentGrid = openList[0];

            //Loop through the open list starting from the second object
            for (int i = 1; i < openList.Count; i++)
            {
                //If the f cost of that object is less than or equal to the f cost of the current grid
                if (openList[i].fCost < currentGrid.fCost 
                ||  openList[i].fCost == currentGrid.fCost 
                &&  openList[i].ihCost < currentGrid.ihCost)
                {
                    //Set the current grid to that object
                    currentGrid = openList[i];
                }
            }

            //Remove that from the open list and add it to the closed list
            openList.Remove(currentGrid);
            closedList.Add(currentGrid);

            //If the current grid is the same as the target grid
            if (currentGrid == target)
            {
                //Calculate the final path
                return GetFinalPath(start, target);
            }

            //Verify all neighbors
            for (int i = 0; i < 4; i++)
            {
                GridSlot neighbor = currentGrid.GetNeighborsByIndex(i);

                //If the neighbor do not exists or has already been checked
                if (neighbor == null || closedList.Contains(neighbor))
                {
                    continue;
                }

                //Get the F cost of that neighbor
                int MoveCost = currentGrid.igCost + GetManhattenDistance(currentGrid, neighbor);

                if (MoveCost < neighbor.igCost || !openList.Contains(neighbor))
                {
                    //Set the g cost to the f cost
                    neighbor.igCost = MoveCost;
                    //Set the h cost
                    neighbor.ihCost = GetManhattenDistance(neighbor, target);
                    //Set the parent of the node for retracing steps
                    neighbor.parentGrid = currentGrid;

                    //If the neighbor is not in the openlist
                    if (!openList.Contains(neighbor))
                    {
                        //Add it to the list
                        openList.Add(neighbor);
                    }
                }
            }
        }

        Debug.LogWarning("Is was not possible to find a Path");
        return null;
    }

    List<GridSlot> GetFinalPath(GridSlot start, GridSlot end)
    {
        List<GridSlot> finalPath = new List<GridSlot>();//List to hold the path sequentially 
        GridSlot currentGrid = end;//Node to store the current node being checked

        //While loop to work through each node going through the parents to the beginning of the path
        while (currentGrid != start)
        {
            currentGrid.sprite.color = Color.yellow;
            //Add that node to the final path
            finalPath.Add(currentGrid);
            //Move onto its parent node
            currentGrid = currentGrid.parentGrid;
        }

        //Reverse the path to get the correct order
        finalPath.Reverse();
        return finalPath;
    }

    int GetManhattenDistance(GridSlot gridA, GridSlot gridB)
    {
        int ix = Mathf.Abs(gridA.matrix.x - gridB.matrix.x);//x1-x2
        int iy = Mathf.Abs(gridA.matrix.y - gridB.matrix.y);//y1-y2

        return ix + iy;//Return the sum
    }
}
