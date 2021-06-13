using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualCollectorBehaviour : MonoBehaviour, IMovable, ICrasher, ICollector
{
    public Transform Transform => transform;
    public Transform GfxTransform => transform.GetChild(0);

    public PlayerBehaviour PlayerCache;
    private bool isShooting = false;
    private bool isCharging = false;
    private int throwStack = 1;
    private float innerTimer = 0f;
    private CircleCollider2D col;

    public CollectType Type => CollectType.Virtual;
    
    private void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        Swarm = new List<ICollectable>();
        isCharging = true;
        isShooting = false;
        Position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        var dt = Time.deltaTime;
        var cursor = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cursor = cursor.GetXY(cursor.y * 0.1f);

        if (!isShooting)
            Position = PlayerCache? PlayerCache.Position + (cursor - PlayerCache.Position).normalized * 
                                    Mathf.Min((cursor - PlayerCache.Position).magnitude, 
                                        3 + PlayerCache.Swarm.Count.GetSwarmLevel() * 1):
                cursor;
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
        target.Index = Swarm.Count;
        target.Collected(this);
        target.GetReady();
        col.radius = (0.2f + Swarm.Count * 0.05f);
    }
    
    public void Throw(ICollectable target, Vector3 pos)
    {
        Swarm.Remove(target);
        target.Fly(pos);
    }

    public void MoveTo(ICollectable target, ICollector swarm)
    {
        Swarm.Remove(target);
        swarm.Collect(target);
    }

    public void Blast(Vector3 pos)
    {
        foreach (var follwer in Swarm)
        {
            follwer.OnAir();
        }
        isCharging = false;
        isShooting = true;
        Velocity = pos - Position;
    }

    public void OnSwarmDead(ICollectable target)
    {
        Throw(target, GlobalUtils.RandomWholeRange(2f));
    }

    #region IMovable

    [SerializeField] private float _speed = 10f;
    private float _friction = 0.9f;

    private Vector3 _veclocity = Vector3.zero;
    public void UpdateVelocity(float dt)
    {
        Position += _veclocity.normalized * (_speed * dt);
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

    #endregion
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 15 && isShooting)
        {
            var pos = other.ClosestPoint(Position);
            //TODO: Damage Effect
            
            while (Swarm.Count > 0)
            {
                Throw(Swarm[0], Position.ToVector2() + ((Position.ToVector2() - pos) * 4) + GlobalUtils.RandomWholeRange(1f).ToVector2());
            }
            _veclocity = Vector3.zero;
            isShooting = false;
            gameObject.SetActive(false);
        }
    }
}
