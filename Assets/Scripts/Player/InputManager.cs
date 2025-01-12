using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private GameObject test;

    [SerializeField] private KeyMapper keyMapperSO;
    [field: SerializeField]
    public bool CanMove { get; set; } = true;
    
    [field: SerializeField]
    public bool CanJump { get; set; } = true;

    private bool isShuffled = false;

    public Vector2 GetMovementInput()
    {
        if (!CanMove) return Vector2.zero;

        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");

        return new Vector2(horizontal, vertical);
    }

    public bool GetJumpInput()
    {
        return Input.GetKey(keyMapperSO.jumpKey);
    }

    public bool GetJumpInputUp()
    {
        return Input.GetKeyUp(keyMapperSO.jumpKey);
    }

    public bool GetJumpInputDown()
    {
        return Input.GetKeyDown(keyMapperSO.jumpKey);
    }

    public void EnableShuffleInputs(bool shuffle)
    {
        isShuffled = shuffle;
    }
}

[CreateAssetMenu(fileName = "KeyMapperSO", menuName = "KeyMapper")]
public class KeyMapper : ScriptableObject
{
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftControl;
}