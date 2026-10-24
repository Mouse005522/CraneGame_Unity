using Kun.Tool;
using Microsoft.Extensions.Logging;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ILogger<Test> logger = UnityLoggerProvider.CreateLogger<Test> ("GameFlow");
        logger.LogInformation ("Test log message from Test.cs");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
