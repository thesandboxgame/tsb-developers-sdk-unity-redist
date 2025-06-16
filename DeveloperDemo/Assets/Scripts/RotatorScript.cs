using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatorScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float speed = 30f;
        transform.RotateAround(Vector3.zero, Vector3.up, Time.deltaTime * speed);
    }
}
