using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using System;
using DyrdaDev.Singleton;

public class GameLocationService : SingletonMonoBehaviour<GameLocationService>
{
    [HideInInspector] public bool locationServiceInitialized = false;
    [HideInInspector] public LocationInfo currentLocationInfo;

    public void Start()
    {
        StartCoroutine(StartLocationService());
    }

    public void Update()
    {
        if (locationServiceInitialized)
        {
            currentLocationInfo = Input.location.lastData;
        }
    }

    private IEnumerator StartLocationService()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("Location is disabled by the user.");
            
            yield break;
        }

        // Start service before querying location
        Input.location.Start();
        
        // Wait until service initializes
        var maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait < 1)
        {
            // Service didn't initialize in 20 seconds
            Debug.Log("Timed out");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            // Connection has failed
            Debug.Log("Unable to determine device location");
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
            Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " +
                      Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " +
                      Input.location.lastData.timestamp);

            locationServiceInitialized = true;
        }
    }

    /// <summary>
    /// Location to World. (alt is currently not used.)
    /// </summary>
    /// <param name="lat">in degrees</param>
    /// <param name="lon">in degrees</param>
    /// <param name="alt">in meters</param>
    /// <param name="rad">in unity units</param>
    /// <returns></returns>
    public static Vector3 LocationToWorld(double lat, double lon, double alt, double rad)
    {
        // to radians
        lat = Mathf.PI * lat / 180;
        lon = Mathf.PI * lon / 180;

        // Spherical coordinates to orthogonal axes:
        // switch y and z -> y is up
        var x = rad * Math.Cos(lat) * Math.Cos(lon);
        var z = rad * Math.Cos(lat) * Math.Sin(lon);
        var y = rad * Math.Sin(lat);

        return new Vector3((float) x, (float) y, (float) z);
    }

    public static float DistanceInKm(float lat1, float lon1, float lat2, float lon2)
    {
        var radius = 6371;
        var lat = Mathf.Deg2Rad * (lat2 - lat1);
        var lon = Mathf.Deg2Rad * (lon2 - lon1);

        // Haversine formula:
        var a = Mathf.Sin(lat / 2) * Mathf.Sin(lat / 2) + Mathf.Cos(Mathf.Deg2Rad * lat1) *
            Mathf.Cos(Mathf.Deg2Rad * lat2) * Mathf.Sin(lon / 2) * Mathf.Sin(lon / 2);
        var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        var d = radius * c;

        return Math.Abs(d);
    }
}