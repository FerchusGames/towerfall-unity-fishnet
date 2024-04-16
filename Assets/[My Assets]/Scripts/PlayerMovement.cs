/*
 
    Based on the PlayerMovement.cs script by @Dawnosaur on GitHub.
    Game feel concepts learned from @Dawnosaur video: https://www.youtube.com/watch?v=KbtcEVCM7bw.
    Document about concepts learned: https://docs.google.com/document/d/1neL358fLZM3yz3O9kkJKi3RBuq3Bzq6jvvDE-C_UFkM/edit?usp=sharing.
 
*/

using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region VARIABLES
    public Rigidbody2D PlayerRigidbody2D { get; private set; }
    public Animator PlayerAnimator { get; private set; }

    // State Control
    public bool IsDead { get; private set; }
    public bool IsFacingRight { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsJumpCut { get; private set; }
    public bool IsJumpFalling { get; private set; }

    // Timers
    public float LastOnGroundTime { get; private set; }
    public float LastPressedJumpTime { get; private set; }

    [Header ("Acceleration")]
    [SerializeField] private float _runMaxSpeed = default;
    [SerializeField] private float _runAccelerationRate = default;
    [SerializeField] private float _runDecelerationRate = default;
    [SerializeField, Range(0, 1f)] private float _airAccelerationMultiplier = default;
    [SerializeField, Range(0, 1f)] private float _airDecelerationMultiplier = default;

    [Header("Jumping")]
    [SerializeField] private float _jumpForce = default;
    [SerializeField] private float _coyoteTime = default;
    [SerializeField] private float _jumpInputBufferTime = default;
    [SerializeField] private float _jumpHangTimeThreshold = default;
    [SerializeField] private float _jumpHangAccelerationMultiplier = default;
    [SerializeField] private float _jumpHangMaxSpeedMultiplier = default;

    [Header("Gravity")]
    [SerializeField] private float _gravityScale = default;
    [SerializeField] private float _maxFallSpeed = default;
    [SerializeField] private float _maxFastFallSpeed = default;
    [SerializeField] private float _fallGravityMultiplier = default;
    [SerializeField] private float _fastFallGravityMultiplier = default;
    [SerializeField] private float _jumpCutGravityMultiplier = default;
    [SerializeField] private float _jumpHangGravityMultiplier = default;

    [Header ("Checks")]
    [SerializeField] private Transform _groundCheckPoint = null;
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.5f, 0.03f);

    [Header("Layers & Tags")]
    [SerializeField] private LayerMask _groundLayer = default;

    private Vector2 _moveInput = default;
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
        PlayerRigidbody2D = GetComponent<Rigidbody2D>();
        PlayerAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        IsFacingRight = true;
        IsDead = false;
        SetGravityScale(_gravityScale);
    }

    private void Update()
    {
        UpdateTimers();
        HandleInput();
        GroundCheck();
        JumpChecks();
        GravityShifts();
        SetAnimatorParameters();
    }

    private void FixedUpdate()
    {
        Run();
    }

    #region INPUT HANDLER
    private void HandleInput()
    {
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");

        if (_moveInput.x != 0)
        {
            CheckDirectionToFace(_moveInput.x > 0);
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
        float targetSpeed = _moveInput.x * _runMaxSpeed;

        #region CALCULATING ACCELERATION RATE
        float accelerationRate;

        // Our acceleration rate will differ depending on if we are trying to accelerate or if we are trying to stop completely.
        // It will also change if we are in the air or if we are grounded.

        if (LastOnGroundTime > 0)
        {
            accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? _runAccelerationRate : _runDecelerationRate;
        }

        else
        {
            accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? _runAccelerationRate * _airAccelerationMultiplier : _runDecelerationRate * _airDecelerationMultiplier;
        }
        #endregion

        #region ADD BONUS JUMP APEX ACCELERATION
        if ((IsJumping || IsJumpFalling) && Mathf.Abs(PlayerRigidbody2D.velocity.y) < _jumpHangTimeThreshold)
        {
            accelerationRate *= _jumpHangAccelerationMultiplier;
            targetSpeed *= _jumpHangMaxSpeedMultiplier;
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

        float force = _jumpForce;

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
        if (PlayerRigidbody2D.velocity.y < 0 && _moveInput.y < 0) 
        {
            SetGravityScale(_gravityScale * _fastFallGravityMultiplier);
            FallSpeedCap(_maxFastFallSpeed);
        }

        // Scale gravity up if jump button released
        else if (IsJumpCut) 
        {
            SetGravityScale(_gravityScale * _jumpCutGravityMultiplier);
            FallSpeedCap(_maxFallSpeed);
        }

        // Higher gravity when near jump height apex
        else if ((IsJumping || IsJumpFalling) && Mathf.Abs(PlayerRigidbody2D.velocity.y) < _jumpHangTimeThreshold) 
        {
            SetGravityScale(_gravityScale * _jumpHangGravityMultiplier);
        }

        // Higher gravity if falling
        else if (PlayerRigidbody2D.velocity.y < 0) 
        {
            SetGravityScale(_gravityScale * _fallGravityMultiplier);
            FallSpeedCap(_gravityScale * _maxFallSpeed);
        }

        // Reset gravity
        else
        {
            SetGravityScale(_gravityScale);
        }
    }

    private void FallSpeedCap(float fallSpeedMaxValue)
    {
        PlayerRigidbody2D.velocity = new Vector2(PlayerRigidbody2D.velocity.x, Mathf.Max(PlayerRigidbody2D.velocity.y, -fallSpeedMaxValue));
    }

    #endregion

    #region INPUT CALLBACKS
    private void OnJumpInput()
    {
        LastPressedJumpTime = _jumpInputBufferTime;
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
        if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer) && !IsJumping)
        {
            LastOnGroundTime = _coyoteTime;
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
        PlayerAnimator.SetFloat(_ahSpeed, Mathf.Abs(_moveInput.x));
        PlayerAnimator.SetBool(_ahIsJumping, IsJumping || IsJumpFalling);

        if (Input.GetKeyDown(KeyCode.F))
        {
            IsDead = !IsDead;
            PlayerAnimator.SetBool(_ahIsDead, IsDead);
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            PlayerAnimator.SetTrigger(_ahShoot);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            PlayerAnimator.SetTrigger(_ahMelee);
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
}
