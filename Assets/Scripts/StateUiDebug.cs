using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StateUiDebug : MonoBehaviour
{
    [SerializeField] private PlayerStateMachine playerStateMachine;
    [SerializeField] private TMP_Text text;


    private void Update()
    {
        text.text = playerStateMachine.GetCurrentState().ToString();
    }
}
