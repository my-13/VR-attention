using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldObject : MonoBehaviour
{
    // This is a script given to every object that will be picked up

    public bool isCorrectObject = false;
    public string objectType = "None";
    public string objectName = "None";
    public Color objectColor = Color.white;

    // Start is called before the first frame update
    void Start()
    {
        
    }

}
