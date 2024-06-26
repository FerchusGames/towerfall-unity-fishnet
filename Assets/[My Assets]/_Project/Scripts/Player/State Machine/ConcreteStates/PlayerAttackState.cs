using FishNet;
using FishNet.Managing;
using UnityEngine;
using FishNet.Object;
using FishNet.Transporting;
public class PlayerAttackState : PlayerState
{
    private float _attackTimer;
    private GameObject _arrowGameObject;

    private Vector2 vecUpRight = new Vector2(1, 1);
    private Vector2 vecUpLeft = new Vector2(-1, 1);
    private Vector2 vecDownRight = new Vector2(1, -1);
    private Vector2 vecDownLeft = new Vector2(-1, -1);

    private Vector2 aimDirection;
    
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
        
        _player.HideArrows();
        
        if (aimDirection != Vector2.zero)
        {
            if (InstanceFinder.NetworkManager.IsServerStarted)
            {
                _player.LastAttackTime = _player.ArrowSpawnInterval;
                Vector2 shootingDirection = aimDirection.normalized;

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

            if (_player.IsOwner && !_player.PredictionManager.IsReconciling)
            {
                AudioManager.GetInstance().SetAudio(SOUND_TYPE.SHOOT_SELF);
            }

            if (!_player.IsOwner)
            { 
                AudioManager.GetInstance().SetAudio(SOUND_TYPE.SHOOT_OPPONENT);
            }
        }
    }

    private Vector2 GetAimDirection()
    {
        if (_inputData.Joystick != Vector2.zero)
        {
            _player.LastAimTime = _player.AimTresholdTime;
            return _inputData.Joystick;
        }
        
        if (_player.LastAimTime > 0)
        {
            return aimDirection;
        }

        return Vector2.zero;
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
        
        aimDirection = GetAimDirection();
        
        JumpChecks(state);
        
        ShowArrows();
    }

    private void ShowArrows()
    {
        if (_inputData.Joystick == Vector2.right)
            _player._rightDir.enabled = true;
        
        if (_inputData.Joystick == Vector2.left)
            _player._rightDir.enabled = true;
        
        if (_inputData.Joystick == Vector2.down)
            _player._downDir.enabled = true;
        
        if (_inputData.Joystick == Vector2.up)
            _player._upDir.enabled = true;
        
        if (_inputData.Joystick == vecUpRight)
            _player._upRightDir.enabled = true;
        
        if (_inputData.Joystick == vecUpLeft)
            _player._upRightDir.enabled = true;
        
        if (_inputData.Joystick == vecDownRight)
            _player._downRightDir.enabled = true;
        
        if (_inputData.Joystick == vecDownLeft)
            _player._downRightDir.enabled = true;
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
