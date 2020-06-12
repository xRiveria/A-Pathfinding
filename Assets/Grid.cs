using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Grid : MonoBehaviour
{
    public Transform player;             //The player, or rather the starting node. 
    public Vector2 gridWorldSize;        //The size of the entire grid which the A* agent will calculate on. 
    public float nodeRadius;             //The radius each individual node covers. 
    public LayerMask unwalkableMask;     //Unwalkable places or nodes. 

    Node[,] grid;                        



    float nodeDiameter;                  //The full size of a node. 
    int gridSizeX, gridSizeY;            //The amount of nodes that can fit into the grid both X and Y positioning wise. 

    private void Start()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);   //Checks the amount of nodes that can fit into the grid X wise. We round to int as we can't have like half a node etc. 
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);   //Checks the amount of nodes that can fit into the grid Y wise. We round to int as we can't have like half a node etc. 
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];   //A brand new grid creation of given X and Y nodes. 
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }    

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }

    public List<Node> path;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
        if (grid != null)
        {
            Node playerNode = NodeFromWorldPoint(player.position);
            foreach (Node node in grid)
            {
                Gizmos.color = (node.walkable) ? Color.white : Color.red;
                if (playerNode == node)
                {
                    Gizmos.color = Color.cyan;
                }
                if (path != null)
                {
                    if (path.Contains(node))
                    {
                        Gizmos.color = Color.black;
                    }
                }    
                Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter - .1f));
            }
        }
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x, y];
    }
}
