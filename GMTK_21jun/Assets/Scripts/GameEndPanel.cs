using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndPanel : MonoBehaviour
{
    public PlayTimeRecorder timeRecorder;

    private void Awake()
    {
        timeRecorder.StopTime();
    }
}
