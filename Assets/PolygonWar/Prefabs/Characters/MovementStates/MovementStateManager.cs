using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementStateManager : MonoBehaviour
{
    MovementBaseState currentState;

    public IdleState idle = new IdleState();
    public WalkState walk = new WalkState();
    public CrouchState crouch = new CrouchState();

    [HideInInspector] public Animator anim;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        SwitchState(idle);
    }
    private void Update()
    {
        currentState.UpdateState(this);
    }

    public void SwitchState(MovementBaseState state)
    {
        currentState = state;
        currentState.EnterState(this);
    }

}
