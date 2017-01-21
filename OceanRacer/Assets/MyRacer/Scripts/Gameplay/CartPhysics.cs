/*
 * The Player Boat Physics Functionality is Implemented Here
 */ 

using UnityEngine;
using System.Collections;

public class CartPhysics : MonoBehaviour 
{
    [SerializeField] private float yOffset = -2f ;
    [SerializeField] private float gyroSpeed = 50.0F ;
    [SerializeField] private float []gearSpeed ;
    [SerializeField] private float []maxSpeedWRTGear ;
	[SerializeField] private float accelerometerThreshold = 0.2f ; //More The Tolerance Less The Sensitivity For Slight Tilt
	[SerializeField] private float accelerometerDirFactor = 1f ;
	[SerializeField] private float autoGearShiftForceFrameDelay = 0.2f ;
	[SerializeField] private float maxSpeedCap = 40 ;
	[SerializeField] private float smoothXVelNullify = 8f ;

    enum GEAR_TYPE
    {
       FIRST = 1,	
       SECOND = 2,	
       THIRD = 3,	
       FOURTH = 4	
    }

    private int gearNo = 0 ;		
    private bool checkSpeedDrop = false ;	
	//private RacingUI uiSystem ;	

	//Auto Gear Shift
	private bool shiftGear = false ;	
	private float startTime = 0 ;	
	private float currTime = 0 ;		

	//Debug
	private float accelerometerValue = 0 ;	

	private bool pauseMovement = true;
	private Vector3 prevVelocity = Vector3.zero ;	
	private Vector3 prevPosition = Vector3.zero ;	
	private AudioSource playerAudioSource ;	 
	private float maxBank = 90f ;
	private float gyroForce = 0 ;
    void Start()
    {
		//uiSystem = GameObject.Find("Canvas").GetComponent<RacingUI>() ;
		Vector3 startPos = Vector3.zero;
		startPos.y = yOffset;
		transform.position = startPos;
		pauseMovement = true ;
		playerAudioSource = GetComponent<AudioSource> ();
    }

	void FixedUpdate() 
    {
		if (pauseMovement)
		{
			GetComponent<Rigidbody> ().velocity = Vector3.zero;
			return;
		}
        
		//*********** HERE ********************	
		Vector3 gyroDir = Vector3.zero ; 
		if(Application.isEditor && Application.isPlaying)
        {
			//***********Key Board Input***********
			accelerometerValue = Input.GetAxis("Horizontal") ; 
			if (accelerometerValue >= (accelerometerThreshold * (-1)) && accelerometerValue <= accelerometerThreshold)	 
			{
				//gyroDir.x = 0 ;			
				Vector3 nullifyForce = transform.GetComponent<Rigidbody> ().velocity ; 		
				nullifyForce.x = Mathf.Lerp(nullifyForce.x , 0, smoothXVelNullify * Time.deltaTime ) ;			 
				transform.GetComponent<Rigidbody>().velocity = nullifyForce ;
			}
			else 
			{
				accelerometerValue *= accelerometerDirFactor ;		
				gyroDir.x = accelerometerValue ;	
			}
		}
        else
        {
			//***********ACCELEROMETER Works Only On Device*********
			accelerometerValue = Input.acceleration.x ; 
			if(accelerometerValue >= (accelerometerThreshold * (-1)) && accelerometerValue <= accelerometerThreshold)
			{
				//gyroDir.x = 0 ;	
				Vector3 nullifyForce = transform.GetComponent<Rigidbody> ().velocity ;  	
				nullifyForce.x = Mathf.Lerp(nullifyForce.x , 0, smoothXVelNullify * Time.deltaTime) ;
				transform.GetComponent<Rigidbody> ().velocity = nullifyForce ;		
			}
			else 
			{
				accelerometerValue *= accelerometerDirFactor ;		
				gyroDir.x = accelerometerValue ;	
			}
		}

        if(gyroDir.sqrMagnitude > 1)  
            gyroDir.Normalize() ;   
    	    
		gyroDir *= Time.fixedDeltaTime ; 
		//transform.GetComponent<Rigidbody>().AddForce(gyroDir * gyroSpeed) ; //This Is Only Gyro Force									
																			  //More The Force ... Quicker The Left & Right Navigation


		transform.GetComponent<Rigidbody>().AddForce(gyroDir * gyroForce) ; //This Is Only Gyro Force
																			//More The Force ... Quicker The Left & Right Navigation
	}

	void Update()
 	{
		//Debug.Log ("StartBtnPressed " + pauseMovement) ;		
//		if(uiSystem.IsCountDownComplete)
//		{
//			pauseMovement = false ;		
//			uiSystem.IsCountDownComplete = false ;	
//			GearShift() ;	
//		}

		if (pauseMovement)
		{
			return ;	
		}
		
		if (checkSpeedDrop)
		{
			CheckSpeedDrop () ;	
			AutoSpeedIncrease () ;	
		}
				
		ClampMaxSpeedCap () ;	
		CalcGYROForce () ;  	
		//uiSystem.UpdateSpeedVal(GetComponent<Rigidbody> ().velocity.z , maxSpeedCap) ; //Updating The UI Speed Meter
		Rotate() ;
 	}

	void LateUpdate()
	{
		RunningBoatAudioEffect ();
	}

	void AutoSpeedIncrease()
	{
		if (gearNo < (int)GEAR_TYPE.FOURTH && !shiftGear) 
		{
			shiftGear = true ;
			currTime = startTime = Time.time ;	
		}

		if (shiftGear) 
		{
			if (gearNo == (int)GEAR_TYPE.FOURTH) 
			{
				shiftGear = false ;			
			}
			else
			{
				currTime = Time.time ;
				float timeDiff = Mathf.Abs (currTime - startTime) ;	
				if (timeDiff >= autoGearShiftForceFrameDelay)
				{
					if (gearNo == 1)
					{
						AddForce (GEAR_TYPE.SECOND)  ;			
						gearNo = (int)GEAR_TYPE.SECOND ;				 
					} 
					else if (gearNo == 2) 
					{
						AddForce (GEAR_TYPE.THIRD) ;			
						gearNo = (int)GEAR_TYPE.THIRD ;	
					} 
					else if (gearNo == 3) 
					{
						AddForce (GEAR_TYPE.FOURTH) ;				
						gearNo = (int)GEAR_TYPE.FOURTH ;			
					}
												
//					else if (gearNo == 4) 
//					{
//						AddForce (GEAR_TYPE.FOURTH) ;	
//						gearNo = (int)GEAR_TYPE.FOURTH ; 	
//						Debug.Log ("Four..") ;	
//					}
														
					startTime = Time.time ;					 		
				}
			}
		}
	}

	void CheckSpeedDrop()
    {
        float currSpeed = GetComponent<Rigidbody>().velocity.magnitude ;

        if(currSpeed < maxSpeedWRTGear[(int)GEAR_TYPE.FIRST - 1])
        {
            AddForce(GEAR_TYPE.FIRST) ;   
        }

        if(currSpeed < maxSpeedWRTGear[(int)GEAR_TYPE.SECOND - 1] && (gearNo == 2))
        {
            gearNo = (int)GEAR_TYPE.FIRST ;     
        }

        if(currSpeed < maxSpeedWRTGear[(int)GEAR_TYPE.THIRD - 1] && (gearNo == 3))
        {
            gearNo = (int)GEAR_TYPE.SECOND ;             
        }

        if(currSpeed < maxSpeedWRTGear[(int)GEAR_TYPE.FOURTH - 1] && (gearNo == 4))
        {
            gearNo = (int)GEAR_TYPE.THIRD ;
        }
	}

    void AddForce(GEAR_TYPE gear_)
    {
        Vector3 forwardDir = Vector3.zero ;    
        forwardDir.z = 1 ; 
        if(forwardDir.sqrMagnitude > 1)   
            forwardDir.Normalize() ; 
        
        forwardDir *= Time.deltaTime ;       

        switch(gear_)
        {
            case GEAR_TYPE.FIRST:
                {
                    transform.GetComponent<Rigidbody>().AddForce(forwardDir * gearSpeed[(int)GEAR_TYPE.FIRST - 1]) ;
					checkSpeedDrop = true ; 
                }
                break ;
            case GEAR_TYPE.SECOND:
                {
                    transform.GetComponent<Rigidbody>().AddForce(forwardDir * gearSpeed[(int)GEAR_TYPE.SECOND - 1]) ; 
                }
                break ;	
            case GEAR_TYPE.THIRD:
                {
                    transform.GetComponent<Rigidbody>().AddForce(forwardDir * gearSpeed[(int)GEAR_TYPE.THIRD - 1]) ; 
                }
                break ;	
            case GEAR_TYPE.FOURTH:
                {
                    transform.GetComponent<Rigidbody>().AddForce(forwardDir * gearSpeed[(int)GEAR_TYPE.FOURTH - 1]) ; 
                }
                break ;		
        }
    }

    void GearShift()
    {
        gearNo++ ;	
        if(gearNo > 4)
        {
           gearNo = 4 ;        
        }
        else
        {
            if(gearNo == 1)
            {
               AddForce(GEAR_TYPE.FIRST) ;
            }
			/*
	            else if(gearNo == 2)	
	            {
	               AddForce(GEAR_TYPE.SECOND) ;		
	            }
	            else if(gearNo == 3)	
	            {
	               AddForce(GEAR_TYPE.THIRD) ;		
	            }
	            else if(gearNo == 4)	
	            {
	               AddForce(GEAR_TYPE.FOURTH) ;	            
	            }
			*/
            //uiSystem.ShowGearText(gearNo) ;	
        }
    }

	void ClampMaxSpeedCap()
	{
		if (GetComponent<Rigidbody> ().velocity.z > maxSpeedCap)
		{
			Vector3 currVel = GetComponent<Rigidbody> ().velocity;
			currVel.z = maxSpeedCap;
			GetComponent<Rigidbody> ().velocity = currVel ;
		}
	}

	void CalcGYROForce()
	{
		gyroForce = (float)(gyroSpeed / maxSpeedCap) * GetComponent<Rigidbody> ().velocity.z ;
		gyroForce *= 2f ;
		gyroForce = (gyroForce > gyroSpeed) ? gyroSpeed : gyroForce ;
	}

	void Rotate()
	{
		Vector3 currPos = transform.position ;   
		Vector3 dir = currPos - prevPosition ; 

		if (dir.sqrMagnitude > 1)
				dir.Normalize ();

		Vector3 target = currPos + dir ;

		Vector3 targetDir = target - currPos ;
		if(targetDir != Vector3.zero)
		{
			float currSpeed = GetComponent<Rigidbody> ().velocity.z;
			currSpeed = currSpeed * (float)( 15 / 100f);
			var newRotation = Quaternion.LookRotation (target - currPos);

			//Banking
			float bank = maxBank * -Vector3.Dot(transform.right, dir) ;
			Quaternion banking = Quaternion.AngleAxis (bank, Vector3.forward) ;
			newRotation = newRotation * banking;
			//Banking

			newRotation.x = 0;
			newRotation.y = 0;
			//newRotation.z = 0 ;
			transform.rotation = Quaternion.Slerp(transform.rotation , newRotation, Time.deltaTime * currSpeed);
		}
		prevPosition = transform.position ;	
		prevVelocity = GetComponent<Rigidbody>().velocity ;
	}

	public void LevelEndPause_Movement()
	{
		prevVelocity = transform.GetComponent<Rigidbody> ().velocity;
		transform.GetComponent<Rigidbody> ().velocity = Vector3.zero;
		//uiSystem.UpdateSpeedVal (0, maxSpeedCap);
		pauseMovement = true;
	}

	public void PauseMovement()
	{
		prevVelocity = transform.GetComponent<Rigidbody> ().velocity ;
		transform.GetComponent<Rigidbody> ().velocity = Vector3.zero ;
		//uiSystem.UpdateSpeedVal (0, maxSpeedCap);
		pauseMovement = true;
	}

	public void ResumeMovement()
	{
		transform.GetComponent<Rigidbody> ().velocity = prevVelocity;
		pauseMovement = false ;
	}

	public void ResetMovement()
	{
		transform.GetComponent<Rigidbody> ().velocity = Vector3.zero ;
		transform.eulerAngles = Vector3.zero ;
	}

	void RunningBoatAudioEffect()
	{
		Vector3 currVel = GetComponent<Rigidbody> ().velocity;

		if (!playerAudioSource.isPlaying)
		{
			if (currVel.z > 0)
			{
				playerAudioSource.Play ();
			}
		}
		else
		{
			if (currVel.z > 0.1f)
			{
				float initialPitch = 0.1f ;	
				float maxPitch = 3.0f ;
				float pitchLength = Mathf.Abs (maxPitch - initialPitch) ;
				float speedInPercntg = (float)(100f / maxSpeedCap) * currVel.z ;
				float pitch = initialPitch + (float)(pitchLength * speedInPercntg) / 100f ; 
				playerAudioSource.pitch = pitch ;	
			} 
			else
			{
				playerAudioSource.Stop ();
			}
		}
	}


	void OnCollisionEnter(Collision collisionInfo) 
	{
				
	}

	void OnCollisionStay(Collision collisionInfo) 
	{
		//****When Collided On the Sides***
		//Debug.Log ("Colliding....") ;	
		Vector3 currVel = transform.GetComponent<Rigidbody> ().velocity;
		currVel.x = 0;
		transform.GetComponent<Rigidbody> ().velocity = currVel;
	}

	void OnCollisionExit(Collision collisionInfo) 
	{
		
	}


    //Debug
    /*
    void OnGUI()
    {
        float btnWidth  = Screen.width * 0.2f ;
        float btnHeight = Screen.width * 0.2f ;

        if(GUI.Button(new Rect(Screen.width - btnWidth, Screen.height - btnHeight, btnWidth, btnHeight) , ("GEAR: "+gearNo.ToString())))
        {
            gearNo++ ;
            if(gearNo > 4)
            {
                gearNo = 4 ;        
            }
            else
            {
                if(gearNo == 1)
                {
                    AddForce(GEAR_TYPE.FIRST) ; 
                }
                else if(gearNo == 2)
                {
                    AddForce(GEAR_TYPE.SECOND) ;
                }
                else if(gearNo == 3)
                {
                    AddForce(GEAR_TYPE.THIRD) ;
                }
                else if(gearNo == 4)
                {
                    AddForce(GEAR_TYPE.FOURTH) ;            
                }
            }
        }
        Debug.Log("GearNo: "+gearNo) ;
        float speed  = transform.GetComponent<Rigidbody>().velocity.magnitude ;
        GUI.Box(new Rect((Screen.width - btnWidth), (Screen.height - (btnHeight * 1.12f)), btnWidth, btnHeight) , ("SPEED: "+speed.ToString())) ;
        //GUI.Label(new Rect((Screen.width - btnWidth), (Screen.height - (btnHeight * 1.1f)), btnWidth, btnHeight), ("SPEED: "+speed.ToString())) ;
    }

	void OnGUI()
	{
		GUI.color = Color.red;
		GUIStyle style = new GUIStyle ();
		style.fontSize = 50;
		GUI.Label (new Rect (Screen.width - 200, Screen.height * 0.5f, 100, 100), Input.acceleration.x.ToString (), style) ;	
	}
    */
}
