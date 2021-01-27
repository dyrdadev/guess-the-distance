using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using System;



public class GameLocationService : Singleton_GO<GameLocationService>
{
    [HideInInspector] public bool locationServiceInitialized = false;
    public LocationInfo currentLocationInfo;
    
    public void Start()
    {
        StartCoroutine("StartLocationService");
    }

    public void Update()
    {
        if (locationServiceInitialized)
        {
            // fetch current location info
            currentLocationInfo = Input.location.lastData;
        }
    }

    IEnumerator StartLocationService()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
            yield break;

        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            Debug.Log("Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
            Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);

            locationServiceInitialized = true;
        }
    }
    
    public static Vector3 GPSToWorld(double lat, double lon, double alt, double rad)
    {
        // to radians
        lat = Mathf.PI * lat / 180;
        lon = Mathf.PI * lon / 180;

        // switch y and z -> y is up
        double x = (rad) * Math.Cos(lat) * Math.Cos(lon);
        double z = (rad) * Math.Cos(lat) * Math.Sin(lon);
        double y = (rad) * Math.Sin(lat);

        return new Vector3((float)x, (float)y, (float)z);
    }

    public static float DistanceInKm(float lat1, float lon1, float lat2, float lon2)
    {
        int radius = 6371;
        float lat = Mathf.Deg2Rad * (lat2 - lat1);
        float lon = Mathf.Deg2Rad * (lon2 - lon1);
        float a = Mathf.Sin(lat / 2) * Mathf.Sin(lat / 2) + Mathf.Cos(Mathf.Deg2Rad * (lat1)) * Mathf.Cos(Mathf.Deg2Rad * (lat2)) * Mathf.Sin(lon / 2) * Mathf.Sin(lon / 2);
        float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        float d = radius * c;

        return Math.Abs(d);
    }
}

