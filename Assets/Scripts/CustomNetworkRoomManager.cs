using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class CustomNetworkRoomManager : NetworkRoomManager
{
    private List<Observer> observers = new List<Observer>();
    private List<int> usedStartPositions = new List<int>();
    public CustomPlayer player;
    
    public override Transform GetStartPosition()
    {
        startPositions.RemoveAll(t => t == null);
    
        if (startPositions.Count == 0)
            return null;
    
        if (playerSpawnMethod == PlayerSpawnMethod.Random)
        {
            if (WasAllStartPositionUsed())
                ClearUsedStartPositions();
            
            int index;
            do
            {
                index = UnityEngine.Random.Range(0, startPositions.Count);
            } while (WasStartPositionUsed(index));
            
            AddStartPositionInUsedStartPositions(index);
            return startPositions[index];
        }
        else
        {
            Transform startPosition = startPositions[startPositionIndex];
            startPositionIndex = (startPositionIndex + 1) % startPositions.Count;
            return startPosition;
        }
    }

    private bool WasStartPositionUsed(int index)
    {
        return usedStartPositions.Contains(index);
    }

    private void AddStartPositionInUsedStartPositions(int index)
    {
        usedStartPositions.Add(index);
    }

    private bool WasAllStartPositionUsed()
    {
        return usedStartPositions.Count == startPositions.Count;
    }
    
    private void ClearUsedStartPositions()
    {
        usedStartPositions.Clear();
    }
}