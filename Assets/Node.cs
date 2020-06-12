using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node 
{
    public bool walkable;                  //Whether this node is walkable or not.
    public Vector3 worldPosition;           

    [Header("A* Node Properties")]
    public int gCost;                      //How far away is this node from the start node.
    public int hCost;                      //How far away is this node from the end node.
    public int fCost                       //GCost + HCost. A* will choose the one with the lowest fCost to look at (which means its the closest to the target) and recalculate all surrounding nodes again. 
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int gridX;
    public int gridY;
    public Node parent;


    public Node (bool _walkable, Vector3 _worldPosition, int _gridX, int _gridY)        //Node constructor called when building the game's grid.
    {
        walkable = _walkable;
        worldPosition = _worldPosition;
        gridX = _gridX;
        gridY = _gridY;
    }
}
