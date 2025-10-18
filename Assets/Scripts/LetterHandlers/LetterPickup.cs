using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterPickup : MonoBehaviour
{
    private Vector3 _startPosition;
    public float spinSpeed;
    public float bounceAmplitude;
    public float bounceSpeed;
    
    // Start is called before the first frame update
    void Start()
    {
        _startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 myPosition = transform.position;
        // float up and down
        myPosition.y = _startPosition.y + Mathf.Sin(Time.time*bounceSpeed) * bounceAmplitude;
        transform.position = myPosition;

        transform.Rotate(Vector3.up, Time.deltaTime * spinSpeed);
    }
}
