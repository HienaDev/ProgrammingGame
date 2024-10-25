using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class RobotController : MonoBehaviour
{

    private Queue<(Action<MethodParameters>, MethodParameters)> orders;

    private Dictionary<string, Action<MethodParameters>> methodsDictionary;

    private bool coroutineRunning = false;

    private Rigidbody2D rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        orders = new Queue<(Action<MethodParameters>, MethodParameters)>();
        methodsDictionary = new Dictionary<string, Action<MethodParameters>>();

        methodsDictionary.Add("walk", Walk);
        methodsDictionary.Add("jump", Jump);
        methodsDictionary.Add("wait", Wait);


        rb = GetComponent<Rigidbody2D>();

        orders.Enqueue((Jump, new JumpParameters(40f)));
        orders.Enqueue((Wait, new WaitParameters(2f)));
        orders.Enqueue((Jump, new JumpParameters(40f)));
        orders.Enqueue((Walk, new WalkParameters(50f, 4f)));
        orders.Enqueue((Wait, new WaitParameters(2f)));
        //orders.Enqueue(Walk(50f, 1f));
        //orders.Enqueue(Jump(200f));
        //orders.Enqueue(Wait(2f));
        //orders.Enqueue(Walk(-50f, 1f));
        //orders.Enqueue(Wait(2f));
        //orders.Enqueue(Jump(200f));

    }

    // Update is called once per frame
    void Update()
    {
        //if(!coroutineRunning && orders.Count != 0)
        //{
        //    IEnumerator action = orders.Dequeue();
        //    StartCoroutine(action);
        //}

        //if(Input.GetKeyDown(KeyCode.Space))
        //{
        //    //StartCoroutine(Walk(50f, 5f));
        //    //StartCoroutine(Jump(40));
        //}

        ProcessOrders();
    }

    private void ProcessOrders()
    {
        if (!coroutineRunning && orders.Count != 0)
        {
            (Action<MethodParameters>, MethodParameters) action = orders.Dequeue();
            action.Item1(action.Item2);
        }
    }

    private void Walk(MethodParameters parameters)
    {
        if(parameters is WalkParameters)
        {
            parameters = (WalkParameters)parameters;
            StartCoroutine(WalkCR(((WalkParameters)parameters).Speed, ((WalkParameters)parameters).Duration));
        }
            
    }

    private IEnumerator WalkCR(float speed, float duration)
    {
        coroutineRunning = true;

        float time = 0f;

        while(time < duration)
        {
            time += Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);

            yield return null;
        }

        Debug.Log(time);

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        coroutineRunning = false;
    }

    private void Jump(MethodParameters parameters)
    {
        if (parameters is JumpParameters)
        {
            parameters = (JumpParameters)parameters;
            StartCoroutine(JumpCR(((JumpParameters)parameters).Height));
        }

    }

    private IEnumerator JumpCR(float height)
    {

        coroutineRunning = true;

        // Calculate jump force based on target height and gravity
        float jumpForce = Mathf.Sqrt(2 * Mathf.Abs(-313.92f * rb.gravityScale) * (height) * 1.7f);

        // Apply vertical force to reach the target height
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        yield return null;

        coroutineRunning = false ;
    }

    private void Wait(MethodParameters parameters)
    {
        if (parameters is WaitParameters)
        {
            parameters = (WaitParameters)parameters;
            StartCoroutine(WaitCR(((WaitParameters)parameters).Duration));
        }

    }

    private IEnumerator WaitCR(float duration)
    {

        coroutineRunning = true;

        yield return new WaitForSeconds(duration);

        coroutineRunning = false;
    }
}
