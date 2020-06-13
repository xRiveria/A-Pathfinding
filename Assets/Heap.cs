using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//When we think of a Heap, think of a tree where a Node has at least two child nodes. 
//Each of those nodes have their own 2 child notes and so on.
//There's one very simple rule: Each parent node's priority must be lower than both of its child nodes. 
//We swap values around the tree so that the rule above is always fulfilled whenever a new node is added to the tree.
//When we want to remove an item from the Heap with the lowest FCost, we remove that node from the Heap and take the node from the end of the Heap and put it at the start.
//Once that's done, we compare it with its two children and if its greater than either of them, we swap it with the lowest one. And this continues until it finds its place.
//This is very performance saving as once we compare the parent node with either of its children and move to either side, we cut out calculation for the other side entirely. 
//So how do we tell the computer about each node's priority values?

//Parent Node Formula: (n-1)/2 
//Child Left Formula: (2n + 1)
//Child Right Formula: (2n + 2)



public class Heap<T> where T : IHeapItem<T> //Let IHeapItem inherit Heap. 
{
    T[] items;
    int currentItemCount;      //The amount of items in the Heap. 

    public Heap(int maxHeapSize)     //The maximum size of the heap. In the case of our pathfinding, we can multiple gridSizeX by gridSizeY to get the max amount of nodes in the heap at any given point. 
    {
        items = new T[maxHeapSize];
    }

    public void Add(T item)  
    {
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;   //Add it to the end of the item array. Theres not nessarilly where it belongs, and thus we need to keep comparing it. 
        SortUp(item);
        currentItemCount++;
    }

    public void SortUp(T item)  //We compare this with the parent and sort it up if it has a higher priority than the parent.
    {
        int parentIndex = (item.HeapIndex - 1) / 2;
        while (true)
        {
            T parentItem = items[parentIndex];
            if (item.CompareTo(parentItem) > 0) //CompareTo returns 1 if there's a higher priority, returns 0 if the priority is equal and returns -1 if there's a lesser priority. In this case, its the fCost. 
            {
                Swap(item, parentItem);
            }
            else 
            {
                break;
            }
            parentIndex = (item.HeapIndex - 1) / 2;  //Otherwise we keep calculating and comparing it with its new parent. 
        }
    }

    public void SortDown(T item) //The item in this case is the parent. We compare it with its children and swap if the parent has a lower priority than its higher priority child. 
    {
        while(true)
        {
            int childIndexLeft = item.HeapIndex * 2 + 1; //Get indices of the item's 2 children.
            int childIndexRight = item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            if (childIndexLeft < currentItemCount) //If this item has at least one child (the one on the left)
            {
                swapIndex = childIndexLeft;

                if (childIndexRight < currentItemCount) //If the item has a second child - the one on the right. 
                {
                    if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0) //We need to check which of the two children has a higher priority, and we will set the swap index accordingly. 
                    {
                        swapIndex = childIndexRight; //Else, it remains as childIndexLeft. 
                    }              
                }
                //Now that swap index is equal to the child with the higher priority, then we check if the parent has a lower priority than its higher priority child. If so, we swap them.
                if (item.CompareTo(items[swapIndex]) < 0)
                {
                    Swap(item, items[swapIndex]);
                }
                else
                {
                    return; 
                }
            }
            else
            {
                return; //If the parent does not have any children, we return as well. 
            }
        }
    }

    public T RemoveFirst()   //Sort down the list. 
    {
        T firstItem = items[0];      //Grab a copy of the first item in our Heap. 
        currentItemCount--;          //Remove one item from the Heap. 
        items[0] = items[currentItemCount];  //Take the item at the end of the heap and put it at the first place. 
        items[0].HeapIndex = 0;     //Set its Heap index to 0 since its now the first item in the Heap.
        SortDown(items[0]);
        return firstItem;
    
    }

    public bool Contains (T item) //Check if the Heap contains a specific item.
    {
        return Equals(items[item.HeapIndex], item);
    }

    public int Count //Get amount of items in the Heap.
    {
        get
        {
            return currentItemCount;
        }
    }

    public void UpdateItem(T item) //Update positions in the heap. In our pathfinding, it would be because we found a new path relative to the parent, thus changing its priority/F Cost. 
    {
        SortUp(item); //We will only ever increase priority in Pathfinding, not decrease it. So SortUp is enough. 
    }

    void Swap (T itemA, T itemB)
    {
        items[itemA.HeapIndex] = itemB; //Swaps the items with one another.
        items[itemB.HeapIndex] = itemA;

        int itemAIndex = itemA.HeapIndex;  //Temporary index before we swap them below. 
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAIndex; 
    }
}

public interface IHeapItem<T> : IComparable<T> 
 //We need each item to keep track of its own index in the Heap, and we also need to compare them to see which one has the higher priority. 
 //As we're using a Generic, we are not capable of doing any of that. So, we can use an Interface to do so.
{
    int HeapIndex
    {
        get;
        set;
    }
}




//Refer: en.wikipedia.org/wiki/Heap_(data_structure)