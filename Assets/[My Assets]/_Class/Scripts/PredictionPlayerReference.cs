using System;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using TMPro;
using UnityEngine;

public class PredictionPlayerReference : NetworkBehaviour
{
    public struct InputData : IReplicateData // To sync inputs that affect multiplayer (Like pausing)
    {
        public Vector2 Joystick;
        public bool Jump;

        public InputData(Vector2 joystick, bool jump)
        {
            Joystick = joystick;
            Jump = jump;
            tick = 0; // Fishnet puts the correct value later
        }
        
        private uint tick; // To what tick these inputs correspond

        public uint GetTick() => tick;

        public void SetTick(uint value)
        {
            tick = value;
        }

        public void Dispose() { } // We only need to put them because it is part of IReplicateData, but FishNet deals with it.
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

    [SerializeField] private float _velocity = 5f;
    [SerializeField] private float _jumpForce = 13f;
    private bool _jumpCache;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (base.IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _jumpCache = true;
            }
        }
    }

    public override void OnStartNetwork() // OnEnable
    {
        base.TimeManager.OnTick += OnTick;
        base.TimeManager.OnPostTick += OnPostTick;
    }

    public override void OnStopNetwork() // OnDisable
    {
        base.TimeManager.OnTick -= OnTick;
        base.TimeManager.OnPostTick -= OnPostTick;
    }
    
    private void OnTick() // Good moment to send inputs
    {
        Move(BuildInputData());
    }

    InputData BuildInputData()
    {
        if (!base.IsOwner) // If it isn't my character, we don't generate inputs
            return default;

        Vector2 joystick = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        InputData inputData = new InputData(joystick, _jumpCache);
        _jumpCache = false;

        return inputData;
    }

    [Replicate] // Must process the client as a server with player inputs
    //        Input structure,  Replicate                                   & Channel always go
    void Move(InputData input, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
        input.Joystick.Normalize();
        Vector3 moveDirection = new Vector3(input.Joystick.x, 0, input.Joystick.y);
        
        // Movement logic
        _rigidbody.AddForce(moveDirection * _velocity);

        bool isGrounded = true;
        if (input.Jump && isGrounded)
        {
            if (state == ReplicateState.CurrentCreated) // CurrentCreate only happens if I'm the owner
            {
                // Play jump sound
            }
            
            _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
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
                _rigidbody.position,
                _rigidbody.rotation,
                _rigidbody.velocity,
                _rigidbody.angularVelocity);
            Reconciliation(reconciliationData);
        }
    }

    [Reconcile] // Function that is replicated in clients to adjust according to the server results
    void Reconciliation(ReconciliationData reconciliationData, Channel channel = Channel.Unreliable)
    {
        // We position and rotate the Rigidbody with the server results
        _rigidbody.position = reconciliationData.Position;
        _rigidbody.velocity = reconciliationData.Velocity;
        _rigidbody.rotation = reconciliationData.Rotation;
        _rigidbody.angularVelocity = reconciliationData.AngularVelocity;
    }
}
