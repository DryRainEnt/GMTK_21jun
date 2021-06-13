using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour, IMovable, ICrasher, ICollector
{
    public Camera mainCam;
    public Transform Transform => transform;
    public Transform GfxTransform => transform.GetChild(0);

    public GameObject VirtualColletorPrefab;
    public VirtualCollectorBehaviour VirtualCollectorCache = null;

    public GameEndPanel GamePanel;
    private Animator _anim;
    private SpriteRenderer _sr;

    public bool isFlip => _sr && _sr.flipX;
    
    public CollectType Type => CollectType.Player;

    [SerializeField] private bool isDashing = false;
    [SerializeField] private bool isCharging = false;
    [SerializeField]  private int throwStack = 1;
    [SerializeField] private float innerTimer = 0f;
    [SerializeField]  private float reposTimer = 0f;
    [SerializeField] private float dashTimer = 0f;

    [SerializeField] private float dashDelay = 0.4f;
    [SerializeField] private float repositionDelay = 2f;

    public int HP = 1;
    
    private void Awake()
    {
        _anim = GetComponentInChildren<Animator>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        Swarm = new List<ICollectable>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        var dt = Time.deltaTime;
        if (!isDashing)
            _veclocity = Vector3.right * Input.GetAxis("Horizontal")
                     + Vector3.up * Input.GetAxis("Vertical");
        
        _anim.SetBool("OnMove", _veclocity.magnitude > Mathf.Epsilon);

        if (mainCam)
            _sr.flipX = (mainCam.ScreenToWorldPoint(Input.mousePosition).x < Position.x);

        if (reposTimer < repositionDelay && reposTimer >= 0)
            reposTimer += dt;
        else if (reposTimer > 0)
        {
            reposTimer = -1f;
            AlignSwarm();
        }

        if (dashTimer < dashDelay && dashTimer >= 0f)
            dashTimer += dt;
        else if (dashTimer > 0)
        {
            dashTimer = -1f;
            isDashing = false;
            foreach (var follower in Swarm)
            {
                follower.Distance = 1f;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && dashTimer < 0f)
        {
            ChargeCancel();
            isDashing = true;
            dashTimer = 0f;
            foreach (var follower in Swarm)
            {
                follower.Distance = 0.6f;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            _anim.Play("PlayerHold");
            isCharging = true;

            GameObject vcc;
            if (ObjectPool.instance.TryGet(VirtualColletorPrefab, out vcc))
            {
                VirtualCollectorCache = vcc.GetComponent<VirtualCollectorBehaviour>();
                VirtualCollectorCache.PlayerCache = this;
            }
            
            if (Swarm.Count >= 1 && VirtualCollectorCache)
            {
                MoveTo(Swarm[Swarm.Count - 1], VirtualCollectorCache);
                throwStack = 1;
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            _anim.Play("PlayerIdle");
            ChargeCancel();
        }
        if (isCharging && Input.GetMouseButton(0))
        {
            if (!VirtualCollectorCache.gameObject.activeSelf)
                VirtualCollectorCache = null;
            if (!VirtualCollectorCache)
            {
                _anim.Play("PlayerIdle");
                isCharging = false;
                return;
            }
            
            innerTimer += dt;
            if (innerTimer > Mathf.Max(0.1f / (throwStack * 0.2f), 0.1f))
            {
                innerTimer = 0f;
                
                if (Swarm.Count > 0)
                {
                    // Swarm[Swarm.Count - throwStack].GetReady();
                    var target = Swarm[Swarm.Count - 1];
                    MoveTo(target, VirtualCollectorCache);
                    throwStack++;
                }
            }
        }
        if (isCharging && Input.GetMouseButtonUp(0))
        {
            _anim.Play("PlayerAttack");
            VirtualCollectorCache.Blast(VirtualCollectorCache.Position + (VirtualCollectorCache.Position - Position));
            VirtualCollectorCache = null;
            throwStack = 1;
        }

        var cols = Physics2D.OverlapCircleAll(Position, 1f, LayerMask.NameToLayer("Entity"));
        foreach (var col in cols)
        {
            var ic = col.transform.GetComponent<ICollectable>();
            if (ic != null)
            {
                if (ic.Index < 0)
                    Collect(ic);
            }
        }

    }

    void ChargeCancel()
    {
        if (!VirtualCollectorCache) return;
        isCharging = false;
            
        while (VirtualCollectorCache.Swarm.Count > 0)
        {
            VirtualCollectorCache.MoveTo(VirtualCollectorCache.Swarm[0], this);
        }

        throwStack = 1;
        
        VirtualCollectorCache.gameObject.SetActive(false);
        VirtualCollectorCache = null;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var dt = Time.fixedDeltaTime;
        
        UpdateVelocity(dt);
    }

    [SerializeField] public List<ICollectable> Swarm;

    public void AlignSwarm()
    {
        for (int i = 0; i < Swarm.Count; i++)
        {
            Swarm[i].Index = i + 1;
        }
    }
    
    public void Collect(ICollectable target)
    {
        Swarm.Add(target);
        target.Index = Swarm.Count;
        target.Collected(this);
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

    public bool GetHit(int damage)
    {
        if (HP <= 0)
            return false;
        
        HP -= damage;
        
        //TODO: 피격시 사망 연출
        _anim.Play("PlayerDamaged");
        GamePanel.GameOvered();
        
        return true;
    }

    #region IMovable

    [SerializeField] private float _speed = 30f;
    [SerializeField] private float _dashSpeed = 20f;
    private float _friction = 0.98f;

    private Vector3 _veclocity = Vector3.zero;
    public void UpdateVelocity(float dt)
    {
        var totalVelocity = Velocity;
        
        if (isDashing)
            totalVelocity = (_veclocity.magnitude > 1 ? _veclocity.normalized : _veclocity) * (_dashSpeed * dt);
        else
            totalVelocity = (_veclocity.magnitude > 1 ? _veclocity.normalized : _veclocity) * (_speed * dt);
        
        //TODO: Velocity Collision Check
        var xblock = Physics2D.RaycastAll(Position, 
            totalVelocity.Coefficient(Vector3.right), totalVelocity.magnitude, 1 << 17);
        var yblock = Physics2D.RaycastAll(Position, 
            totalVelocity.Coefficient(Vector3.up).normalized, totalVelocity.magnitude, 1 << 17);
        
        if (xblock.Length > 0)
        {
            totalVelocity = totalVelocity.Coefficient(Vector3.up);
        }
        if (yblock.Length > 0)
        {
            totalVelocity = totalVelocity.Coefficient(Vector3.right);
        }

        if (isDashing)
            Position += totalVelocity;
        else
            Position += totalVelocity;
        
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
            transform.position = value.GetXY(value.y * 0.1f);
    }

    public float height;

    #endregion

    public void OnSwarmDead(ICollectable target)
    {
        Throw(target, target.Movable.Position + GlobalUtils.RandomWholeRange(2f));
        reposTimer = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Damage"))
        {
            other.GetComponent<Bullet>()?.Dispose();
            GetHit(1);
        }
    }
}
