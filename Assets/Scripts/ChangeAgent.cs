using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ChangeAgent : MonoBehaviour
{
    private CinemachineFreeLook cam;
    private List<Transform> agentList;
    [SerializeField] private int agentIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        cam = GetComponent<CinemachineFreeLook>();
        agentList = new List<Transform>();
        Transform agents = GameObject.Find("Agents").transform;
        foreach (Transform agent in agents)
        {
            agentList.Add(agent);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            agentIndex = (agentIndex + 1) % agentList.Count;
            cam.LookAt = agentList[agentIndex];
        } else if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            agentIndex--;
            if (agentIndex < 0)
                agentIndex = agentList.Count - 1;
            cam.LookAt = agentList[agentIndex];
            cam.Follow = agentList[agentIndex];
        }
    }
}
