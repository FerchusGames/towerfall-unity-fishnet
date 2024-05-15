using UnityEngine;
using FishNet.Object;
using FishNet.Transporting;
public class PlayerAttackState : PlayerState
{
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
    }

    public override void FrameUpdate(Player.InputData input, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
    {
        base.FrameUpdate(input);
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
