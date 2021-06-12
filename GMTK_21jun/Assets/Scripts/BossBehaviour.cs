using System;
using System.Collections;
using UnityEngine;

public enum BossTransition
{
    AimPlayer,
    AttackFinished,
}

public class BossBehaviour : MonoBehaviour
{
    public GameObject laserPrefab = null;
    public GameObject target = null;

    public float laserWidth = 20f;
    public float laserLength = 1000f;
    public bool isChanneling = false;

    private FSMController<BossTransition> _fsmController = null;
    private FSMState<BossTransition> _nextAction = null;

    private void Awake()
    {
        InitFSM();
    }

    private void InitFSM()
    {
        var followPlayer = new FollowPlayer(1.0f, 3.0f);
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

            switch (_nextAction)
            {
                case AttackPlayer :

                    break;

            }

            yield return waitForFixedUpdate;
        }
    }

    public void LaunchLaser(Vector3 diff)
    {
        _nextAction = _fsmController.CurrentBehaviour;
        var laserObject = Instantiate(laserPrefab, null);

        var direction = diff.normalized;
        // 레이저 길이 절반 * (1 / 100) * (100 / 32) <- PPU
        laserObject.transform.position = transform.position + (direction * laserLength * 0.5f * 0.03125f);
        laserObject.transform.right = direction;
        laserObject.transform.localScale = new Vector3(laserLength, laserWidth, 1f);
        DoTransition(BossTransition.AttackFinished);
    }
}

public class FollowPlayer : FSMState<BossTransition>
{
    public float MovementSpeed { get; }

    public float DistanceThreshold { get; }

    public FollowPlayer(float movementSpeed, float distanceThreshold)
    {
        MovementSpeed = movementSpeed;
        DistanceThreshold = distanceThreshold;
        _stateMap[BossTransition.AimPlayer] = nameof(AttackPlayer);
    }
    
    public override void CheckCondition(GameObject actor, GameObject target)
    {
        var actorPosition = actor.transform.position;
        var targetPosition = target.transform.position;

        if (Vector2.Distance(actorPosition, targetPosition) <= DistanceThreshold)
        {
            var controller = actor.GetComponent<BossBehaviour>();
            controller.DoTransition(BossTransition.AimPlayer);
        }
    }

    public override void Act(GameObject actor, GameObject target)
    {
        Debug.Log("Aim player");
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
        var controller = actor.GetComponent<BossBehaviour>();
        var diff = target.transform.position - actor.transform.position;
        controller.LaunchLaser(diff);
    }

    public override string GetStateId()
    {
        return nameof(AttackPlayer);
    }
}
