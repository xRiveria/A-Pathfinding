using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Pathfinding : MonoBehaviour
{
    public Transform seeker, target;         //Who's doing the pathfinding, and what is it's target?
    Grid grid;                               //The grid we're doing the pathfinding on. 

    private void Awake()
    {
        grid = GetComponent<Grid>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            FindPath(seeker.position, target.position);
        }
    }

    void FindPath (Vector3 startPosition, Vector3 targetPosition)
    {
        Stopwatch sw = new Stopwatch(); //Performance check.
        sw.Start();

        Node startNode = grid.NodeFromWorldPoint(startPosition);        //Convert the starting object's world position into its corresponding node found in the grid.
        Node targetNode = grid.NodeFromWorldPoint(targetPosition);      //Convert the starting object's world position into its corresponding node found in the grid.

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

                RetracePath(startNode, targetNode);   //Retrace the path we took from the start node to the end node. 
                return;
            }


            foreach (Node neighbour in grid.GetNeighbours(currentNode))   //For each neighbouring node that was found relative to the current node.
            {
                if (neighbour.walkable != true || closedSet.Contains(neighbour))  //If it cannot be walked on or if the neighbouring node has already been evaluated, we skip it. 
                {
                    continue;
                }
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);  //Current node's distance from the start node + current node's distance to the neighbour node. 
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))  //If the new movement cost is less than the neighbour node's G cost or if the neighbour isn't in the open set
                {
                    neighbour.gCost = newMovementCostToNeighbour;  //New gCost.
                    neighbour.hCost = GetDistance(neighbour, targetNode);  //New hCost.
                    neighbour.parent = currentNode;  //Set new parent to the current node. 

                    if (!openSet.Contains(neighbour))  //If the neighbour node isn't in the open set,
                    {
                        openSet.Add(neighbour);   //We add it in. 
                    }
                }
            }
        }
    }

    void RetracePath(Node startNode, Node endNode)     //Retrace the path we took from the start node to the end node. 
    {
        List<Node> path = new List<Node>();  //The path we took to get to get to the end. 
        Node currentNode = endNode;   //We do this as when we do finish tracing the path, the current node would be the end node itself. 
        while (currentNode != startNode) //While the current node is not yet equal to the starting node...
        {
            path.Add(currentNode);  //We add the node to the path list. 
            currentNode = currentNode.parent; //We set the current node to be equal to the parent of the node we just added to the list, which wil automatically trace back to the start. 
        }
        path.Reverse(); //Once we're done, note that the list is in reverse. So we reverse it the right way round. 
        grid.path = path;

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
