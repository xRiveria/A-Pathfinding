using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//If we have a system where units all request paths at the same time, the game might freeze. What we want to do is create a system where units request paths across different frames. 
public class PathRequestManager : MonoBehaviour
{
    Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();       //Queues all path requests from the game.
    PathRequest currentPathRequest;                                       //The current path request being worked on by this system.

    static PathRequestManager instance;
    Pathfinding pathfinding;
    bool isProcessingPath;    //Whether the system is processing any paths at the moment. 

    private void Awake()
    {
        instance = this;
        pathfinding = GetComponent<Pathfinding>();
    }

    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback) //We store a callback here so that we may call the move action on the unit once a path has been successfully requested. 
    {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);   //A new path has been requested. We thus create a new path request.
        instance.pathRequestQueue.Enqueue(newRequest); //We queue the path within the Queue in the system. 
        instance.TryProcessNext();  //And we try processing the queue in this function. 
    }

    void TryProcessNext()
    {
        if (!isProcessingPath && pathRequestQueue.Count > 0)    //In here, we make sure that no path processing is taking place and that there are items in the queue. 
        {
            currentPathRequest = pathRequestQueue.Dequeue();  //If not, we set the current path request to the first item in the queue and remove it from queue. 
            isProcessingPath = true;  //We set that the system is currently requesting a path. 
            pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);  //We start finding a path from the given path request's start to end nodes. 

        }
    }

    public void FinishedProcessingPath(Vector3[] path, bool success)
    {
        currentPathRequest.callback(path, success);  //Once we're done processing the path, we call the OnPathFound that was given as a callback, and the unit now begins moving.
        isProcessingPath = false;  //We set that processing is complete.
        TryProcessNext();  //We begin to try processing more items in the queue. 
    }

    struct PathRequest       //A data struct for storing all information about a path request that can be sent by the game's units. 
    {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action<Vector3[], bool> callback;

        public PathRequest(Vector3 _pathStart, Vector3 _pathEnd, Action<Vector3[], bool> _callback)
        {
            pathStart = _pathStart;
            pathEnd = _pathEnd;
            callback = _callback;
        }
    }
}
