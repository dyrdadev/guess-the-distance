using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/City", order = 1)]
public class City : ScriptableObject {
    public string name;
    public double lat;
    public double lon;
}