using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TrackCheckpoints : MonoBehaviour
{

    private CarDriver agent;

    private List<Checkpoint> checkpointList;
    private int nextCheckpointSingleIndex;

    private void Awake()
    {
        agent = GetComponent<CarDriver>();

        Transform checkpointsTransform = GameObject.Find("Checkpoints").transform;

        checkpointList = new List<Checkpoint>();
        foreach (Transform checkpointTransform in checkpointsTransform)
        {
            Checkpoint checkpoint = checkpointTransform.GetComponent<Checkpoint>();

            checkpointList.Add(checkpoint);
        }

        nextCheckpointSingleIndex = 0;
    }

    public void CheckpointComplete(Checkpoint checkpoint)
    {
        if(checkpointList.IndexOf(checkpoint) == nextCheckpointSingleIndex)
        {
            Debug.Log(checkpointList.IndexOf(checkpoint));
            if((nextCheckpointSingleIndex + 1) % checkpointList.Count != 0)
                nextCheckpointSingleIndex = (nextCheckpointSingleIndex + 1) % checkpointList.Count;
            else{
                ResetCheckpoints();
                agent.EndEpisode();
            }

            agent.TrackCheckpoints_OnCorrectCheckpoint();
        }
        else
        {
            agent.TrackCheckpoints_OnWrongCheckpoint();
        }
    }

    public Transform GetNextCheckpoint()
    {

        return checkpointList[nextCheckpointSingleIndex].transform;
    }

    public void ResetCheckpoints()
    {
        nextCheckpointSingleIndex = 0;
    }
}
