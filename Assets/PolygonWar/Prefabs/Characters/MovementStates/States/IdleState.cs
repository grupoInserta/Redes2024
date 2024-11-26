using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : MovementBaseState
{
    public PlayerController player;

    public override void EnterState(MovementStateManager movement)
    {

    }

    public override void UpdateState(MovementStateManager movement)
    {
        if(player.inputValues.magnitude > 0.1f)
        {
            movement.SwitchState(movement.walk);
        }
        if(Input.GetKeyDown(KeyCode.C))
        {
            movement.SwitchState(movement.crouch);
        }
    }
}
