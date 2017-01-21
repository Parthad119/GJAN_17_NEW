using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoatController : Boyancy
{

	[Header("Physic :")]
	[SerializeField] private float m_accelerationFactor = 2.0F;
	[SerializeField] private float m_turningFactor = 2.0F;
	[SerializeField] private float m_accelerationTorqueFactor = 35F;
	[SerializeField] private float m_turningTorqueFactor = 35F;

	[Header("Audio :")]
	[SerializeField] private bool m_enableAudio = true;
	[SerializeField] private AudioSource m_boatAudioSource;
	[SerializeField] private float m_boatAudioMinPitch = 0.4F;
	[SerializeField] private float m_boatAudioMaxPitch = 1.2F;

	[Header("Other :")]
	[SerializeField] private List<GameObject> m_motors;

	private float m_verticalInput = 0F;
	private float m_horizontalInput = 0F;
    private Rigidbody m_rigidbody;
	private Vector3 m_androidInputInit;

	private float gyroForce = 0 ;

    protected override void Start()
    {
        base.Start();

        m_rigidbody = GetComponent<Rigidbody>();
        m_rigidbody.drag = 1;
        m_rigidbody.angularDrag = 1;

		initPosition ();
	}

	public void initPosition()
	{
		#if UNITY_ANDROID
		m_androidInputInit = Input.acceleration;
		#endif
	}

	void Update()
	{
		#if UNITY_EDITOR 
		setInputs (Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal"));
		#elif UNITY_ANDROID
		Vector3 touchInput = Input.acceleration - m_androidInputInit;
		
		if (touchInput.sqrMagnitude > 1)
			touchInput.Normalize();
		
		setInputs (-touchInput.y, touchInput.x);
		#endif

		//Rotate ();
	}

	public void setInputs(float iVerticalInput, float iHorizontalInput)
	{
		//m_verticalInput = iVerticalInput;
		m_verticalInput = 1;
		m_horizontalInput = iHorizontalInput;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		m_rigidbody.AddRelativeForce(Vector3.forward * m_verticalInput * m_accelerationFactor);

		Vector3 gyroDir = Vector3.zero ;  
		gyroDir.x = m_horizontalInput ;
		if(gyroDir.sqrMagnitude > 1)  
			gyroDir.Normalize() ; 
		m_rigidbody.AddForce(gyroDir * 5f) ;

		/*
		m_rigidbody.AddRelativeTorque(
			m_verticalInput * -m_accelerationTorqueFactor,
			m_horizontalInput * m_turningFactor,
			m_horizontalInput * -m_turningTorqueFactor
        );
		*/

		m_rigidbody.AddRelativeTorque(
			0,
			m_horizontalInput * m_turningFactor,
			m_horizontalInput * -m_turningTorqueFactor
		);



		/*
        if(m_motors.Count > 0)
        {
            float motorRotationAngle = 0F;
			float motorMaxRotationAngle = 70;

			motorRotationAngle = - m_horizontalInput * motorMaxRotationAngle;

            foreach (GameObject motor in m_motors)
            {
				float currentAngleY = motor.transform.localEulerAngles.y;
				if (currentAngleY > 180.0f)
					currentAngleY -= 360.0f;

				float localEulerAngleY = Mathf.Lerp(currentAngleY, motorRotationAngle, Time.deltaTime * 10);
				motor.transform.localEulerAngles = new Vector3(
					motor.transform.localEulerAngles.x,
					localEulerAngleY,
					motor.transform.localEulerAngles.z
				);
            }
        }
		*/

		if (m_enableAudio && m_boatAudioSource != null) 
		{
            m_boatAudioSource.enabled = m_verticalInput != 0;

            float pitchLevel = m_verticalInput * m_boatAudioMaxPitch;
			if (pitchLevel < m_boatAudioMinPitch)
				pitchLevel = m_boatAudioMinPitch;
			float smoothPitchLevel = Mathf.Lerp(m_boatAudioSource.pitch, pitchLevel, Time.deltaTime);

			m_boatAudioSource.pitch = smoothPitchLevel;
		}
    }

	private float maxBank = 90f ;
	private Vector3 prevPosition = Vector3.zero ;
	private Vector3 prevVelocity = Vector3.zero ;
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
			//float currSpeed = GetComponent<Rigidbody> ().velocity.z;
			float currSpeed = GetComponent<Rigidbody> ().velocity.magnitude;
			//currSpeed = currSpeed * (float)( 15 / 100f);
			var newRotation = Quaternion.LookRotation (target - currPos);

			//Banking
			float bank = maxBank * -Vector3.Dot(transform.right, dir) ;
			Quaternion banking = Quaternion.AngleAxis (bank, Vector3.forward) ;
			newRotation = newRotation * banking ;
			//Banking

			newRotation.x = 0 ;
			newRotation.y = 0 ;
			//newRotation.z = 0 ;
			transform.rotation = Quaternion.Slerp(transform.rotation , newRotation, Time.deltaTime * currSpeed);
			//transform.rotation = Quaternion.Slerp(transform.rotation , newRotation, Time.deltaTime * 50);
		}
		prevPosition = transform.position ;	
		prevVelocity = GetComponent<Rigidbody>().velocity ;
	}
}
