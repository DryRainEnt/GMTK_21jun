using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityBehaviour : MonoBehaviour, IMovable, ICrasher, ICollectable
{
    private Animator _anim;
    private FreeFallManager _freeFall;
    public bool isFly = false;
    private Vector3 destination = Vector3.zero;

    public Transform GfxTransform => transform.Find("GFX");
    
    private void Awake()
    {
        _anim = GetComponentInChildren<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        if (_collector != null)
        {
            var col = Physics2D.OverlapCircleAll(Position, 0.5f, 1 << 9);
            for (int i = 0; i < col.Length; i++)
            {
                var ic = col[i].GetComponent<ICollectable>();
                if (ic != null)
                {
                    if (ic.Index < 0)
                        _collector.Collect(ic);
                }
            }
        }
        
        if (!isFly)
        {
            if (_collector != null)
                _veclocity = _collector.Transform.position +
                    Index.GetSwarmOffset() - Position;
            else
                _veclocity = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        var dt = Time.fixedDeltaTime;
        
        UpdateVelocity(dt);
    }

    #region ICollectable

    [SerializeField] private int _index = -1;
    public int Index { get => _index; set => _index = value; }
    
    private ICollector _collector;
    public ICollector Collector 
    { 
        get => _collector; 
        set => _collector = value; 
    }

    public void Fly(Vector3 targetPos)
    {
        destination = targetPos;
        _veclocity = destination - Position;
        _collector = null;
        _anim.Play("EntityFly");
        _freeFall = new FreeFallManager(3, 9.8f, 1, 0);
        isFly = true;
    }

    public void Collected(ICollector target)
    {
        _anim.Play("EntityIdle");
        _collector = target;
    }

    public void GetReady()
    {
        _anim.Play("EntityReady");
    }

    #endregion
    
    #region IMovable

    [SerializeField] private float _speed = 0.3f;
    private float _friction = 0.9f;

    private Vector3 _veclocity = Vector3.zero;
    public void UpdateVelocity(float dt)
    {
        //TODO: Velocity Collision Check

        Position += (_veclocity.magnitude > 1 ? _veclocity.normalized : _veclocity) * _speed;
        
        if (_veclocity.magnitude < Mathf.Epsilon)
            _veclocity = Vector3.zero;


        if (isFly)
        {
            var h = _freeFall.GetHeight();
            if (h < 0)
            {
                _index = -1;
                _anim.Play("EntityDown");
                GfxTransform.localPosition = Vector3.zero;
                isFly = false;
                _freeFall = null;
            }
            else
            {
                _freeFall.Proceed(dt);
                GfxTransform.localPosition = Vector3.up * _freeFall.GetHeight();
            }
        }
        else
        {
            _veclocity *= _friction;
        }
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
