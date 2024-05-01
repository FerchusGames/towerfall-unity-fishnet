using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region Player State Machine

    private PlayerStateMachine _stateMachine;
    private PlayerMoveState _moveState;
    private PlayerAttackState _attackState;
    private PlayerDashState _dashState;
    private PlayerDeadState _deadState;

    #endregion

    private void Awake()
    {
        _stateMachine = new PlayerStateMachine();

        _moveState = new PlayerMoveState(this, _stateMachine);
        _attackState = new PlayerAttackState(this, _stateMachine);
        _dashState = new PlayerDashState(this, _stateMachine);
        _deadState = new PlayerDeadState(this, _stateMachine);
    }

    private void Update()
    {
        _stateMachine.CurrentPlayerState.FrameUpdate();
    }

    private void FixedUpdate()
    {
        _stateMachine.CurrentPlayerState.PhysicsUpdate();
    }

    #region Animation Triggers

    private void AnimationTriggerEvent(AnimationTriggerType triggerType)
    {
        _stateMachine.CurrentPlayerState.AnimationTriggerEvent(triggerType);
    }
    
    public enum AnimationTriggerType
    {
        PlayerDamaged,
        PlayDashSound,
    }
    
    #endregion
}
