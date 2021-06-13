using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndPanel : MonoBehaviour
{
    public PlayTimeRecorder timeRecorder;

    public GameObject clearPanel;

    public PlayerBehaviour playerBehaviour;

    public AudioClip clearClip, overClip;

    bool isClearSFX, isOverSFX;

    void Awake()
    {
        timeRecorder.StopTime();
        playerBehaviour.enabled = false;
    }

    private void Update()
    {
        if (isClearSFX)
        {
            GetComponent<AudioSource>().PlayOneShot(clearClip);
            isClearSFX = false;
        }

        if (isOverSFX)
        {
            GetComponent<AudioSource>().PlayOneShot(overClip);
            isOverSFX = false;
        }
    }

    public void GameCleared()
    {
        isClearSFX = true;
        gameObject.SetActive(true);
        clearPanel.SetActive(true);
    }

    public void GameOvered()
    {
        isOverSFX = true;
        gameObject.SetActive(true);
    }
}
