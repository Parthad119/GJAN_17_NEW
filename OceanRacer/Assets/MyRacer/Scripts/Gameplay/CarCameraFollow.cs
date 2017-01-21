/*
 * It Handles The Camera Follow Of The Racing Game
 */ 

using UnityEngine;
using System.Collections;

public class CarCameraFollow : MonoBehaviour 
{
	[SerializeField]private Transform cart = null; //Cart Transform
    [SerializeField] private float distance = 6.4f ;     	//Distance from car
    [SerializeField] private float height = 1.4f ;       	//Value on Y axis according to car transform
    [SerializeField] private float rotationDamping = 3.0f ; //lower the value , faster the damping will be
    [SerializeField] private float heightDamping = 2.0f;    //lower the value , faster the damping will be
    [SerializeField] private float zoomRatio = 0.5f;    	//Change on FOV
    [SerializeField] private float defaultFOV = 60f;    	//Min FOV
    [SerializeField] private float perspectiveAngle = 30f;  //Min FOV
    [SerializeField] private bool rotate = true;    //Look Back While Reversing
    private Vector3 rotationVector ;   //Rotation Vector


	void Start()
	{
		Vector3 position = transform.position;
		position.z = -5f ;
		transform.position = position ;
	}

	void FixedUpdate ()   
    {
        /*
        
        Vector3 localVilocity = car.InverseTransformDirection(car.GetComponent<Rigidbody>().velocity);
        if (localVilocity.z<-0.5 && rotate)
        {
            rotationVector.y = car.eulerAngles.y + 180;
        }
        else 
        {
            rotationVector.y = car.eulerAngles.y;
        }
        float acc = car.GetComponent<Rigidbody>().velocity.magnitude;
        GetComponent<Camera>().fieldOfView = DefaultFOV + acc * zoomRacio;
        */
    }

    void Update()
    {
		/*
        if(cart == null)
        {
            if(GameObject.FindWithTag("Player") != null)
            {
                cart = GameObject.FindWithTag("Player").transform ;
            }
        }
        else 
        {
            return ;
        }
		*/
    }

	void LateUpdate ()   
    {
        if(cart != null)
        {
            float wantedAngel = rotationVector.y ;
            float wantedHeight = cart.position.y + height ;
            float myAngel = transform.eulerAngles.y ;
            float myHeight = transform.position.y ;
            myAngel = Mathf.LerpAngle(myAngel,wantedAngel, rotationDamping * Time.deltaTime) ;  
            myHeight = Mathf.Lerp(myHeight,wantedHeight,heightDamping*Time.deltaTime);
            Quaternion currentRotation = Quaternion.Euler(0,myAngel,0);
            transform.position = cart.position;
            transform.position -= currentRotation * Vector3.forward * distance;
            Vector3 pos = transform.position ;
            pos.y = myHeight ;
            transform.position = pos;
            transform.LookAt(cart);
            Vector3 angles = transform.eulerAngles ;
            angles.x = perspectiveAngle ;
            transform.eulerAngles = angles ;
        }
	}

}
