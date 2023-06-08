using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TrackCheckpoints : MonoBehaviour
{
    [Header("Agent stats")]
    public int directionPenalties = 0;
    public int checkpointsReached = 0;
    public List<Checkpoint> previousCheckpoints;
    public List<WrongDirection> wrongDirection;
    public float PointDirDot;
    public Transform nextCheckpoint;

    [Header("Settings")]
    public LayerMask roadElements;
    public LayerMask checkpointMask;
    private CarDriver agent;
    private List<Checkpoint> checkpointList;

    private void Awake()
    {
        agent = GetComponent<CarDriver>();

        Transform checkpointsTransform = GameObject.Find("Checkpoints").transform;

        checkpointList = new List<Checkpoint>();
        wrongDirection = new List<WrongDirection>();
        previousCheckpoints = new List<Checkpoint>();
        foreach (Transform checkpointTransform in checkpointsTransform)
        {
            Checkpoint checkpoint = checkpointTransform.GetComponent<Checkpoint>();

            checkpointList.Add(checkpoint);
        }

    }

    private void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        Ray rayRight = new Ray(transform.position + (transform.rotation * new Vector3(0.1f, 0.5f, 4f)), transform.right.normalized);
        Ray rayLeft = new Ray(transform.position + (transform.rotation * new Vector3(-0.1f, 0.5f, 4f)), transform.right.normalized * -1);
        Debug.DrawRay(transform.position + (transform.rotation * new Vector3(0.1f, 0.5f, 4f)), transform.right.normalized*15, Color.yellow);
        Debug.DrawRay(transform.position + (transform.rotation * new Vector3(-0.1f, 0.5f, 4f)), (transform.right.normalized * -1)*15, Color.yellow);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 15, checkpointMask))
        {
            nextCheckpoint = hitInfo.transform;
            Vector3 checkpointForward = hitInfo.transform.forward;
            if (!previousCheckpoints.Contains(hitInfo.transform.GetComponent<Checkpoint>()))
                PointDirDot = Vector3.Dot(transform.forward, checkpointForward);
        }

        if(Physics.Raycast(rayRight, out RaycastHit hitRight, 10, roadElements))
        {
            if (hitRight.collider.CompareTag("Agent"))
            {
                agent.AddReward(-0.01f);
            }

            if (hitRight.collider.CompareTag("LaneMarking"))
            {
                float distance = Vector3.Distance(transform.position, hitRight.point);
                if (distance < 4)
                {
                    agent.AddReward(-0.01f);
                }
            }
        }

        if(Physics.Raycast(rayLeft, out RaycastHit hitLeft, 10, roadElements))
        {
            if (hitLeft.collider.CompareTag("Agent"))
            {
                agent.AddReward(-0.01f);
            }

            if (hitLeft.collider.CompareTag("LaneMarking"))
            {
                float distance = Vector3.Distance(transform.position, hitLeft.point);
                if (distance < 4)
                {
                    agent.AddReward(-0.01f);
                }
            }
        }
    }

    public void CheckpointComplete(Checkpoint checkpoint)
    {
        if (!previousCheckpoints.Contains(checkpoint))
        {
            agent.OnCorrectCheckpoint();
            previousCheckpoints.Add(checkpoint);
            wrongDirection.Add(checkpoint.transform.GetChild(0).GetComponent<WrongDirection>());
            StartCoroutine(DisablePoint(checkpoint.transform.GetChild(0).GetComponent<WrongDirection>()));
            checkpointsReached++;
        }else if (previousCheckpoints.Contains(checkpoint))
        {
            wrongDirection.Add(checkpoint.transform.GetChild(0).GetComponent<WrongDirection>());
            StartCoroutine(DisablePoint(checkpoint.transform.GetChild(0).GetComponent<WrongDirection>()));
        }
    }

    public void ResetCheckpoints()
    {
        previousCheckpoints.Clear();
        wrongDirection.Clear();
    }

    public IEnumerator DisablePoint(WrongDirection point)
    {
        yield return new WaitForSeconds(2f);
        wrongDirection.Remove(point);
    }
}
