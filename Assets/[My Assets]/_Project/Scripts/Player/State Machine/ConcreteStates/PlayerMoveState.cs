using FishNet;
using UnityEngine;
using FishNet.Object;
using FishNet.Transporting;

public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(Player player, PlayerStateMachine playerStateMachine) : base(player, playerStateMachine) { }

    public override void EnterState()
    {
        base.EnterState();
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate(Player.InputData input, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
        base.FrameUpdate(input);
        
        if (input.ShootKeyDown)
        {
            _playerStateMachine.ChangeState(_player.AttackState);
        }
        
        JumpChecks(state);
        Run();
    }

    public override void PhysicsUpdate()
    {
        
    }

    public override void AnimationTriggerEvent(Player.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
    }
    
    #region RUN METHODS
    private void Run()
    {
        float targetSpeed = _inputData.Joystick.x * _player.RunMaxSpeed;

        #region CALCULATING ACCELERATION RATE

        float accelerationRate;

        // Our acceleration rate will differ depending on if we are trying to accelerate or if we are trying to stop completely.
        // It will also change if we are in the air or if we are grounded.

        if (_player.LastOnGroundTime > 0)
        {
            accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? _player.RunAccelerationRate : _player.RunDecelerationRate;
        }

        else
        {
            accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f)
                ? _player.RunAccelerationRate * _player.AirAccelerationMultiplier
                : _player.RunDecelerationRate * _player.AirDecelerationMultiplier;
        }

        #endregion

        #region ADD BONUS JUMP APEX ACCELERATION

        if ((_player.IsJumping || _player.IsJumpFalling) && Mathf.Abs(_player.PlayerRigidbody2D.velocity.y) < _player.JumpHangTimeThreshold)
        {
            accelerationRate *= _player.JumpHangAccelerationMultiplier;
            targetSpeed *= _player.JumpHangMaxSpeedMultiplier;
        }

        #endregion

        float speedDifference = targetSpeed - _player.PlayerRigidbody2D.velocity.x;

        float movement = speedDifference * accelerationRate;    

        _player.PlayerRigidbody2D.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }
    #endregion
    
   
}
