using FishNet;
using FishNet.Managing;
using UnityEngine;
using FishNet.Object;
using FishNet.Transporting;
public class PlayerAttackState : PlayerState
{
    private float _attackTimer;
    private GameObject _arrowGameObject;
    
    public PlayerAttackState(Player player, PlayerStateMachine playerStateMachine) : base(player, playerStateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
    }

    public override void ExitState()
    {
        base.ExitState();

        if (_player.LastAttackTime > 0)
            return;

        if (_inputData.Joystick != Vector2.zero)
        {
            if (InstanceFinder.NetworkManager.IsServerStarted)
            {
                _player.LastAttackTime = _player.ArrowSpawnInterval;
                Vector2 shootingDirection = _inputData.Joystick.normalized;

                GameObject arrow = _player.ArrowPrefab;
                ProjectileMovement projectileMovement = arrow.GetComponent<ProjectileMovement>();
                projectileMovement.Direction = shootingDirection;
                projectileMovement.ownerId = _inputData.Id;
                projectileMovement.OwnerGameObject = _player.gameObject;
                arrow = GameObject.Instantiate(arrow, _player.ArrowSpawnPoint.position, Quaternion.identity);
                if (_arrowGameObject)
                    InstanceFinder.ServerManager.Despawn(_arrowGameObject);
                _arrowGameObject = arrow;
                InstanceFinder.ServerManager.Spawn(arrow, null);
            }

            if (_player.IsOwner)
            {
                AudioManager.GetInstance().SetAudio(SOUND_TYPE.SHOOT_SELF);
            }

            else
            {
                AudioManager.GetInstance().SetAudio(SOUND_TYPE.SHOOT_OPPONENT);
            }
        }
    }

    public override void FrameUpdate(Player.InputData input, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
        base.FrameUpdate(input);
        
        if (input.ShootKeyUp)
        {
            _playerStateMachine.ChangeState(_player.MoveState);
        }
        
        if (_player.LastOnGroundTime > 0)
        {
            _player.SetVelocity(new Vector2(0, _player.PlayerRigidbody2D.velocity.y));
        }
        
        JumpChecks(state);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void AnimationTriggerEvent(Player.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
    }
}
