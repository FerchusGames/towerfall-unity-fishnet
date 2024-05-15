using FishNet.Object.Prediction;
using FishNet.Object;
using FishNet.Transporting;

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
}
