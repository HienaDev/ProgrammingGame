using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using TMPro;
using System.Linq;
using static UnityEditor.Progress;

public class RobotController : MonoBehaviour
{
    // The queue of the orders, saving the method and its corresponding parameters
    private Queue<(Action<MethodParameters>, MethodParameters)> orders;

    // Dictionary that isn't being used but might come in handy for intellisense
    private Dictionary<string, Action<MethodParameters>> methodsDictionary;

    // Bool to keep in check if the coroutine is running so that we can start a new one
    private bool coroutineRunning = false;

    private Rigidbody2D rb;

    // Text that the input field writes to
    [SerializeField] private TextMeshProUGUI code;

    // Save each line of code here split by '\n'
    private string[] ordersCode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        orders = new Queue<(Action<MethodParameters>, MethodParameters)>();
        methodsDictionary = new Dictionary<string, Action<MethodParameters>>();

        rb = GetComponent<Rigidbody2D>();

        methodsDictionary.Add("walk", Walk);
        methodsDictionary.Add("jump", Jump);
        methodsDictionary.Add("wait", Wait);

        ordersCode = code.text.Split('\n');

        for (int i = 0; i < ordersCode.Length; i++)
        {
            string[] splitTemp;

            // If we are in the last string, remove last character, as there seems to be
            // a problem with the last char of the input field, not allowing it to be parsed
            // into a float
            if(i ==  ordersCode.Length - 1)
            {
                ordersCode[ordersCode.Length - 1] = 
                                    ordersCode[ordersCode.Length - 1]
                                    .Remove(ordersCode[ordersCode.Length - 1].Length - 1);
            }

            // Split all the code by the characters showed under: 
            // '(' and ')' contain the parameters
            // ';' is essentially just removed
            // ',' splits the parameters
            // ' ' empty spaces are pretty much useless and are mainly used for identation (hopefully)
            splitTemp = ordersCode[i].Split('(', ')', ';', ',', ' ');
            // Remove all empty strings
            splitTemp = splitTemp.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            //Read each order after being split
            ReadOrders(splitTemp);
        }
        

    }

    public string RemoveWhitespace(string input)
    {
        return new string(input.ToCharArray()
            .Where(c => !Char.IsWhiteSpace(c))
            .ToArray());
    }

    private void ReadOrders(string[] ordersSplit)
    {
        // Need to add Try, but first we'll assume they are all correct
        //      TryParse added but need other checks before assuming its correct
        //      but right now the code only runes if both parameters were parsed

        for (int i = 0; i < ordersSplit.Length; i++)
        {
            // Remove all extra white spaces from the individual strings so " 1" becomes "1"
            ordersSplit[i] = RemoveWhitespace(ordersSplit[i]);
            Debug.Log($"{ordersSplit[i]}");

            switch(ordersSplit[i].ToLower())
            {
                case "walk":
                    //Try to parse the first parameter
                    float parameter1;
                    if (float.TryParse(ordersSplit[i + 1], out parameter1))
                    {
                        Debug.Log($"Parsed {ordersSplit[i + 1]}");
                        //Try to parse the second parameter
                        float parameter2;
                        if (float.TryParse(ordersSplit[i + 2], out parameter2))
                        {
                            Debug.Log($"Parsed {ordersSplit[i + 2]}");
                            orders.Enqueue((Walk, new WalkParameters(parameter1, parameter2)));
                            Debug.Log("walk enqueued");
                            i += 2;
                        }
                        else
                        {
                            Debug.Log($"Cant parse {ordersSplit[i + 2]}");
                        }
                    }
                    else
                    {
                        Debug.Log($"Cant parse {ordersSplit[i + 1]}");
                    }




                    break;

                case "jump":
                    //Try to parse the first parameter
                    if (float.TryParse(ordersSplit[i + 1], out parameter1))
                    {
                        Debug.Log($"Parsed {ordersSplit[i + 1]}");
                        orders.Enqueue((Jump, new JumpParameters(parameter1)));
                        Debug.Log("Jump enqueued");
                        i += 1;
                    }
                    else
                    {
                        Debug.Log($"Cant parse {ordersSplit[i + 1]}");
                    }


                    break;

                case "wait":
                    //Try to parse the first parameter
                    if (float.TryParse(ordersSplit[i + 1], out parameter1))
                    {
                        Debug.Log($"Parsed {ordersSplit[i + 1]}");
                        orders.Enqueue((Wait, new WaitParameters(parameter1)));
                        Debug.Log("Wait enqueued");
                        i += 1;
                    }
                    else
                    {
                        Debug.Log($"Cant parse {ordersSplit[i + 1]}");
                    }


                    break;

                default:

                    break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        ProcessOrders();
    }

    //Method to process orders whenever no order is running
    private void ProcessOrders()
    {
        if (!coroutineRunning && orders.Count != 0)
        {
            (Action<MethodParameters>, MethodParameters) action = orders.Dequeue();
            action.Item1(action.Item2);
        }
    }

    // Assist method for the Walk Coroutine that takes some MethodParameters as input
    private void Walk(MethodParameters parameters)
    {
        if(parameters is WalkParameters)
        {
            parameters = (WalkParameters)parameters;
            StartCoroutine(WalkCR(((WalkParameters)parameters).Speed, ((WalkParameters)parameters).Duration));
        }
            
    }

    // Walking Coroutine
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

        //Debug.Log(time);

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        coroutineRunning = false;
    }

    // Assist method for the Jump Coroutine that takes some MethodParameters as input
    private void Jump(MethodParameters parameters)
    {
        if (parameters is JumpParameters)
        {
            parameters = (JumpParameters)parameters;
            StartCoroutine(JumpCR(((JumpParameters)parameters).Height));
        }

    }

    // Jump Coroutine
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

    // Assist method for the Wait Coroutine that takes some MethodParameters as input
    private void Wait(MethodParameters parameters)
    {
        if (parameters is WaitParameters)
        {
            parameters = (WaitParameters)parameters;
            StartCoroutine(WaitCR(((WaitParameters)parameters).Duration));
        }

    }

    // Wait Coroutine
    private IEnumerator WaitCR(float duration)
    {

        coroutineRunning = true;

        yield return new WaitForSeconds(duration);

        coroutineRunning = false;
    }
}
