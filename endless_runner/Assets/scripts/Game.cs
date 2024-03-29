using System;
using TMPro;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField]
    Runner runner;
    
    [SerializeField]
    TrackingCamera trackingCamera;
    
    [SerializeField]
    TextMeshPro displayText;

    [SerializeField, Min(0.001f)]
    float maxDeltaTime = 1f / 20f;

    [SerializeField]
    SkylineGenerator[] skylineGenerators;

    [SerializeField]
	SkylineGenerator obstacleGenerator;

    [SerializeField]
    float extraGapFactor = 0.5f, extraSequenceFactor = 1f;

    bool isPlaying;

    void StartNewGame()
    {
        trackingCamera.StartNewGame();
        runner.StartNewGame(obstacleGenerator.StartNewGame(trackingCamera));
        trackingCamera.Track(runner.Position);
        for (int i = 0; i < skylineGenerators.Length; i++)
        {
            skylineGenerators[i].StartNewGame(trackingCamera);
        }
        isPlaying = true;
    }
    
    void Update()
    {
        if(isPlaying)
        {
            UpdateGame();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            StartNewGame();
        }

    }

    private void UpdateGame()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            runner.StartJumping();
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            runner.EndJumping();
        }

        float accumulateDeltaTime = Time.deltaTime;

        while (accumulateDeltaTime > maxDeltaTime && isPlaying)
        {
            isPlaying = runner.Run(Time.deltaTime);
            accumulateDeltaTime -= maxDeltaTime;
        }

        isPlaying = isPlaying && runner.Run(accumulateDeltaTime);
        runner.UpdateVisualization();
        trackingCamera.Track(runner.Position);
        displayText.SetText("{0}", Mathf.Floor(runner.Position.x));
        obstacleGenerator.FillView(trackingCamera, runner.SpeedX * extraGapFactor,
            runner.SpeedX * extraSequenceFactor);
        for (int i = 0; i < skylineGenerators.Length; i++)
        {
            skylineGenerators[i].FillView(trackingCamera);
        }
    }
}
