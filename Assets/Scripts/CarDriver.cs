using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarDriver : Agent
{
    [Header("Agent stats")]
    [SerializeField] private float rewards = 0;
    public bool inCheckpoint = false;
    public float waitTimer = 0;

    private TrackCheckpoints trackCheckpoints;
    [SerializeField] private Vector3 spawnPosition;
    private PrometeoCarController controller;
    [SerializeField] private Transform spawn;
    [SerializeField] private LayerMask carsMask;

    private void Awake()
    {
        controller = GetComponent<PrometeoCarController>();
        trackCheckpoints = GetComponent<TrackCheckpoints>();
    }

    // Start is called before the first frame update
    void Start()
    {
        spawn = transform.GetChild(3);
        spawn.transform.SetParent(spawn.transform.root);
        spawnPosition = transform.position;
        spawn.position = spawnPosition;
    }

    private void Update()
    {
        rewards = GetCumulativeReward();
    }

    public override void OnEpisodeBegin()
    {
        controller.maxSpeed = Random.Range(20, 30);
        transform.position = spawn.transform.position;
        transform.forward = spawn.forward;
        trackCheckpoints.ResetCheckpoints();
        trackCheckpoints.checkpointsReached = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(trackCheckpoints.PointDirDot);
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

    public void OnCorrectCheckpoint()
    {
        AddReward(+1f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Agent"))
        {
            AddReward(-1.5f);
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-1.25f);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Agent"))
        {
            AddReward(-0.05f);
            waitTimer += Time.deltaTime;
            if (waitTimer > 3)
            {
                waitTimer = 0;
                EndEpisode();
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(+0.025f);
            waitTimer = 0;
        }

        if (collision.gameObject.CompareTag("Agent"))
        {
            AddReward(+0.025f);
            waitTimer = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            //OnCorrectCheckpoint reward if the checkpoint has not been visited 
            trackCheckpoints.CheckpointComplete(other.GetComponent<Checkpoint>());
            inCheckpoint = true;
        }
        else if (other.CompareTag("WrongDirection") && !trackCheckpoints.wrongDirection.Contains(other.GetComponent<WrongDirection>())) 
        { 
            WrongDirection direction = other.transform.GetComponent<WrongDirection>();
            trackCheckpoints.StartCoroutine(trackCheckpoints.DisablePoint(direction));
            trackCheckpoints.directionPenalties++;
            AddReward(-5f);
        }

        if (other.CompareTag("LaneMarking"))
        {
            AddReward(-0.1f);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("LaneMarking"))
        {
            AddReward(-0.015f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            inCheckpoint = false;
        }
    }
}
