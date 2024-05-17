/*

    Based on the PlayerMovement.cs script by @Dawnosaur on GitHub.
    Game feel concepts learned from @Dawnosaur video: https://www.youtube.com/watch?v=KbtcEVCM7bw.

*/

using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using TreeEditor;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : NetworkBehaviour, ICloned
{
    #region PLAYER STATE MACHINE

    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerMoveState MoveState  { get; private set; }
    public PlayerAttackState AttackState  { get; private set; }
    public PlayerDashState DashState  { get; private set; }
    public PlayerDeadState DeadState  { get; private set; }

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
    
    
    // Network cache
    private bool _jumpKeyDownCache;
    private bool _jumpKeyUpCache;

    private bool _shootKeyDownChache;
    private bool _shootKeyUpCache;

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

    [field:Header("Combat")]
    [field:SerializeField] public GameObject ArrowPrefab { get; private set; }
    [field:SerializeField] public Transform ArrowSpawnPoint { get; private set; }
    
    [field:Header("Layers & Tags")] 
    [field:SerializeField] public LayerMask GroundLayer { get; private set; } = default;

    private InputData _inputData;
    
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
        StateMachine = new PlayerStateMachine();

        MoveState = new PlayerMoveState(this, StateMachine);
        AttackState = new PlayerAttackState(this, StateMachine);
        DashState = new PlayerDashState(this, StateMachine);
        DeadState = new PlayerDeadState(this, StateMachine);
        
        StateMachine.Initialize(MoveState);
        
        PlayerRigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        IsFacingRight = true;
        IsDead = false;
        SetGravityScale(GravityScale);
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
        UpdateTimers();
        GroundCheck();
        GravityShifts();
        SetAnimatorParameters();
        LocalInput();
    }

    private void FixedUpdate()
    {
        StateMachine.CurrentPlayerState.PhysicsUpdate();
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

    #region PREDICTION MODEL

    public struct InputData : IReplicateData
    {
        public Vector2 Joystick;
        
        public bool JumpKeyDown;
        public bool JumpKeyUp;

        public bool ShootKeyDown;
        public bool ShootKeyUp;
            
        public InputData(Vector2 joystick, 
            bool jumpKeyDown, bool jumpKeyUp,
            bool shootKeyDown, bool shootKeyUp)
        {
            Joystick = joystick;
            JumpKeyDown = jumpKeyDown;
            JumpKeyUp = jumpKeyUp;
            ShootKeyDown = shootKeyDown;
            ShootKeyUp = shootKeyUp;
            tick = 0; // Fishnet puts the correct value later
        }
        
        private uint tick; // To what tick these inputs correspond

        public uint GetTick() => tick;

        public void SetTick(uint value)
        {
            tick = value;
        }
        
        public void Dispose() {}
    }
    
    public struct ReconciliationData : IReconcileData // The server information sends to verify the input results
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public Quaternion Rotation;
        public Vector3 AngularVelocity;
        
        // Life, we can use a syncvar
        // Variables that affect the movement

        public ReconciliationData(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
        {
            Position = position;
            Velocity = velocity;
            Rotation = rotation;
            AngularVelocity = angularVelocity;
            tick = 0; // Fishnet deals with assigning the value, we just equal to 0 to prevent C# from throwing errors.
        }
        
        private uint tick;

        public uint GetTick() => tick;

        public void SetTick(uint value)
        {
            tick = value;
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
    
    public override void OnStartNetwork() // OnEnable
    {
        base.TimeManager.OnTick += OnTick;
        base.TimeManager.OnPostTick += OnPostTick;

        if (base.Owner.IsLocalClient)
        {
            gameObject.name += " - Local";
        }
    }

    public override void OnStopNetwork() // OnDisable
    {
        base.TimeManager.OnTick -= OnTick;
        base.TimeManager.OnPostTick -= OnPostTick;
    }
    
    private void OnTick() // Good moment to send inputs
    {
        _inputData = BuildInputData();
        NetworkUpdate(_inputData);
    }
    
    InputData BuildInputData()
    {
        if (!base.IsOwner) // If it isn't my character, we don't generate inputs
            return default;

        Vector2 joystick = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        InputData inputData = new InputData(joystick, _jumpKeyDownCache, _jumpKeyUpCache, 
            _shootKeyDownChache, _shootKeyUpCache);
        
        ResetNetworkChache();

        return inputData;
    }

    private void ResetNetworkChache()
    {
        _jumpKeyDownCache = false;
        _jumpKeyUpCache = false;
        _shootKeyDownChache = false;
        _shootKeyUpCache = false;
    }

    // We can use CollisionEnter, Exit, etc, BUT they will be called multiple times.
    // We CAN'T use STAY 
    
    private void OnCollisionEnter(Collision other)
    {
        if (base.PredictionManager.IsReconciling) // If it is reconciling, we don't process the CollisionEnter 
            return;
    }

    private void OnPostTick() // Good moment to make reconciliation
    {
        if (base.IsServerInitialized) // IsServer
        {
            ReconciliationData reconciliationData = new ReconciliationData(
                PlayerRigidbody2D.position,
                Quaternion.identity, 
                PlayerRigidbody2D.velocity,
                Vector3.zero);
            Reconciliation(reconciliationData);
        }
    }

    [Reconcile] // Function that is replicated in clients to adjust according to the server results
    void Reconciliation(ReconciliationData reconciliationData, Channel channel = Channel.Unreliable)
    {
        // We position and rotate the Rigidbody with the server results
        PlayerRigidbody2D.position = reconciliationData.Position;
        transform.position = reconciliationData.Position;
        
        if (base.IsOwner)
        {
            print($"Reconciliation Position: {reconciliationData.Position}");
            print($"Reconciliation Velocity: {reconciliationData.Velocity}");
        }

        PlayerRigidbody2D.velocity = reconciliationData.Velocity;
        
        // We are using 2D rigidbodies, so no problemo
        //PlayerRigidbody2D.rotation = reconciliationData.Rotation;
        //PlayerRigidbody2D.angularVelocity = reconciliationData.AngularVelocity;
    }
    
    #endregion
    
    #region INPUT HANDLER
    
    [Replicate]
    private void NetworkUpdate(InputData input, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
        NetworkInput(input);
        StateMachine.CurrentPlayerState.FrameUpdate(input);
    }

    private void NetworkInput(InputData input)
    {
        if (input.Joystick.x != 0)
        {
            CheckDirectionToFace(input.Joystick.x > 0);
        }

        if (input.JumpKeyDown)
        {
            OnJumpInput();
        }

        if (input.JumpKeyUp)
        {
            OnJumpUpInput();
        }
    }

    private void LocalInput()
    {
        if (base.IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _jumpKeyDownCache = true;
            }
            
            if (Input.GetKeyUp(KeyCode.Space))
            {
                _jumpKeyUpCache = true;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _shootKeyDownChache = true;
            }
            
            if (Input.GetKeyUp(KeyCode.LeftArrow))
            {
                _shootKeyUpCache = true;
            }
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
        Animators.SetFloat(_ahSpeed, Mathf.Abs(_inputData.Joystick.x));
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
    
    #region GRAVITY

    private void GravityShifts()
    {
        // Make player fall faster if holding down S
        if (PlayerRigidbody2D.velocity.y < 0 && _inputData.Joystick.y < 0)
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

    #endregion

    #region State Functions

    public void SetVelocity(Vector2 velocity)
    {
        PlayerRigidbody2D.velocity = velocity;
    }

    #endregion
    
    #region Animation Triggers

    private void AnimationTriggerEvent(AnimationTriggerType triggerType)
    {
        StateMachine.CurrentPlayerState.AnimationTriggerEvent(triggerType);
    }
    
    public enum AnimationTriggerType
    {
        PlayerDamaged,
        PlayDashSound,
    }
    
    #endregion
}
