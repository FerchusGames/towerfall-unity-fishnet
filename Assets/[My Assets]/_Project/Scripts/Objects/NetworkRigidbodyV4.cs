// using UnityEngine;
// using FishNet.Object;
// using FishNet.Object.Synchronizing;
// using FishNet.Transporting;
//
// public class NetworkRigidbodyV4 : NetworkBehaviour
// {
//     [SerializeField] private Vector2 _direction;
//     [SerializeField] private float _force;
//     
//     private Rigidbody2D _rigidbody2D;
//
//     private Vector2 _startPosition;
//
//     [SerializeField] private PredictionManager _predictionManager;
//
//     [SyncVar(SendRate = 0.2f, OnChange = nameof(OnReconciliationDataChange), Channel = Channel.Unreliable)] // Refreshes the last packet each second
//     private ReconciliationData reconciliationData;
//
//     void OnReconciliationDataChange(ReconciliationData before, ReconciliationData next, bool asServer)
//     {
//         if (base.IsServer) // If I'm the server, I'm always right
//             return;
//         
//         if (next == null) // There is no info yet
//             return;
//         
//         // The same logic as ShootRPC
//         float passedTime = (float)base.TimeManager.TimePassed(next.Tick);
//
//         float stepInterval = Time.fixedDeltaTime;
//         int steps = (int)(passedTime / stepInterval); // How many physics frame to calculate
//
//         _rigidbody2D.position = next.Position; // We position according to where the server says it is located
//         (Vector2 finalPosition, Vector2 finalVelocity) =
//             _predictionManager.Predict(gameObject, next.Velocity, steps);
//         _rigidbody2D.position = finalPosition;
//         _rigidbody2D.velocity = finalVelocity;
//     }
//     
//     private void Awake()
//     {
//         _rigidbody2D = GetComponent<Rigidbody2D>();
//         _startPosition = transform.position;
//     }
//
//     private void Update()
//     {
//         if (!base.IsServer)
//             return;
//
//         if (Input.GetKeyDown(KeyCode.Space))
//         {
//             ShootRPC(base.TimeManager.Tick);
//             _rigidbody2D.velocity = _force * _direction;
//         }
//
//         if (Input.GetKeyDown(KeyCode.R))
//         {
//             RestartRPC();
//         }
//         
//         // Update the reconciliation data
//         reconciliationData = new ReconciliationData() // Only sends the last value when it is due
//         {
//             Position = _rigidbody2D.position,
//             Velocity = _rigidbody2D.velocity,
//             Tick = base.TimeManager.Tick
//         };
//     }
//
//     [ObserversRpc]
//     private void ShootRPC(uint serverTick)
//     {
//         float passedTime = (float)base.TimeManager.TimePassed(serverTick);
//
//         float stepInterval = Time.fixedDeltaTime;
//         int steps = (int)(passedTime / stepInterval); // How many physics frame to calculate
//
//         (Vector2 finalPosition, Vector2 finalVelocity) =
//             _predictionManager.Predict(gameObject, _force * _direction, steps);
//         _rigidbody2D.position = finalPosition;
//         _rigidbody2D.velocity = finalVelocity;
//     }
//
//     [ObserversRpc (RunLocally = true)]
//     private void RestartRPC()
//     {
//         _rigidbody2D.position = _startPosition;
//         _rigidbody2D.velocity = Vector2.zero;
//         _rigidbody2D.angularVelocity = 0f;
//     }
//
//     [System.Serializable] // Can be turned into a JSON or be transferred via networking (fishnet, servers)
//     public class ReconciliationData
//     {
//         public Vector2 Position;
//         public Vector2 Velocity;
//         public uint Tick;
//     }
// }