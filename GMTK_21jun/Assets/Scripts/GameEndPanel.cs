using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndPanel : MonoBehaviour
{
    public PlayTimeRecorder timeRecorder;

    public GameObject clearPanel;

    private void Awake()
    {
        timeRecorder.StopTime();
    }

    public void GameCleared()
    {
        gameObject.SetActive(true);
        clearPanel.SetActive(true);
    }

    public void GameOvered()
    {
        gameObject.SetActive(true);
    }
}
