using System;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine.Serialization;

public class ProjectileMovement : NetworkBehaviour
{
    [SerializeField] public Vector2 Direction;
    [SerializeField] private float _force;

    public float ownerId = -1;
    public GameObject OwnerGameObject;
    
    private Rigidbody2D _rigidbody2D;

    private Vector2 _startPosition;

    readonly private SyncVar<ReconciliationData> reconciliationData = new SyncVar<ReconciliationData>();
    
    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _startPosition = transform.position;

        reconciliationData.UpdateSendRate(0.2f);
        reconciliationData.OnChange += OnReconciliationDataChange;
    }

    void OnReconciliationDataChange(ReconciliationData before, ReconciliationData next, bool asServer)
    {
        if (base.IsServer) // If I'm the server, I'm always right
            return;
        
        if (next == null) // There is no info yet
            return;
        
        // The same logic as ShootRPC
        float passedTime = (float)base.TimeManager.TimePassed(next.Tick);

        float stepInterval = Time.fixedDeltaTime;
        int steps = (int)(passedTime / stepInterval); // How many physics frame to calculate

        _rigidbody2D.position = next.Position; // We position according to where the server says it is located
        (Vector2 finalPosition, Vector2 finalVelocity) =
            global::PredictionManager.Instance.Predict(gameObject, next.Velocity, steps);
        _rigidbody2D.position = finalPosition;
        transform.position = finalPosition;
        _rigidbody2D.velocity = finalVelocity;
        
        Debug.Log($"Position: {transform.position}, Velocity: {_rigidbody2D.velocity}, PassedTime: {passedTime}, " +
                  $"Steps: {steps} , Tick: {TimeManager.Tick}");
    }

    private void RotateTowardsMovement()
    {
        Vector2 dir = _rigidbody2D.velocity;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Start()
    {
        _rigidbody2D.velocity = _force * Direction;
        ShootRPC(base.TimeManager.Tick);
    }

    private void Update()
    {
        if (!base.IsServer)
            return;
        
        // Update the reconciliation data
        reconciliationData.Value = new ReconciliationData() // Only sends the last value when it is due
        {
            Position = _rigidbody2D.position,
            Velocity = _rigidbody2D.velocity,
            Tick = base.TimeManager.Tick
        };
        
        RotateTowardsMovement();
    }

    [ObserversRpc]
    private void ShootRPC(uint serverTick)
    {
        float passedTime = (float)base.TimeManager.TimePassed(serverTick);

        float stepInterval = Time.fixedDeltaTime;
        int steps = (int)(passedTime / stepInterval); // How many physics frame to calculate
        
        (Vector2 finalPosition, Vector2 finalVelocity) =
            global::PredictionManager.Instance.Predict(gameObject, _force * Direction, steps);
        _rigidbody2D.position = finalPosition;
        transform.position = finalPosition;
        _rigidbody2D.velocity = finalVelocity;
    }

    [System.Serializable] // Can be turned into a JSON or be transferred via networking (fishnet, servers)
    public class ReconciliationData
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public uint Tick;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!IsServerInitialized)
            return;
        
        if (other.gameObject != OwnerGameObject && other.gameObject.CompareTag("Player"))
        {
            Debug.Log($"A point has been awarded to {ownerId}");
        }
        
        Despawn();
    }
}
