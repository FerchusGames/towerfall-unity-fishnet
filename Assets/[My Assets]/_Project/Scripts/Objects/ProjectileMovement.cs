using System;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ProjectileMovement : NetworkBehaviour
{
    [SerializeField] private float _force;
    
    private Rigidbody2D _rigidbody2D;
    [SerializeField] private SpriteRenderer _sprite;

    [SerializeField] private float _maxSpeed;
    
    private const float MAX_PASSED_TIME = 0.3f;
    
    public GameObject OwnerGameObject;
    private Player _player;
    public Vector2 Direction;

    public float ownerId = -1;

    public struct ReplicateData : IReplicateData
    {
        private uint tick; // To what tick these inputs correspond

        public uint GetTick() => tick;

        public void SetTick(uint value)
        {
            tick = value;
        }
        
        public void Dispose() {}

        public ReplicateData(int id)
        {
            tick = 0;
        }
    }
    
    ReplicateData BuildReplicateData()
    {
        if (!base.IsOwner) // If it isn't my character, we don't generate inputs
            return default;
        
        ReplicateData replicateData = new ReplicateData(0);
        
        return replicateData;
    }
    
    public struct ReconciliationData : IReconcileData
    {
        public Vector3 Position;
        public Vector2 Velocity;

        private uint tick;
        
        public ReconciliationData(Vector3 position, Vector2 velocity)
        {
            Position = position;
            Velocity = velocity;
            tick = 0;
        }
        
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

    private void Start()
    {
        _player = OwnerGameObject.GetComponent<Player>();
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

    private void OnTick()
    {
        NetworkUpdate(BuildReplicateData());
    }

    [Replicate]
    private void NetworkUpdate(ReplicateData replicateData ,ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
        
    }

    private void OnPostTick() // Good moment to make reconciliation
    {
        if (base.IsServerInitialized) // IsServer
        {
            ReconciliationData reconciliationData = new ReconciliationData(
                transform.position, _rigidbody2D.velocity);
            Reconciliation(reconciliationData); 
        }
    }
    
    [Reconcile] // Function that is replicated in clients to adjust according to the server results
    void Reconciliation(ReconciliationData reconciliationData, Channel channel = Channel.Unreliable)
    {
        // We position and rotate the Rigidbody with the server results
        _rigidbody2D.position = reconciliationData.Position;
        transform.position = reconciliationData.Position;

        _rigidbody2D.velocity = reconciliationData.Velocity;
    }
    
    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _rigidbody2D.velocity = _force * Direction;
        RotateTowardsMovement();
    }

    private void RotateTowardsMovement()
    {
        Vector2 dir = _rigidbody2D.velocity;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Update()
    {
        RotateTowardsMovement();
        ClampVelocity();
    }

    private void ClampVelocity()
    {
        if (_rigidbody2D.velocity.magnitude >= _force)
        {
            Vector2 clampedVelocity = _rigidbody2D.velocity.normalized * _force;
            _rigidbody2D.velocity = clampedVelocity;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!IsServerInitialized)
        {
            _sprite.enabled = false;
            return;
        }
        
        if (other.gameObject != OwnerGameObject && other.gameObject.CompareTag("Player"))
        {
            OwnerGameObject.GetComponent<Player>().AddScore();
            
            if (_player.IsOwner)
            {
                AudioManager.GetInstance().SetAudio(SOUND_TYPE.DAMAGE_SELF);
            }

            else
            {
                AudioManager.GetInstance().SetAudio(SOUND_TYPE.DAMAGE_OPPONENT);
            }
        }
        
        Despawn();
    }
}
