using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class Grid : MonoBehaviour
{
    public bool disaplyGridGizmos;
    public Transform player;             //The player, or rather the starting node. 
    public Vector2 gridWorldSize;        //The size of the entire grid which the A* agent will calculate on. 
    public float nodeRadius;             //The radius each individual node covers. 
    public LayerMask unwalkableMask;     //Unwalkable places or nodes. 
    public LayerMask walkableMask;       //Walkable places.
    public TerrainType[] walkableRegions;
    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();  //Tracks all layers with movement penalties and its associated cost.
    int penaltyMin = int.MaxValue;
    int penaltyMax = int.MinValue;
    public int obstacleProximityPenalty = 10; //Penalty for being too close to obstacle/Unwalkables.


    Node[,] grid;                        //New Grid consisting of a X and Y.          

    float nodeDiameter;                  //The full size of a node. 
    int gridSizeX, gridSizeY;            //The amount of nodes that can fit into the grid both X and Y positioning wise. 

    private void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);   //Checks the amount of nodes that can fit into the grid X wise. We round to int as we can't have like half a node etc. 
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);   //Checks the amount of nodes that can fit into the grid Y wise. We round to int as we can't have like half a node etc. 

        foreach(TerrainType region in walkableRegions)
        {
            walkableMask.value += region.terrainMask.value; //Adds all layers from the WalkableRegions array to the WalkableMask.
            walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);   //Adds the found layer's layer value and its terrain penalty to the Dictionary for use later.
            //Each layer is stored in a 32bit integer. If Grass is in the 10th layer, Unity will find it with 2*10 which will be 1024. Thus, to retrieve this value, we use Mathf.Log with a Power of 2, that returns its respective value in the layer.
        
        }
        CreateGrid();
    }

    public int MaxSize   //Max size of the grid.
    {
        get { return gridSizeX * gridSizeY; }
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];   //A brand new grid creation of given amount of X and Y nodes. 
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2; //Gets the bottom left corner of the Grid.

        for (int x = 0; x < gridSizeX; x++)        //For each node in X...
        {
            for (int y = 0; y < gridSizeY; y++)    //For each node in Y...
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter); //As each node is looped, we increment accordingly. 
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));   //Checks if the node collides with a unwalkable mask. 
                int movementPenalty = 0;
                //Raycast to find walkable layers will terrain penalties.

                Ray ray = new Ray(worldPoint + Vector3.up * 450, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 500, walkableMask))
                {
                    walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);   //Trys to get the value of each layers and measure it against the dictionary. Once found, retrieves the movement penalty.
                }
                if (!walkable)
                {
                    movementPenalty += obstacleProximityPenalty;  //Add a obstacle proximity penalty for getting too close to unwalkable obstacles. 
                }

                grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);  //Actually populate the grid with nodes now that had been created in the world with values such as movement penalty, X and Y location etc. 
            }
        }
        BlurPenaltyMap(3);
    }    

    void BlurPenaltyMap(int blurSize) //Blur map algorhitm.
    {
        int kernalSize = blurSize * 2 + 1;     //Box check size. We must make sure this is an odd number with a box in center.
        int kernalExtents = (kernalSize - 1) / 2;  //How many squares between the center square to the edge of the kernel.

        int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY]; //Temporary grids to store horizontal and vertical pass values;
        int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];
        for (int y = 0; y < gridSizeY; y++)  //Row calculation. We have to loop through all the nodes in the Kernel only for the first loop, while subsequently we can use the number found and minimize calculations.
        {
            for (int x = -kernalExtents; x <= kernalExtents; x++) 
            {
                int sampleX = Mathf.Clamp(x, 0, kernalExtents); //We want to clamp X to 0 when X is negative so that it will take the value from the first node instead of going out of bounds, up till the max kernal extend.
                penaltiesHorizontalPass[0, y] += grid[sampleX, y].movementPenalty; //Add the movement penalty from SampleX, Coloumn Y to the horizontal pass at Column 0, Row Y.                  
            }
            for (int x = 1; x < gridSizeX; x++) //We start at 1 since we already calculated for Column 0 above. We are now calculating for all remaining columns. 
            {
                int removeIndex = Mathf.Clamp(x - kernalExtents - 1, 0, gridSizeX); //Calculate index of the node that is no longer inside the kernal when the kernal shifts along. We once again clamp so it doesn't fall below 0 and doesn't go above maximum. 
                int addIndex = Mathf.Clamp(x + kernalExtents, 0, gridSizeX - 1); //Calcuate index of the node that has been added to the kernal when the kernal shifts along. 

                //Previous Sum - Value on the Left (Remove Index) + Value on the Right (Add Index)
                penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty;
            }
        }

        //Repeat above but for columns calculation now. 
        for (int x = 0; x < gridSizeX; x++)  //Column calculation. We have to loop through all the nodes in the Kernel only for the first loop, while subsequently we can use the number found and minimize calculations.
        {
            for (int y = -kernalExtents; y <= kernalExtents; y++)
            {
                int sampleY = Mathf.Clamp(x, 0, kernalExtents); 
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];                
            }

            int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernalSize * kernalSize)); //Get final blurred penalty for each node. Integer always rounds down, so to be more accurate we will just round to the nearest integer by using Mathf.RoundToInt.
            grid[x, 0].movementPenalty = blurredPenalty;

            for (int y = 1; y < gridSizeY; y++)  
            {
                int removeIndex = Mathf.Clamp(y - kernalExtents - 1, 0, gridSizeY); 
                int addIndex = Mathf.Clamp(y + kernalExtents, 0, gridSizeY - 1); 

                //Previous Sum - Value on the Left (Remove Index) + Value on the Right (Add Index)
                penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
                blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernalSize * kernalSize)); //Get final blurred penalty for each node. Integer always rounds down, so to be more accurate we will just round to the nearest integer by using Mathf.RoundToInt.
                grid[x, y].movementPenalty = blurredPenalty;

                if (blurredPenalty > penaltyMax)  //For visualization.
                {
                    penaltyMax = blurredPenalty;
                }
                if (blurredPenalty < penaltyMin)
                {
                    penaltyMin = blurredPenalty;
                }
            }
        }

    }

    public List<Node> GetNeighbours(Node node)  //We can't use arrays as we don't know how many nodes are neighbours. Normally it would be 8, but if its in the corner, it's going to change. 
    {
        List<Node> neighbours = new List<Node>();      //Create a list of nodes for the node's neighbours. 
        for (int x = -1; x <= 1; x++)                  //Searches in a 3 by 3 block.
        {
            for (int y = -1; y <= 1; y++)      
            {
                if (x == 0 && y == 0)                  //If the algoritm searches relative to the node's position, we found the node that was give to us if the index is 0 and so we skip it. 
                {
                    continue;
                }
                int checkX = node.gridX + x;           //Get the neighbour's position in the grid. 
                int checkY = node.gridY + y;          

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)   //Checks if the node is inside the grid. 
                {
                    neighbours.Add(grid[checkX, checkY]);   //If everything fits, we add this node to the list of neighbouring nodes. 
                }
            }
        }
        return neighbours;
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null && disaplyGridGizmos)
        {
            foreach (Node node in grid)
            {
                Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, node.movementPenalty)); //Fade from white to black depending on the weight.
                Gizmos.color = (node.walkable) ? Gizmos.color : Color.red;   //If the node is walkable, it stays its original color, else its red.
                Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeDiameter)); //Draws the nodes out in the Editor.
            }
        }

    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)    //Converts given world position to grid coordinate. 
    {
        //We will use a percent value to see how far into the grid the given position is.
        //For the X value for example, if we're on the far left, its 0, if its in the center, its 0.5, and far right would be 1. 

        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x; //If the world size is 30, and the world position given is -15, -15 + 15 / 30 = 0. (Far Left)
                                                                                    //If the world size is 30, and the world position given is 0, 0 + 15 / 30 = 0.5. (Center)
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y; //Note that we have to use worldPosition.z as it is a 3D space. Thus Y would be a Z axis. 

        percentX = Mathf.Clamp01(percentX);  //Make sure to clamp the values between 0 and 1 so that if somehow the node is outside of the grid for some reason, we don't get insane errors. 
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);  //Gets the actual index of the node found in the grid. Once again, we round to Integers because we're finding index values.  
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);  //We minus 1 from the gridSize because they're arrays which are 0 based. Say we're on the far right (thus percentY would be 1), gridSizeX (if 30 nodes in total) * percent Y of 1 would be 30 flat. We minus 1 to get index 29 in the array which is well, the 30th node counting its index 0.

        return grid[x, y];   //We return the grid index with the found X and Y. 
    }

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }
}
