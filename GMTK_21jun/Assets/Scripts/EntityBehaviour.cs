using System;
using System.Collections;
using System.Collections.Generic;
//using System.Net.Configuration;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class EntityBehaviour : MonoBehaviour, IMovable, ICrasher, ICollectable
{
    private Animator _anim;
    private SpriteRenderer _sr;
    private FreeFallManager _freeFall;
    public bool isFly = false;
    public bool isShoot = false;
    public Vector3 destination = Vector3.zero;
    public float distMult = 1f;

    public GameObject FXPrefab;

    public int hp = 1;

    public float Distance { get => distMult; set => distMult = value; }
    public Transform GfxTransform => transform.Find("GFX");

    private UnityEvent onCollected = new UnityEvent();

    public IMovable Movable => this;
    
    private void Awake()
    {
        _sr = GetComponentInChildren<SpriteRenderer>();
        _anim = GetComponentInChildren<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        if (!isFly)
        {
            if (_collector != null)
            {
                _veclocity = _collector.Transform.position +
                    Index.GetSwarmOffset(_collector.Type, distMult) - Position;
                if (_collector.Type == CollectType.Player)
                {
                    _sr.flipX = _collector.isFlip;
                }
            }
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
        isShoot = false;
        Position = _collector.Transform.position +
                   Index.GetSwarmOffset(_collector.Type);
        destination = targetPos;
        _veclocity = destination - Position;
        _collector = null;
        // _anim.Play("EntityFly");
        _freeFall = new FreeFallManager(3, 9.8f, 1, 0);
        isFly = true;
    }

    public void OnAir()
    {
        isShoot = true;
        // _anim.Play("EntityFly");
    }

    public void Collected(ICollector target)
    {
        hp = 1;
        _anim.Play("DroneIdle");
        _collector = target;
        isFly = false;
        distMult = 1f;
    }

    public void GetReady()
    {
        _anim.Play("DroneReady");
        
        isFly = false;
    }

    public void ResetState()
    {
        _anim.Play("DroneIdle");
        isFly = false;
        isShoot = false;
    }

    public void GetDamage(int dmg)
    {
        hp -= dmg;
    }
    

    #endregion
    
    #region IMovable

    [SerializeField] private float _speed = 3f;
    private float _friction = 0.98f;

    private Vector3 _veclocity = Vector3.zero;
    public void UpdateVelocity(float dt)
    {
        //TODO: Velocity Collision Check

        if (isShoot)
            Position += _veclocity;
        else if (!isFly)
            Position += (_veclocity.magnitude > 1 ? _veclocity.normalized : _veclocity) * (_speed / distMult * dt);
        else
            Position += _veclocity * dt;
        
        if (_veclocity.magnitude < Mathf.Epsilon)
            _veclocity = Vector3.zero;


        if (isFly)
        {
            var h = _freeFall.GetHeight();
            if (h < 0)
            {
                _index = -1;
                _anim.Play("DroneDown");
                GfxTransform.localPosition = Vector3.zero;
                isFly = false;
                _freeFall = null;
                
                if (hp <= 0)
                {
                    OnDead();
                }
            }
            else
            {
                _freeFall.Proceed(dt);
                GfxTransform.localPosition = Vector3.up * _freeFall.GetHeight();
                if (hp <= 0 && Random.value < 0.075f)
                {
                    OnDead();
                }

            }
        }
        else
        {
            _veclocity *= _friction;
        }
    }

    void OnDead()
    {
        GameObject fx;
        ObjectPool.instance.TryGet(FXPrefab, out fx);
        fx.transform.position = GfxTransform.position;
        gameObject.SetActive(false);
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
            transform.position = value.GetXY(value.y * 0.1f);
    }

    public float height;

    #endregion

    public void OnHit()
    {
        if (_collector != null)
            _collector.OnSwarmDead(this);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_collector == null) return;
        if (_collector.Type == CollectType.Virtual) return;
            
        var ic = other.transform.GetComponent<ICollectable>();
        if (ic != null)
        {
            if (ic.Index < 0)
                _collector.Collect(ic);
        }

        if (other.gameObject.layer == 16)
        {
            other.gameObject.GetComponent<Bullet>()?.Dispose();
            GetDamage(1);
            OnHit();
        }

        if (other.gameObject.layer == 15)
        {
            OnHit();
        }
    }

    public void AddCollectedEventListener(FollowerGenerator generator, Vector2 coord)
    {
        UnityAction addedListener = () => generator.CollectedFollowerOn(coord, gameObject);

        onCollected.AddListener(addedListener);
        onCollected.AddListener(() => onCollected.RemoveListener(addedListener));
    }
}
