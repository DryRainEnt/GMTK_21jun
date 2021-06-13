using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class vCamZoomer : MonoBehaviour
{
    public PlayerBehaviour player;

    public float baseOthoSize = 5;
    public float levelPerScaleValue = 3;

    CinemachineVirtualCamera cinemachine;

    private void Start()
    {
        cinemachine = GetComponent<CinemachineVirtualCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        cinemachine.m_Lens.OrthographicSize = baseOthoSize + (GlobalUtils.GetSwarmLevel(player.Swarm.Count) * levelPerScaleValue);
    }
}
