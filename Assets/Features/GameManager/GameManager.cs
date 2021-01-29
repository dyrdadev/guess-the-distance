using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using System;
using DyrdaIo.Singleton;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [Header("Gameplay Configuration")]
    [Range(0, 100.0f)]
    [SerializeField] private float deviationPercentage = 10;
    [SerializeField] private List<City> gameLocationList = new List<City>();
    [SerializeField] private float RotationSpeed = 1.0f;


    [Header("Globe References")]
    [SerializeField] private Transform currentPlayerLocationTrsnfm;
    [SerializeField] private Transform currentGameLocationTrnsfm;
    [SerializeField] private Transform globeTrsnfm;
    [SerializeField] private Transform cameraPivot;

    [Header("GUI References")]
    [SerializeField] private Text taskLabel;
    [SerializeField] private Text scoreLabel;
    [SerializeField] private GameObject resultGO;
    [SerializeField] private Text resultLabel;
    [SerializeField] private GameObject logInAnswerButton;
    [SerializeField] private GameObject nextCityButton;
    [SerializeField] private InputField inputField;
    [SerializeField] private GameObject infoButton;
    [SerializeField] private GameObject infoPanel;

    [SerializeField] private Color correctAnswerColor;
    [SerializeField] private Color incorrectAnswerColor;

    public Material destinationMaterial;
    private int lastCityIndex = -1;
    private City currentGameLocation;
    private Vector3 currentCameraLookAtTarget = Vector3.forward;
    private static System.Random targetRand = null;

    public enum State
    {
        none,
        input,
        result,
    };

    protected State crntState
    {
        get => m_crntState;
        set
        {
            if (value == State.input && m_crntState != State.input)
            {
                PrepareInputState();
            }

            if (value == State.result && m_crntState != State.result)
            {
                PrepareResultState();
            }

            m_crntState = value;

            DetermineCameraLookAtTarget();
        }
    }

    private State m_crntState = State.none;

    protected int score
    {
        get => m_score;
        set
        {
            m_score = value;

            // Update score label
            if (scoreLabel != null)
            {
                scoreLabel.text = "" + score;
            }
        }
    }

    private int m_score;

    public void Start()
    {
        score = 0;
        resultGO.SetActive(false);
        NextLevel();
    }

    public void Update()
    {
        DetermineCameraLookAtTarget();
        var _direction = (currentCameraLookAtTarget - cameraPivot.position).normalized;
        var _lookRotation = Quaternion.LookRotation(_direction);

        //rotate us over time according to speed until we are in the required rotation
        cameraPivot.rotation = Quaternion.Slerp(cameraPivot.rotation, _lookRotation, Time.deltaTime * RotationSpeed);
    }

    public void NextLevel()
    {
        // Create the Random object if it doesn't exist.
        if (targetRand == null)
        {
            targetRand = new System.Random();
        }

        // Determine a random index (expect same city again)
        var index = lastCityIndex;
        while (index == lastCityIndex)
        {
            index = targetRand.Next(0, gameLocationList.Count);
        }

        lastCityIndex = index;

        currentGameLocation = gameLocationList[index];

        crntState = State.input;
    }

    private void DetermineCameraLookAtTarget()
    {
        if (crntState == State.input)
        {
            currentCameraLookAtTarget = currentPlayerLocationTrsnfm.position;
        }
        else if (crntState == State.result)
        {
            //currentCameraLookAtTarget = currentPlayerLocationTrsnfm.position - currentGameLocationTrnsfm.position;
            currentCameraLookAtTarget = currentGameLocationTrnsfm.position;
        }
    }

    public void LogInAnswerButton()
    {
        crntState = State.result;
    }

    public void NextCityButton()
    {
        NextLevel();
    }

    private void PrepareInputState()
    {
        // Set location markers:
        // Set player location marker.
        if (GameLocationService.Instance.locationServiceInitialized)
        {
            SetMarkerTransform(currentPlayerLocationTrsnfm,
                GameLocationService.GPSToWorld(
                    GameLocationService.Instance.currentLocationInfo.latitude,
                    GameLocationService.Instance.currentLocationInfo.longitude,
                    GameLocationService.Instance.currentLocationInfo.altitude, 2.5));
        }
        else
        {
            SetMarkerTransform(currentPlayerLocationTrsnfm, GameLocationService.GPSToWorld(48, 11.3f, 2.5, 2.5));
        }

        // Hide destination marker.
        currentGameLocationTrnsfm.position = globeTrsnfm.position;

        // Update GUI elements:
        taskLabel.text = "Guess the Distance to " + currentGameLocation.name;
        infoButton.SetActive(true);
        infoPanel.SetActive(false);
        logInAnswerButton.SetActive(true);
        nextCityButton.SetActive(false);
        resultGO.SetActive(false);

        inputField.interactable = true;
        inputField.text = 0 + "";
    }

    private void PrepareResultState()
    {
        // Set destination location marker.
        SetMarkerTransform(currentGameLocationTrnsfm, GameLocationService.GPSToWorld(currentGameLocation.lat,
            currentGameLocation.lon, 2.5, 2.5));

        // Update GUI elements:
        logInAnswerButton.SetActive(false);
        infoButton.SetActive(false);
        infoPanel.SetActive(false);
        nextCityButton.SetActive(true);
        resultGO.SetActive(true);
        inputField.interactable = false;

        // Default position
        var tempLat = 48.08f;
        var tempLon = 11.3f;

        // Get location info
        if (GameLocationService.Instance.locationServiceInitialized)
        {
            tempLat = GameLocationService.Instance.currentLocationInfo.latitude;
            tempLon = GameLocationService.Instance.currentLocationInfo.longitude;
        }

        var distance = GameLocationService.DistanceInKm(tempLat, tempLon, (float) currentGameLocation.lat,
            (float) currentGameLocation.lon);

        // Resolve Input
        var x = 0;
        int.TryParse(inputField.text, out x);

        resultLabel.text = "You are off by " + (int) (x - distance) + "km (" + (int) distance + "km )";

        if (Mathf.Abs(x - distance) <= distance * (deviationPercentage / 100.0f))
        {
            // correct
            resultLabel.color = correctAnswerColor;
            destinationMaterial.color = correctAnswerColor;
            IncreaseScore();
        }
        else
        {
            // incorrect
            resultLabel.color = incorrectAnswerColor;
            destinationMaterial.color = incorrectAnswerColor;
        }
    }

    private void SetMarkerTransform(Transform marker, Vector3 position)
    {
        marker.localPosition = position;
        marker.LookAt(globeTrsnfm.position);
    }

    public void IncreaseScore()
    {
        score += 1;
    }
}