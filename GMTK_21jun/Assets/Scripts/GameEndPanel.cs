using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndPanel : MonoBehaviour
{
    public PlayTimeRecorder timeRecorder;

    public GameObject clearPanel;

    public PlayerBehaviour playerBehaviour;

    private void Awake()
    {
        timeRecorder.StopTime();
        playerBehaviour.enabled = false;
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
