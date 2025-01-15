using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    private PlayerBaseState currentState; // The current active state
    public PlayerStates currentStateEnum;

    private DefaultState defaultState = new DefaultState();
    private CrushedState crushedState = new CrushedState();
    private SpringyState springyState = new SpringyState();
    private IcedState icedState = new IcedState();
    private HurtState hurtState = new HurtState();
    private BubbleState bubbleState = new BubbleState();

    void Start()
    {
        SwitchStateInner(defaultState, PlayerStates.Default);
    }

    void Update()
    {
        currentState.UpdateState();

        if(Input.GetKeyDown(KeyCode.K))
        {
            SwitchState(PlayerStates.Crushed);
        }
        if(Input.GetKeyDown(KeyCode.L))
        {
            SwitchState(PlayerStates.Springy);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            SwitchState(PlayerStates.Iced);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            SwitchState(PlayerStates.Hurt);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            SwitchState(PlayerStates.Bubble);
        }
    }

    public void SwitchState(PlayerStates newState)
    {
        if(!currentState.AllowedTransitions.Contains(newState))
            {
                Debug.LogWarning($"Transition from {currentStateEnum} to {newState} is not allowed.");
                return;
            }
        switch (newState)
        {
            case PlayerStates.Default:
                SwitchStateInner(defaultState, PlayerStates.Default);
                break;
            case PlayerStates.Crushed:
                SwitchStateInner(crushedState, PlayerStates.Crushed);
                break;
            case PlayerStates.Springy:
                SwitchStateInner(springyState, PlayerStates.Springy);
                break;
            case PlayerStates.Iced:
                SwitchStateInner(icedState, PlayerStates.Iced);
                break;
            case PlayerStates.Hurt:
                SwitchStateInner(hurtState, PlayerStates.Hurt);
                break;
            case PlayerStates.Bubble:
                SwitchStateInner(bubbleState, PlayerStates.Bubble);
                break;
        }
        //currentStateEnum = playerStates;
        
    }

    // dumb fucking name
    private void SwitchStateInner(PlayerBaseState newState, PlayerStates newStateEnum)
    {
        currentState?.ExitState();
        currentState = newState;
        currentStateEnum = newStateEnum;
        newState.EnterState(this);
    }

    public PlayerBaseState GetCurrentState()
    {
        return currentState;
    }

}

public enum PlayerStates
{
    Default,
    Crushed,
    Springy,
    Iced,
    Hurt,
    Bubble
}
