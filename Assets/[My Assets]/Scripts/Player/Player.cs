/*
 
    Based on the PlayerMovement.cs script by @Dawnosaur on GitHub.
    Game feel concepts learned from @Dawnosaur video: https://www.youtube.com/watch?v=KbtcEVCM7bw.
 
*/

using UnityEngine;
using UnityEngine.Serialization;

public class Player : MonoBehaviour, ICloned
{
    #region PLAYER STATE MACHINE

    private PlayerStateMachine _stateMachine;
    private PlayerMoveState _moveState;
    private PlayerAttackState _attackState;
    private PlayerDashState _dashState;
    private PlayerDeadState _deadState;

    #endregion

    #region VARIABLES

    public Rigidbody2D PlayerRigidbody2D { get; private set; }
    public Animator[] Animators { get; private set; } = new Animator[5];

    // State Control
    public bool IsDead;
    public bool IsFacingRight;
    public bool IsJumping;
    public bool IsJumpCut;
    public bool IsJumpFalling;

    // Timers
    public float LastOnGroundTime;
    public float LastPressedJumpTime;

    [field:Header("Acceleration")] [SerializeField]
    [field:SerializeField] public float RunMaxSpeed { get; private set; } = default;

    [field:SerializeField] public float RunAccelerationRate { get; private set; } = default;
    [field:SerializeField] public float RunDecelerationRate { get; private set; } = default;
    [field:SerializeField, Range(0, 1f)] public float AirAccelerationMultiplier { get; private set; } = default;
    [field:SerializeField, Range(0, 1f)] public float AirDecelerationMultiplier { get; private set; } = default;

    [field:Header("Jumping")] 
    [field:SerializeField] public float JumpForce { get; private set; } = default;
    [field:SerializeField] public float CoyoteTime { get; private set; } = default;
    [field:SerializeField] public float JumpInputBufferTime { get; private set; } = default;
    [field:SerializeField] public float JumpHangTimeThreshold { get; private set; } = default;
    [field:SerializeField] public float JumpHangAccelerationMultiplier { get; private set; } = default;
    [field:SerializeField] public float JumpHangMaxSpeedMultiplier { get; private set; } = default;

    [field:Header("Dashing")] 
    [field:SerializeField] public float DashSpeed { get; private set; } = default;
    [field:SerializeField] public float DashTime { get; private set; } = default;
    [field:SerializeField] public float DashHangTime  { get; private set; } = default;
    [field:SerializeField] public float DashCooldown { get; private set; } = default;

    [field:Header("Gravity")] 
    [field:SerializeField] public float GravityScale { get; private set; } = default;
    [field:SerializeField] public float MaxFallSpeed { get; private set; } = default;
    [field:SerializeField] public float MaxFastFallSpeed { get; private set; } = default;
    [field:SerializeField] public float FallGravityMultiplier { get; private set; } = default;
    [field:SerializeField] public float FastFallGravityMultiplier { get; private set; } = default;
    [field:SerializeField] public float JumpCutGravityMultiplier { get; private set; } = default;
    [field:SerializeField] public float JumpHangGravityMultiplier { get; private set; } = default;

    [field:Header("Checks")] 
    [field:SerializeField] public Transform GroundCheckPoint { get; private set; } = null;
    [field:SerializeField] public Vector2 GroundCheckSize { get; private set; } = new Vector2(0.5f, 0.03f);

    [field:Header("Layers & Tags")] 
    [field:SerializeField] public LayerMask GroundLayer { get; private set; } = default;

    public Vector2 MoveInput { get; private set; } = default;

    #endregion
    
    #region ANIMATION HASHES

    private int _ahIsDead = Animator.StringToHash("IsDead");
    private int _ahIsJumping = Animator.StringToHash("IsJumping");
    private int _ahMelee = Animator.StringToHash("Melee");
    private int _ahShoot = Animator.StringToHash("Shoot");
    private int _ahSpeed = Animator.StringToHash("Speed");

    #endregion
    
    private void Awake()
    {
        _stateMachine = new PlayerStateMachine();

        _moveState = new PlayerMoveState(this, _stateMachine);
        _attackState = new PlayerAttackState(this, _stateMachine);
        _dashState = new PlayerDashState(this, _stateMachine);
        _deadState = new PlayerDeadState(this, _stateMachine);
        
        PlayerRigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        IsFacingRight = true;
        IsDead = false;
        SetGravityScale(GravityScale);
        
        _stateMachine.Initialize(_moveState);
    }

    public void SetAnimators(Animator[] animators)
    {
        for (int i = 0; i < 5; i++)
        {
            Animators[i] = animators[i];
        }
    }
    
    private void Update()
    {
        _stateMachine.CurrentPlayerState.FrameUpdate();
        UpdateTimers();
        HandleInput();
        GroundCheck();
        SetAnimatorParameters();
    }

    private void FixedUpdate()
    {
        _stateMachine.CurrentPlayerState.PhysicsUpdate();
    }
    
    private void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight)
        {
            Turn();
        }
    }

    private void Turn()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        IsFacingRight = !IsFacingRight;
    }

    #region INPUT HANDLER

    private void HandleInput()
    {
        MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (MoveInput.x != 0)
        {
            CheckDirectionToFace(MoveInput.x > 0);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnJumpInput();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            OnJumpUpInput();
        }
    }

    #endregion
    
    #region GRAVITY

    public void SetGravityScale(float gravityScale)
    {
        PlayerRigidbody2D.gravityScale = gravityScale;
    }

    public void FallSpeedCap(float fallSpeedMaxValue)
    {
        PlayerRigidbody2D.velocity = new Vector2(PlayerRigidbody2D.velocity.x,
            Mathf.Max(PlayerRigidbody2D.velocity.y, -fallSpeedMaxValue));
    }

    #endregion

    #region INPUT CALLBACKS

    private void OnJumpInput()
    {
        LastPressedJumpTime = JumpInputBufferTime;
    }

    private void OnJumpUpInput()
    {
        if (CanJumpCut())
        {
            IsJumpCut = true;
        }
    }

    #endregion

    #region COLLISION CHECKS

    private void GroundCheck()
    {
        if (Physics2D.OverlapBox(GroundCheckPoint.position, GroundCheckSize, 0, GroundLayer) && !IsJumping)
        {
            LastOnGroundTime = CoyoteTime;
        }
    }

    #endregion

    #region CHECK METHODS

    public bool CanJump()
    {
        return LastOnGroundTime > 0 && !IsJumping;
    }

    public bool CanJumpCut()
    {
        return IsJumping && PlayerRigidbody2D.velocity.y > 0;
    }

    #endregion

    #region ANIMATIONS

    private void SetAnimatorParameters()
    {
        Animators.SetFloat(_ahSpeed, Mathf.Abs(MoveInput.x));
        Animators.SetBool(_ahIsJumping, IsJumping || IsJumpFalling);
    }
    
    #endregion
    
    #region TIMERS
    private void UpdateTimers()
    {
        LastOnGroundTime -= Time.deltaTime;
        LastPressedJumpTime -= Time.deltaTime;
    }
    #endregion
    
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
