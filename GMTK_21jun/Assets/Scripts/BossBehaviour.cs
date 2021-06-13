using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public enum BossTransition
{
    AimPlayer,
    AttackFinished,
}

public class BossBehaviour : MonoBehaviour
{
    public GameObject missilePrefab = null;
    public GameObject target = null;

    public float movementSpeed = 2.0f;
    public float distanceThreshold = 3.0f;
    public bool isChanneling = false;

    public int missileCount = 4;
    public float missileInterval = 0.2f;
    public float missileCooldown = 4f;
    public Transform MissileOffset = null;

    private FSMController<BossTransition> _fsmController = null;
    private Animator _animator = null;

    public bool IsMissileCooldown { get; private set; } = false;

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
        for (int i = 0; i < missileCount; ++i)
        {
            if (ObjectPool.instance.TryGet(missilePrefab, out var missileObject))
            {
                var missile = missileObject.GetComponent<Missile>();
                missile.SetTarget(MissileOffset.position, target.transform.position);
            }

            yield return new WaitForSeconds(missileInterval);
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
        _animator.SetTrigger("DoublePunch");
        var animationTime = _animator.GetCurrentAnimatorStateInfo(0).length;
        Debug.Log(animationTime);
        yield return new WaitForSeconds(animationTime);
        isChanneling = false;
        DoTransition(BossTransition.AttackFinished);
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
        else
        {
            controller.MissileAttack();
        }
    }

    public override string GetStateId()
    {
        return nameof(AttackPlayer);
    }
}
