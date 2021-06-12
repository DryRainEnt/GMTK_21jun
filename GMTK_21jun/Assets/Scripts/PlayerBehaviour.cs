using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour, IMovable, ICrasher, ICollector
{
    public Camera mainCam;
    public Transform Transform => transform;
    public Transform GfxTransform => transform.GetChild(0);

    private int throwStack = 1;
    private float innerTimer = 0f;

    private void Awake()
    {
        Swarm = new List<ICollectable>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        var dt = Time.deltaTime;
        _veclocity = Vector3.right * Input.GetAxis("Horizontal")
                     + Vector3.up * Input.GetAxis("Vertical");

        var col = Physics2D.OverlapCircleAll(Position, 1f, 1 << 9);
        for (int i = 0; i < col.Length; i++)
        {
            var ic = col[i].GetComponent<ICollectable>();
            if (ic != null)
            {
                if (ic.Index < 0)
                    Collect(ic);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            throwStack = 1;
            if (Swarm.Count >= throwStack)
                Swarm[Swarm.Count - throwStack].GetReady();
        }
        if (Input.GetMouseButton(0))
        {
            innerTimer += dt;
            if (innerTimer > (0.1f / (throwStack * 0.1f)))
            {
                innerTimer = 0f;
                throwStack++;
                if (Swarm.Count >= throwStack)
                    Swarm[Swarm.Count - throwStack].GetReady();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            while (throwStack > 0)
            {
                if (Swarm.Count < 1) break;
                Throw(Swarm[Swarm.Count - 1], mainCam.ScreenToWorldPoint(Input.mousePosition));
                throwStack--;
            }

            throwStack = 1;
        }
    }

    public void ValidateCollectable(ICollectable target)
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var dt = Time.fixedDeltaTime;
        
        UpdateVelocity(dt);
    }

    [SerializeField] public List<ICollectable> Swarm;

    public void Collect(ICollectable target)
    {
        Swarm.Add(target);
        print("Swarm Added : " + Swarm.Count);
        target.Index = Swarm.Count;
        target.Collected(this);
    }
    
    public void Throw(ICollectable target, Vector3 pos)
    {
        Swarm.Remove(target);
        print("Swarm Reduced : " + Swarm.Count);
        target.Fly(pos);
    }

    #region IMovable

    [SerializeField] private float _speed = 0.3f;
    private float _friction = 0.9f;

    private Vector3 _veclocity = Vector3.zero;
    public void UpdateVelocity(float dt)
    {
        //TODO: Velocity Collision Check

        Position += (_veclocity.magnitude > 1 ? _veclocity.normalized : _veclocity) * _speed;
        
        _veclocity *= _friction;
        
        if (_veclocity.magnitude < Mathf.Epsilon)
            _veclocity = Vector3.zero;
    }
    
    public Vector3 Velocity 
    { 
        get => _veclocity; 
        set => _veclocity = value; 
    }

    public Vector3 Position
    {
        get => transform.position; 
        set => 
            transform.position = value.GetXY();
    }

    public float height;

    #endregion
}
