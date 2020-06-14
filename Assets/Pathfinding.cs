using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;

public class Pathfinding : MonoBehaviour
{
    Grid grid;                               //The grid we're doing the pathfinding on. 
    PathRequestManager requestManager;


    private void Awake()
    {
        grid = GetComponent<Grid>();
        requestManager = GetComponent<PathRequestManager>();
    }

    public void StartFindPath(Vector3 startPosition, Vector3 targetPosition)
    {
        StartCoroutine(FindPath(startPosition, targetPosition));
    }

    IEnumerator FindPath (Vector3 startPosition, Vector3 targetPosition)
    {
        Stopwatch sw = new Stopwatch(); //Performance check.
        sw.Start();

        Vector3[] waypoints = new Vector3[0];  //Waypoints that the unit must traverse to get to the end destination. It starts with 1 index with is the start position. 
        bool pathSuccess = false; 

        Node startNode = grid.NodeFromWorldPoint(startPosition);        //Convert the starting object's world position into its corresponding node found in the grid.
        Node targetNode = grid.NodeFromWorldPoint(targetPosition);      //Convert the starting object's world position into its corresponding node found in the grid.


        if (startNode.walkable && targetNode.walkable)    //We make sure that the nodes are walkable in the first place, else there's no chance of finding a path.
        {
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);             //The list of nodes that are yet to be evaluated.
            HashSet<Node> closedSet = new HashSet<Node>();     //The list of nodes that have been searched and thus closed. 
            openSet.Add(startNode);                            //Add the starting node into the open set as everything begins from there.

            while (openSet.Count > 0)                          //As long as the open set is not empty and still have things to be evaluated.
            {
                Node currentNode = openSet.RemoveFirst();                 //First node will be equal to the first element in the open set. 

                closedSet.Add(currentNode);           //We add the new node to the closed set as it has been evaluated. 

                if (currentNode == targetNode)        //If we have found our target node, we return from the loop. Complete! 
                {
                    sw.Stop();
                    print("Path Found: " + sw.ElapsedMilliseconds + " ms");

                    pathSuccess = true;
                    break;
                }


                foreach (Node neighbour in grid.GetNeighbours(currentNode))   //For each neighbouring node that was found relative to the current node.
                {
                    if (neighbour.walkable != true || closedSet.Contains(neighbour))  //If it cannot be walked on or if the neighbouring node has already been evaluated, we skip it. 
                    {
                        continue;
                    }
                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.movementPenalty;  //Current node's distance from the start node + current node's distance to the neighbour node + the neighbour node's movement penalty. 
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))  //If the new movement cost is less than the neighbour node's G cost or if the neighbour isn't in the open set
                    {
                        neighbour.gCost = newMovementCostToNeighbour;  //New gCost.
                        neighbour.hCost = GetDistance(neighbour, targetNode);  //New hCost.
                        neighbour.parent = currentNode;  //Set new parent to the current node. 

                        if (!openSet.Contains(neighbour))  //If the neighbour node isn't in the open set,
                        {
                            openSet.Add(neighbour);   //We add it in. 
                        }
                        else
                        {
                            openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }
        }
        yield return null;
        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);   //Retrace the path we took from the start node to the end node. 
        }
        requestManager.FinishedProcessingPath(waypoints, pathSuccess);  //Path processing complete. 
    }

    Vector3[] RetracePath(Node startNode, Node endNode)     //Retrace the path we took from the start node to the end node. 
    {
        List<Node> path = new List<Node>();  //The path we took to get to get to the end. 
        Node currentNode = endNode;   //We do this as when we do finish tracing the path, the current node would be the end node itself. 
        while (currentNode != startNode) //While the current node is not yet equal to the starting node...
        {
            path.Add(currentNode);  //We add the node to the path list. 
            currentNode = currentNode.parent; //We set the current node to be equal to the parent of the node we just added to the list, which wil automatically trace back to the start. 
        }
        Vector3[] waypoints = SimplifyPath(path);  //Find all movement waypoints the unit has to take. 
        Array.Reverse(waypoints); //Once we're done, note that the list is in reverse. So we reverse it the right way round. 
        return waypoints;
    }

    Vector3[] SimplifyPath(List<Node> path) //Waypoints that are created everytime there's a change in trajectories.
    {
        List<Vector3> waypoints = new List<Vector3>();     //All created waypoints.
        Vector2 directionOld = Vector2.zero;               //The old direction the unit was taking.

        for (int i = 1; i < path.Count; i++)               //For each node that was found in the path. We use 1 because 0 is the starting position.
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);  //We calculate the movement directions here. 
            if (directionOld != directionNew)              //If the old direction and new direction isn't the same.
            {
                waypoints.Add(path[i].worldPosition);      //We add a new waypoint to the list.
            }
            directionOld = directionNew;                   //Then, we set the old direction to be equal to the new direction, and the loop repeats.
        }
        return waypoints.ToArray();   //Once all waypoints are done, we complete the list and turn it to an array to be returned. 
    }

    int GetDistance(Node nodeA, Node nodeB)           //Gets the distance between 2 any given nodes. 
    {
        int distanceX = Math.Abs(nodeA.gridX - nodeB.gridX);    //Gets the absolute value in terms of distances between NodeA and NodeB on the X Axis. 
        int distanceY = Math.Abs(nodeA.gridY - nodeB.gridY);    //Gets the absolute value in terms of distances between NodeA and NodeB on the Y Axis. 

        if (distanceX > distanceY)    
        {
            return 14 * distanceY + 10 * (distanceX - distanceY);
        }

        return 14 * distanceX + 10 * (distanceY - distanceX); 
        //Remember that costs for diagonal movement is 14, and adjacent movements are 10. 
        //Take for example an End Node that is 5 on the X-axis away from the start, and 2 on the Y axis away on the Y. 
        //We will always take the smaller number, in this case 2 on the Y axis and move it diagonally by 2 units from the start axis. If it is the X axis that is smaller, we move it horizontally.
        //To calculate how much moves we need in this case, horizontally, we minus the lower number from the higher number. In this case, we get 3. If it was the X axis that is smaller, we move it by 3 diagonally.
        //So by moving 2 diagonally and 3 horizontally, we moved from the start node to the end node.
        //The equation for this would be 14y + 10(x - y) which is only in the case where X is greater than Y, else if Y is greater than X, which is 14x + 10(y - x)
    }
}
