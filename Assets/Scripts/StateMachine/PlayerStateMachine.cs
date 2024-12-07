using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    private PlayerBaseState currentState; // The current active state
    public PlayerStates currentStateEnum;

    private DefaultState defaultState = new DefaultState();
    private CrushedState crushedState = new CrushedState();
    private SpringyState springyState = new SpringyState();
    private IcedState icedState = new IcedState();

    void Start()
    {
        SwitchStateInner(defaultState);
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
    }

    public void SwitchState(PlayerStates playerStates)
    {
        switch (playerStates)
        {
            case PlayerStates.Default:
                SwitchStateInner(defaultState);
                break;
            case PlayerStates.Crushed:
                SwitchStateInner(crushedState);
                break;
            case PlayerStates.Springy:
                SwitchStateInner(springyState);
                break;
            case PlayerStates.Iced:
                SwitchStateInner(icedState);
                break;
        }
        currentStateEnum = playerStates;
        
    }

    // dumb fucking name
    private void SwitchStateInner(PlayerBaseState newState)
    {
        currentState?.ExitState();
        currentState = newState;
        newState.EnterState(this);
    }
}
public enum PlayerStates
{
    Default,
    Crushed,
    Springy,
    Iced
}
