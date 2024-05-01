using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(Player player, PlayerStateMachine playerStateMachine) : base(player, playerStateMachine)
    {
    }

    public override void EnterState()
    {
        base.EnterState();
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        JumpChecks();
        GravityShifts();
    }

    public override void PhysicsUpdate()
    {
        Run();
    }

    public override void AnimationTriggerEvent(Player.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
    }
    
    #region RUN METHODS
    private void Run()
    {
        float targetSpeed = _player.MoveInput.x * _player.RunMaxSpeed;

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
    
    #region JUMP CHECKS

    private void JumpChecks()
    {
        JumpingCheck();
        JumpCutCheck();

        if (_player.CanJump() && _player.LastPressedJumpTime > 0)
        {
            _player.IsJumping = true;
            _player.IsJumpCut = false;
            _player.IsJumpFalling = false;
            Jump();
        }
    }

    private void JumpingCheck()
    {
        if (_player.IsJumping && _player.PlayerRigidbody2D.velocity.y < 0)
        {
            _player.IsJumping = false;

            _player.IsJumpFalling = true;
        }
    }

    private void JumpCutCheck()
    {
        if (_player.LastOnGroundTime > 0 && !_player.IsJumping)
        {
            _player.IsJumpCut = false;
            _player.IsJumpFalling = false; // Logic failure in the original script?
        }
    }
    
    #endregion
    
    #region JUMP METHODS

    private void Jump()
    {
        JumpResetTimers();

        float force = _player.JumpForce;

        if (_player.PlayerRigidbody2D.velocity.y < 0)
        {
            force -= _player.PlayerRigidbody2D.velocity.y; // To always jump the same amount.
        }

        _player.PlayerRigidbody2D.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    private void JumpResetTimers()
    {
        _player.LastPressedJumpTime = 0;
        _player.LastOnGroundTime = 0;
    }

    #endregion
    
    #region GRAVITY

    private void GravityShifts()
    {
        // Make player fall faster if holding down S
        if (_player.PlayerRigidbody2D.velocity.y < 0 && _player.MoveInput.y < 0)
        {
            _player.SetGravityScale(_player.GravityScale * _player.FallGravityMultiplier);
            _player.FallSpeedCap(_player.MaxFastFallSpeed);
        }

        // Scale gravity up if jump button released
        else if (_player.IsJumpCut)
        {
            _player.SetGravityScale(_player.GravityScale * _player.JumpCutGravityMultiplier);
            _player.FallSpeedCap(_player.MaxFallSpeed);
        }

        // Higher gravity when near jump height apex
        else if ((_player.IsJumping || _player.IsJumpFalling) && Mathf.Abs(_player.PlayerRigidbody2D.velocity.y) < _player.JumpHangTimeThreshold)
        {
            _player.SetGravityScale(_player.GravityScale * _player.JumpHangGravityMultiplier);
        }

        // Higher gravity if falling
        else if (_player.PlayerRigidbody2D.velocity.y < 0)
        {
            _player.SetGravityScale(_player.GravityScale * _player.FallGravityMultiplier);
            _player.FallSpeedCap(_player.MaxFallSpeed);
        }

        // Reset gravity
        else
        {
            _player.SetGravityScale(_player.GravityScale);
        }
    }

    #endregion
}
