using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;


public class CarMovement : MonoBehaviour
{
    public float speed = 10f;
    public Transform initialStartPoint;
    public Transform resetStartPoint;
    public Transform endPoint;

    private bool isInitialStart = true;
    
    // Start is called before the first frame update
    void Start()
    {
        transform.position = initialStartPoint.position;
    }

    

    // Update is called once per frame
    void Update()
    {
        Transform target = endPoint;
        
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.01f)
        {
            ResetCarPosition();
        }

        void ResetCarPosition()
        {
            if (isInitialStart)
            {
                transform.position = resetStartPoint.position;
                isInitialStart = false;
            }
            else
            {
                transform.position = resetStartPoint.position;
            }
        }
        
       
    }
    
}


