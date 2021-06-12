using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour, IMovable, ICrasher
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var dt = Time.deltaTime;
        
        UpdateVelocity(dt);
    }

    #region IMovable

    private Vector3 _veclocity = Vector3.zero;
    public void UpdateVelocity(float dt)
    {
        
    }
    
    public Vector3 Velocity { 
        get => _veclocity; 
        set => _veclocity = value; 
    }

    public Vector3 Position
    {
        get => transform.position; 
        set => 
            transform.position = value.GetXY();
    }

    #endregion
}
