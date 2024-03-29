﻿using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public enum BossTransition
{
    AimPlayer,
    AttackFinished,
}

public class BossBehaviour : MonoBehaviour, ICrasher
{
    public GameObject missilePrefab = null;
    public GameObject barragePrefab = null;
    public GameObject target = null;

    public GameObject laserParent;

    public float movementSpeed;
    public float distanceThreshold;
    public bool isChanneling = false;

    public float doublePunchChargeTime;
    public float doublePunchDelay;

    public int missileCount;
    public float missileInterval;
    public float missileCooldown;
    public Transform missileOffset = null;

    public float barrageTime;
    public float barrageDelay;
    public float barrageInterval;
    public float barrageCooldown;
    public float[] barrageAngles;
    public Transform barrageOffset = null;

    public SpriteRenderer leftArmRenderer = null;
    public SpriteRenderer rightArmRenderer = null;
    public SpriteRenderer bossBodyRenderer = null;
    public Color phaseTwoColor;

    public AudioClip missileLaunchClip;
    public AudioClip hitSoundClip;
    public AudioClip gunFireClip;
    public AudioClip punchClip;

    private FSMController<BossTransition> _fsmController = null;
    private Animator _animator = null;

    public GameEndPanel GamePanel;
    [SerializeField] private int HPMax = 50;
    public int HP = 50;
    public Image BossHPGauge;

    private bool isPhaseTwo = false;

    public bool IsMissileCooldown { get; private set; } = false;

    public bool IsBarrageCooldown { get; private set; } = false;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        InitFSM();
    }

    private void InitFSM()
    {
        var followPlayer = new FollowPlayer(movementSpeed);
        var attackPlayer = new AttackPlayer(1.0f);

        _fsmController = new FSMController<BossTransition>();
        _fsmController.AddState(attackPlayer);
        _fsmController.AddState(followPlayer);
        StartCoroutine(CoLoopFSM());
    }

    public void DoTransition(BossTransition transition)
    {
        _fsmController.DoTransition(transition);
    }

    private IEnumerator CoLoopFSM()
    {
        var waitForFixedUpdate = new WaitForFixedUpdate();
        while (true)
        {
            if (!isChanneling)
            {
                _fsmController.CurrentBehaviour.CheckCondition(gameObject, target);
                _fsmController.CurrentBehaviour.Act(gameObject, target);
            }

            yield return waitForFixedUpdate;
        }
    }

    public void MissileAttack()
    {
        StartCoroutine(CoMissileAttack());
    }

    private IEnumerator CoMissileAttack()
    {
        isChanneling = true;
        IsMissileCooldown = true;
        _animator.SetTrigger("MissileAttack");

        var interval = new WaitForSeconds(missileInterval);
        for (int i = 0; i < missileCount; ++i)
        {
            if (ObjectPool.instance.TryGet(missilePrefab, out var missileObject))
            {
                var missile = missileObject.GetComponent<Missile>();
                AudioSource.PlayClipAtPoint(missileLaunchClip, missileOffset.position);
                missile.SetTarget(missileOffset.position, target.transform.position);
            }

            yield return interval;
        }
        isChanneling = false;
        DoTransition(BossTransition.AttackFinished);

        yield return new WaitForSeconds(missileCooldown);
        IsMissileCooldown = false;
    }

    public void DoublePunch()
    {
        StartCoroutine(CoDoublePunch());
    }

    private IEnumerator CoDoublePunch()
    {
        isChanneling = true;
        _animator.SetTrigger("DoublePunchStart");

        var originalColor = leftArmRenderer.color;
        leftArmRenderer.DOColor(Color.red, doublePunchChargeTime);
        rightArmRenderer.DOColor(Color.red, doublePunchChargeTime);
        yield return new WaitForSeconds(doublePunchChargeTime);
        _animator.SetTrigger("DoublePunch");
        AudioSource.PlayClipAtPoint(punchClip, transform.position);
        leftArmRenderer.DOColor(originalColor, doublePunchDelay);
        rightArmRenderer.DOColor(originalColor, doublePunchDelay);
        yield return new WaitForSeconds(0.3f);
        AudioSource.PlayClipAtPoint(punchClip, transform.position);
        yield return new WaitForSeconds(doublePunchDelay - 0.1f);
        isChanneling = false;
        DoTransition(BossTransition.AttackFinished);
    }

    public void Barrage()
    {
        StartCoroutine(CoBarrage());
    }

    private IEnumerator CoBarrage()
    {
        isChanneling = true;
        IsBarrageCooldown = true;
        _animator.SetTrigger("BarrageStart");
        yield return new WaitForSeconds(barrageDelay);

        var interval = new WaitForSeconds(barrageInterval);
        var elapsed = 0.0f;
        while (elapsed < barrageTime)
        {
            var pool = ObjectPool.instance;
            for (int i = 0; i < barrageAngles.Length; ++i)
            {
                if (pool.TryGet(barragePrefab, out var bulletObject))
                {
                    var bullet = bulletObject.GetComponent<Bullet>();
                    bullet.transform.position = barrageOffset.position;
                    bullet.SetDirection(barrageAngles[i]);
                }
            }

            AudioSource.PlayClipAtPoint(gunFireClip, transform.position);
            yield return interval;
            elapsed += barrageInterval;
        }

        _animator.SetTrigger("BarrageEnd");
        var animationTime = _animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationTime);

        isChanneling = false;
        DoTransition(BossTransition.AttackFinished);

        yield return new WaitForSeconds(barrageCooldown);
        IsBarrageCooldown = false;
    }

    public bool GetHit(int damage)
    {
        AudioSource.PlayClipAtPoint(hitSoundClip, transform.position);
        HP -= damage;
        //TODO: 보스 피격 이펙트
        if (BossHPGauge)
            BossHPGauge.fillAmount = HP / (float)HPMax;

        if (!isPhaseTwo && HP <= HPMax * 0.5f)
        {
            StartCoroutine(CoPhase2());
        }
        
        if (HP <= 0)
            GamePanel.GameCleared();

        return true;
    }

    public IEnumerator CoPhase2()
    {
        isPhaseTwo = true;
        isChanneling = true;
        _animator.Rebind();
        bossBodyRenderer.DOColor(phaseTwoColor, 1f);
        leftArmRenderer.DOColor(phaseTwoColor, 1f);
        rightArmRenderer.DOColor(phaseTwoColor, 1f);
        yield return new WaitForSeconds(1f);
        isChanneling = false;
        laserParent.SetActive(true);
    }
}

public class FollowPlayer : FSMState<BossTransition>
{
    public float MovementSpeed { get; }

    public float DistanceThreshold { get; }

    public FollowPlayer(float movementSpeed)
    {
        MovementSpeed = movementSpeed;
        _stateMap[BossTransition.AimPlayer] = nameof(AttackPlayer);
    }
    
    public override void CheckCondition(GameObject actor, GameObject target)
    {
        var actorPosition = actor.transform.position;
        var targetPosition = target.transform.position;
        var controller = actor.GetComponent<BossBehaviour>();

        if (!controller.IsMissileCooldown ||
            !controller.IsBarrageCooldown ||
            Vector2.Distance(actorPosition, targetPosition) <= controller.distanceThreshold)
        {
            controller.DoTransition(BossTransition.AimPlayer);
        }
    }

    public override void Act(GameObject actor, GameObject target)
    {
        var dt = MovementSpeed * Time.fixedDeltaTime;
        var direction = target.transform.position - actor.transform.position;
        actor.transform.position += direction.normalized * dt;
    }

    public override string GetStateId()
    {
        return nameof(FollowPlayer);
    }
}

public class AttackPlayer : FSMState<BossTransition>
{
    public BossBehaviour Actor { get; }

    public float Damage { get; }

    public AttackPlayer(float damage)
    {
        Damage = damage;
        _stateMap[BossTransition.AttackFinished] = nameof(FollowPlayer);
    }

    public override void CheckCondition(GameObject actor, GameObject target)
    {

    }

    public override void Act(GameObject actor, GameObject target)
    {
        var actorPosition = actor.transform.position;
        var targetPosition = target.transform.position;
        var controller = actor.GetComponent<BossBehaviour>();

        if (Vector2.Distance(actorPosition, targetPosition) <= controller.distanceThreshold)
        {
            controller.DoublePunch();
        }
        else if (!controller.IsBarrageCooldown)
        {
            controller.Barrage();
        }
        else if (!controller.IsMissileCooldown)
        {
            controller.MissileAttack();
        }
    }

    public override string GetStateId()
    {
        return nameof(AttackPlayer);
    }
}
