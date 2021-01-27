﻿
using UnityEngine;

public class Singleton_GO<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T instance;

  
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                
                    instance = (T) FindObjectOfType(typeof(T));

                    if (instance == null)
                    {
                        Debug.LogWarning("An instance of " + typeof(T) +
                           " is needed in the scene, but there is none.");
                    }

                
            }

            return instance;
        }
    }
}

