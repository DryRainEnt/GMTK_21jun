using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class SwarmCounter : MonoBehaviour
{
    public PlayerBehaviour playerBehaviour;
    Text text;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = playerBehaviour.Swarm.Count.ToString();
    }
}
