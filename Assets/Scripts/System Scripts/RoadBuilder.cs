using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadBuilder : MonoBehaviour
{
    public bool locked = false;

    public static RoadBuilder Instance;
    public int buildMode = 0;

    public Transform currentObject;
    public Transform ghostObject;

    [SerializeField] private List<Transform> roads;
    [SerializeField] private List<Transform> ghostRoads;
    [SerializeField] private List<Transform> roadElements;
    [SerializeField] private Transform agentGhost;
    [SerializeField] private int objRotation = 0;
    private Node curNode;
    private int curIndex = 0;

    [SerializeField] private Vector3 objOffset;

    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask nodeMask;
    [SerializeField] private LayerMask roadElemMask;
    [SerializeField] private LayerMask roadItemMask;

    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        cam = transform.GetComponentInChildren<Camera>();
        currentObject = roads[0];
        ghostObject = Instantiate(ghostRoads[curIndex]);
        ghostObject.transform.position = new Vector3(0, -1000, 0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            locked = !locked;

        if (!locked)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Input.GetKeyDown(KeyCode.M))
                buildMode = (buildMode + 1) % 2;

            if (objRotation == 0 && currentObject.name == "Road")
                objOffset = new Vector3(0, 0.51f, -0.25f);
            else if (objRotation == 90 && currentObject.name == "Road")
                objOffset = new Vector3(-0.25f, 0.51f, 0);
            else if (objRotation == 180 && currentObject.name == "Road")
                objOffset = new Vector3(0, 0.51f, 0.25f);
            else if (objRotation == 270 && currentObject.name == "Road")
                objOffset = new Vector3(0.25f, 0.51f, 0);
            else
                objOffset = new Vector3(0, 0.51f, 0);

            switch (buildMode)
            {
                case 0: RoadMode(ray); break;
                case 1: ElementMode(ray); break;
            }

            if (Input.GetKeyDown(KeyCode.R))
                RotateObject();

            if (Input.GetKeyDown(KeyCode.T))
                ChangeObject();
        }
    }


    private void RoadMode(Ray ray)
    {
        if (ghostObject.TryGetComponent<MeshCollider>(out MeshCollider collider) && !collider.enabled)
            collider.enabled = true;

        if (!roads.Contains(currentObject))
        {
            curIndex = 0;
            currentObject = roads[0];
            Destroy(ghostObject.gameObject);
            ghostObject = Instantiate(ghostRoads[curIndex]);
        }

        if (Physics.Raycast(ray, out RaycastHit hit, 1000, nodeMask))
        {
            curNode = hit.collider.GetComponent<Node>();
            if (!curNode.hasRoadObj)
            {
                ghostObject.SetParent(curNode.gameObject.transform);
                ghostObject.rotation = Quaternion.Euler(0, objRotation, 0);
                ghostObject.transform.localPosition = objOffset;
            }

            if (Input.GetKeyDown(KeyCode.Mouse0) && !curNode.hasRoadObj)
            {
                Transform newRoad = Instantiate(currentObject);
                newRoad.SetParent(curNode.gameObject.transform);
                newRoad.rotation = Quaternion.Euler(0, objRotation, 0);
                newRoad.transform.localPosition = objOffset;

                foreach (Checkpoint checkpoint in newRoad.GetComponentsInChildren<Checkpoint>())
                {
                    curNode.checkpoints.Add(checkpoint);
                    checkpoint.transform.SetParent(GameObject.Find("Checkpoints").transform);
                }
                curNode.hasRoadObj = true;
            }
            else if (Input.GetKeyDown(KeyCode.Mouse1) && curNode.hasRoadObj)
            {
                curNode.hasRoadObj = false;
                ghostObject.SetParent(ghostObject.root);
                foreach (Checkpoint checkpoint in curNode.checkpoints)
                {
                    Destroy(checkpoint.gameObject);
                }
                Destroy(curNode.transform.GetChild(0).gameObject);
            }
        }
    }
    private void ElementMode(Ray ray)
    {
        if (ghostObject.TryGetComponent<MeshCollider>(out MeshCollider collider) && collider.enabled)
            collider.enabled = false;

        if (roads.Contains(currentObject))
        {
            curIndex = 0;
            currentObject = roadElements[0];
            Destroy(ghostObject.gameObject);
            ghostObject = Instantiate(currentObject);
        }

        if (Physics.Raycast(ray, out RaycastHit hit, 1000, roadElemMask))
        {
            ghostObject.transform.position = hit.point;
            ghostObject.transform.rotation = Quaternion.Euler(0, objRotation, 0);

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Transform newRoadElem = Instantiate(currentObject);
                newRoadElem.rotation = Quaternion.Euler(0, objRotation, 0);
                newRoadElem.transform.position = hit.point;
            }
        }
        else
        {
            ghostObject.transform.position = new Vector3(0, -1000, 0);
        }

        if (Physics.Raycast(ray, out RaycastHit point, 1000, roadItemMask))
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
                Destroy(point.collider.gameObject);
        }
    }
    private void RotateObject()
    {
        objRotation = (90 + objRotation) % 360;
    }
    private void ChangeObject()
    {
        if (buildMode == 0)
        {
            curIndex = (curIndex + 1) % roads.Count;
            currentObject = roads[curIndex];
        }
        else if (buildMode == 1)
        {
            curIndex = (curIndex + 1) % roadElements.Count;
            currentObject = roadElements[curIndex];
        }
        Destroy(ghostObject.gameObject);
        if (buildMode != 0)
            ghostObject = Instantiate(currentObject);
        else if (currentObject.CompareTag("Agent"))
            ghostObject = Instantiate(agentGhost);
        else
            ghostObject = Instantiate(ghostRoads[curIndex]);
    }
}
