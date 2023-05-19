using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private TrackCheckpoints trackCheckpoints;
    private void OnTriggerEnter(Collider other)
    {
        trackCheckpoints = other.GetComponent<TrackCheckpoints>();
        if(other.CompareTag("Agent")){
            trackCheckpoints.CheckpointComplete(this);
        }
    }

}
