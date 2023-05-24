using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarDriver : Agent
{
    private TrackCheckpoints trackCheckpoints;
    [SerializeField] private Transform spawnPosition;

    private PrometeoCarController controller;

    private void Awake()
    {
        controller = GetComponent<PrometeoCarController>();
        trackCheckpoints = GetComponent<TrackCheckpoints>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    public void TrackCheckpoints_OnCorrectCheckpoint()
    {
        AddReward(+10f);
    }

    public void TrackCheckpoints_OnWrongCheckpoint()
    {
        AddReward(-1f);
    }

    public override void OnEpisodeBegin()
    {
        transform.position = spawnPosition.position + new Vector3(Random.Range(-4f, +4f), 0, Random.Range(-4f, +4f));
        transform.forward = spawnPosition.forward;
        trackCheckpoints.ResetCheckpoints();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 checkpointForward = trackCheckpoints.GetNextCheckpoint().forward;
        float directionDot = Vector3.Dot(transform.forward, checkpointForward);
        sensor.AddObservation(directionDot);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float forwardAmount = 0f;
        float turnAmount = 0f;

        switch (actions.DiscreteActions[0])
        {
            case 0: forwardAmount = 0f; break;
            case 1: forwardAmount = +1f; break;
            case 2: forwardAmount = -1f; break;
        }
        switch (actions.DiscreteActions[1])
        {
            case 0: turnAmount = 0f; break;
            case 1: turnAmount = +1f; break;
            case 2: turnAmount = -1f; break;
        }

        controller.SetInput(forwardAmount, turnAmount);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int forwardAction = 0;
        if (Input.GetKey(KeyCode.W)) forwardAction = 1;
        if (Input.GetKey(KeyCode.S)) forwardAction = 2;

        int turnAction = 0;
        if (Input.GetKey(KeyCode.D)) turnAction = 1;
        if (Input.GetKey(KeyCode.A)) turnAction = 2;

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = forwardAction;
        discreteActions[1] = turnAction;
    }

    /*    private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Wall"))
            {
                AddReward(-10f);
                EndEpisode();
            }
        } */

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Agent"))
        {
            Debug.Log("Vehicle hit!");
            AddReward(-20f);
        }
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Wall Hit!");
            AddReward(-10f);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Wall stay!");
            AddReward(-5f);
        }
        if (collision.gameObject.CompareTag("Agent"))
        {
            Debug.Log("Agent stay!");
            AddReward(-10f);
        }
    }
}
