using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class vCamZoomer : MonoBehaviour
{
    public PlayerBehaviour player;

    public float baseOthoSize;
    public float levelPerScaleValue;

    CinemachineVirtualCamera cinemachine;
    Coroutine _coroutine = null;

    private void Start()
    {
        cinemachine = GetComponent<CinemachineVirtualCamera>();
        cinemachine.m_Lens.OrthographicSize = baseOthoSize;
        //cinemachine.m_Lens.OrthographicSize = baseOthoSize + (GlobalUtils.GetSwarmLevel(player.Swarm.Count) * levelPerScaleValue);
    }
}
