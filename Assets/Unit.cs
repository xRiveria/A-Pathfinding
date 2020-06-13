using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public Transform target;    //The target to move towards.
    float speed = 20f;          //The speed of the current unit.
    Vector3[] path;             //The paths to be taken to reach the target. Think of this as waypoints that are marked along the path.
    int targetIndex;            //The current index of the path that the unit is moving towards. 

    private void Start()
    {
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);        //At the start of the game, we request a path from system for the unit to take. We pass in a OnPathFound callback so that we can inform the unit to move when its ready. 
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)      //Once a path has been found.
    {
        if (pathSuccessful)                      //Check again to make sure that the path is successful.
        {
            path = newPath;                      //If it is, we set this unit's path to the successfully requested path. 
            StopCoroutine("FollowPath");         //In case there was previously a coroutine that was running, we stop it first.
            StartCoroutine("FollowPath");        //Start running the FollowPath coroutine. 
        }
    }

    IEnumerator FollowPath()                    //Follow the path calculated and move the unit. 
    {
        Vector3 currentWaypoint = path[0];      //Set the current waypoint to be the first waypoint found in the path. 
        while (true) 
        {
            if (transform.position == currentWaypoint)     //If the unit's current position is equal to the current waypoint's position.
            {
                targetIndex++;                             //We increment the current index of the unit. 
                if (targetIndex >= path.Length)            //If the current index of the unit is more than or equal to the path's total waypoints, we end the loop and break it. 
                {
                    yield break;
                }
                currentWaypoint = path[targetIndex];       //Else, we set the current waypoint to the next waypoint in the path. 
            }
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);     //Moves the unit to the next waypoint found in the path. 
            yield return null;  //Since we're in a coroutine, we use this to move to the next frame. 
        }
    }

    public void OnDrawGizmos()     //Draws the path out in the world.
    {
        if (path != null)
        {
            for (int i = targetIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], Vector3.one);

                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);   //Draw a line at every waypoint on the map.
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }
}
