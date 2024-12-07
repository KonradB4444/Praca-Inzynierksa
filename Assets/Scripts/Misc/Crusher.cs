using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crusher : MonoBehaviour
{

    [SerializeField] private float totalTime;
    [SerializeField] private float currentTime;
    [SerializeField] private float moveSpeed;
    private int reverse = 1;

    // Update is called once per frame
    void Update()
    {
        if(currentTime <= 0)
        {
            currentTime = totalTime;
            reverse *= -1;
        }
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime * reverse, 0);
        currentTime -= Time.deltaTime;
    }
}
