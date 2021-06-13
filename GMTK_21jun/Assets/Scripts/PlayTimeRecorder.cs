using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class PlayTimeRecorder : MonoBehaviour
{
    Text text;

    float startTime;

    float stoppedTime;

    bool isStopped = false;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
        StartTimer();
    }

    private void StartTimer()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStopped)
            UpdateText();
    }

    public float StopTime()
    {
        isStopped = true;
        stoppedTime = Time.time - startTime;
        return stoppedTime;
    }

    private void UpdateText()
    {
        float currentTime = Time.time - startTime;

        text.text = GetHour(currentTime).ToString("D2") + ":" + GetMinute(currentTime).ToString("D2") + ":" + GetSecond(currentTime).ToString("D2") + GetMiliSecond(currentTime);
    }

    public static int GetHour(float _time)
    {
        return (int)(_time / (60 * 60) % 24);
    }

    public static int GetMinute(float _time)
    {
        return (int)((_time / 60) % 60);
    }

    public static int GetSecond(float _time)
    {
        return (int)(_time % 60);
    }

    public static string GetMiliSecond(float _time)
    {
        return string.Format("{0:.00}", (_time % 1));
    }
}
