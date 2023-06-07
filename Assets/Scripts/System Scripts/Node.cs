using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public bool hasRoadObj = false;
    [SerializeField] private Renderer rend;
    [SerializeField] private Color original;
    public List<Checkpoint> checkpoints;

    private void Start()
    {
        rend = GetComponent<Renderer>();
    }

    private void OnMouseEnter()
    {
        if(RoadBuilder.Instance.buildMode == 0)
            rend.material.color = Color.green;
    }

    private void OnMouseExit()
    {
        rend.material.color = original;
    }
}
