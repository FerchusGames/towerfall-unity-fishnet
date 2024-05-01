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
    public bool IsDead { get; private set; }
    public bool IsFacingRight { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsJumpCut { get; private set; }
    public bool IsJumpFalling { get; private set; }

    // Timers
    public float LastOnGroundTime { get; private set; }
    public float LastPressedJumpTime { get; private set; }

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
        JumpChecks();
        GravityShifts();
        SetAnimatorParameters();
    }

    private void FixedUpdate()
    {
        _stateMachine.CurrentPlayerState.PhysicsUpdate();
        Run();
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

    #region RUN METHODS

    private void Run()
    {
        float targetSpeed = MoveInput.x * RunMaxSpeed;

        #region CALCULATING ACCELERATION RATE

        float accelerationRate;

        // Our acceleration rate will differ depending on if we are trying to accelerate or if we are trying to stop completely.
        // It will also change if we are in the air or if we are grounded.

        if (LastOnGroundTime > 0)
        {
            accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? RunAccelerationRate : RunDecelerationRate;
        }

        else
        {
            accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f)
                ? RunAccelerationRate * AirAccelerationMultiplier
                : RunDecelerationRate * AirDecelerationMultiplier;
        }

        #endregion

        #region ADD BONUS JUMP APEX ACCELERATION

        if ((IsJumping || IsJumpFalling) && Mathf.Abs(PlayerRigidbody2D.velocity.y) < JumpHangTimeThreshold)
        {
            accelerationRate *= JumpHangAccelerationMultiplier;
            targetSpeed *= JumpHangMaxSpeedMultiplier;
        }

        #endregion

        float speedDifference = targetSpeed - PlayerRigidbody2D.velocity.x;

        float movement = speedDifference * accelerationRate;

        PlayerRigidbody2D.AddForce(movement * Vector2.right, ForceMode2D.Force);
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

    #endregion

    #region JUMP CHECKS

    private void JumpChecks()
    {
        JumpingCheck();
        JumpCutCheck();

        if (CanJump() && LastPressedJumpTime > 0)
        {
            IsJumping = true;
            IsJumpCut = false;
            IsJumpFalling = false;
            Jump();
        }
    }

    private void JumpingCheck()
    {
        if (IsJumping && PlayerRigidbody2D.velocity.y < 0)
        {
            IsJumping = false;

            IsJumpFalling = true;
        }
    }

    private void JumpCutCheck()
    {
        if (LastOnGroundTime > 0 && !IsJumping)
        {
            IsJumpCut = false;
            IsJumpFalling = false; // Logic failure in the original script?
        }
    }

    #endregion

    #region JUMP METHODS

    private void Jump()
    {
        JumpResetTimers();

        float force = JumpForce;

        if (PlayerRigidbody2D.velocity.y < 0)
        {
            force -= PlayerRigidbody2D.velocity.y; // To always jump the same amount.
        }

        PlayerRigidbody2D.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    private void JumpResetTimers()
    {
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;
    }

    #endregion

    #region GRAVITY

    private void SetGravityScale(float gravityScale)
    {
        PlayerRigidbody2D.gravityScale = gravityScale;
    }

    private void GravityShifts()
    {
        // Make player fall faster if holding down S
        if (PlayerRigidbody2D.velocity.y < 0 && MoveInput.y < 0)
        {
            SetGravityScale(GravityScale * FallGravityMultiplier);
            FallSpeedCap(MaxFastFallSpeed);
        }

        // Scale gravity up if jump button released
        else if (IsJumpCut)
        {
            SetGravityScale(GravityScale * JumpCutGravityMultiplier);
            FallSpeedCap(MaxFallSpeed);
        }

        // Higher gravity when near jump height apex
        else if ((IsJumping || IsJumpFalling) && Mathf.Abs(PlayerRigidbody2D.velocity.y) < JumpHangTimeThreshold)
        {
            SetGravityScale(GravityScale * JumpHangGravityMultiplier);
        }

        // Higher gravity if falling
        else if (PlayerRigidbody2D.velocity.y < 0)
        {
            SetGravityScale(GravityScale * FallGravityMultiplier);
            FallSpeedCap(MaxFallSpeed);
        }

        // Reset gravity
        else
        {
            SetGravityScale(GravityScale);
        }
    }

    private void FallSpeedCap(float fallSpeedMaxValue)
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

    private bool CanJump()
    {
        return LastOnGroundTime > 0 && !IsJumping;
    }

    private bool CanJumpCut()
    {
        return IsJumping && PlayerRigidbody2D.velocity.y > 0;
    }

    #endregion

    #region ANIMATIONS

    private void SetAnimatorParameters()
    {
        Animators.SetFloat(_ahSpeed, Mathf.Abs(MoveInput.x));
        Animators.SetBool(_ahIsJumping, IsJumping || IsJumpFalling);
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            IsDead = !IsDead;
            Animators.SetBool(_ahIsDead, IsDead);
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Animators.SetTrigger(_ahShoot);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Animators.SetTrigger(_ahMelee);
        }
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
