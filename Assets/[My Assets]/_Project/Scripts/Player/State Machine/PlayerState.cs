using FishNet;
using FishNet.Managing.Timing;
using FishNet.Object.Prediction;
using FishNet.Object;
using FishNet.Transporting;
using UnityEngine;

public class PlayerState
{
    protected Player _player;
    protected PlayerStateMachine _playerStateMachine;

    protected Player.InputData _inputData;
    
    public PlayerState(Player player, PlayerStateMachine playerStateMachine)
    {
        _player = player;
        _playerStateMachine = playerStateMachine;
    }

    public virtual void EnterState() { }
    public virtual void ExitState() { }

    [Replicate] // Must process the client as a server with player inputs
    //        Input structure,  Replicate                                   & Channel always go
    public virtual void FrameUpdate(Player.InputData input, ReplicateState state = ReplicateState.Invalid,
        Channel channel = Channel.Unreliable)
    {
        _inputData = input;
    }
    
    [Replicate]
    public virtual void PhysicsUpdate() { }
    public virtual void AnimationTriggerEvent(Player.AnimationTriggerType triggerType) { }
    
    #region JUMP CHECKS

    protected void JumpChecks(ReplicateState state)
    {
        JumpingCheck();
        JumpCutCheck();

        if (_player.CanJump() && _player.LastPressedJumpTime > 0)
        {
            _player.IsJumping = true;
            _player.IsJumpCut = false;
            _player.IsJumpFalling = false;
            Debug.Log($"Jump on Tick: {_inputData.GetTick()}, State: {state}, {Time.time}");
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

        if (!_player.PredictionManager.IsReconciling && _player.IsOwner)
        {
            AudioManager.GetInstance().SetAudio(SOUND_TYPE.JUMP_SELF);
        }
        
        // if (!_player.IsOwner)
        // {
        //     AudioManager.GetInstance().SetAudio(SOUND_TYPE.JUMP_OPPONENT);    
        // }
    }

    private void JumpResetTimers()
    {
        _player.LastPressedJumpTime = 0;
        _player.LastOnGroundTime = 0;
    }

    #endregion
}
