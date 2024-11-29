using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkState : MovementBaseState
{
    public override void EnterState(MovementStateManager movement)
    {
        movement.anim.SetBool("Waliking", true);
    }

    public override void UpdateState(MovementStateManager movement)
    {
        if(Input.GetKey(KeyCode.C))
        {
            ExitState(movement, movement.crouch);
        }
    }

    void ExitState(MovementStateManager movement, MovementBaseState state)
    {
        movement.anim.SetBool("Walking", false);
        movement.SwitchState(state);
    }
}
