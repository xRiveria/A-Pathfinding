using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node> 
{
    public bool walkable;                  //Whether this node is walkable or not.
    public Vector3 worldPosition;          //The node's position in the world.  

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
    public Node parent;                    //The "parent" of this node where the path is previously calculated from. 

    public int gridX;                      //Its position in the grid's X axis.
    public int gridY;                      //Its position in the grid's Y axis. 
    int heapIndex;


    public Node (bool _walkable, Vector3 _worldPosition, int _gridX, int _gridY)        //Node constructor called when building the game's grid.
    {
        walkable = _walkable;
        worldPosition = _worldPosition;
        gridX = _gridX;
        gridY = _gridY;
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value; 
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0) //aka Equal F Cost.
        {
            compare = hCost.CompareTo(nodeToCompare.hCost); //We use hCost as tiebreaker.
        }
        return -compare; //For integers, returning 1 would mean the integer has a higher priority. With our nodes, that is reversed. We want to return 1 if its lower.  
    }
}
