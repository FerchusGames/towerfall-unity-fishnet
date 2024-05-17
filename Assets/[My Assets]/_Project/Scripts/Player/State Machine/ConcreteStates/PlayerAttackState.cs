using FishNet;
using FishNet.Managing;
using UnityEngine;
using FishNet.Object;
using FishNet.Transporting;
public class PlayerAttackState : PlayerState
{
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

        if (InstanceFinder.NetworkManager.IsServerStarted && _inputData.Joystick != Vector2.zero)
        {
            Vector2 shootingDirection = _inputData.Joystick.normalized;

            GameObject arrow = _player.ArrowPrefab;
            arrow.GetComponent<ProjectileMovement>().Direction = shootingDirection;
            arrow = GameObject.Instantiate(arrow, _player.ArrowSpawnPoint.position, Quaternion.identity);
            InstanceFinder.ServerManager.Spawn(arrow, null);
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
        
        JumpChecks();
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
