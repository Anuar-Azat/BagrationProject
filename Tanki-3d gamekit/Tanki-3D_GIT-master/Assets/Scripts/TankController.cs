using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class TankController : NetworkBehaviour
{
    [Header("Физические колёса")]
    public WheelCollider[] wheelCollidersLeft;
    public WheelCollider[] wheelCollidersRight;
    [Header("Видимые колёса")]
    public Transform[] wheelMeshLeft;          //Левый мЭш колеса  Ось колёс
    public Transform[] wheelMeshRight;         //правый Мэш колеса //  

    public float maxSpeed;
    public float maxMotorTorque;        //Крутящий момент
    public float auxiliaryForce = 10;
    public float Steering;      // Скорость вращения гусениц при развороте на месте
    public float steeringBrake;
    [Header("Доп. сила вращения на месте")]
    public float auxiliaryRotationForceInPlace = 10;
    [Header("Доп. сила вращения в движении")]
    public float auxiliaryRotationForceInMotion = 5;
    [Space]
    //   public AnimationCurve engineTorqueCurve;
    public float decelerationForce;     //Сила торможения
    public float downWheel;
    [Header("Коррекция положения катков")]
    public float WheelPosY;
    public float maxDownPressure;
    [Space]
    public Transform[] leftJoints = new Transform[5];
    public Transform[] rightJoints = new Transform[5];

    private Vector3[] leftJointsPos = new Vector3[5];
    private Vector3[] rightJointsPos = new Vector3[5]; //Начальная позиция костей

    [Header("Гусеницы")]
    public Renderer leftTrackRender;
    public Renderer rightTrackRender;
    public float trackTextureSpeed = 2.5f;
    private float leftTrackTextureOffset = 0f;
    private float rightTrackTextureOffset = 0f;

    [Header("Направляющие катки")]
    public Transform[] wheelLeadingLeft;
    public Transform[] wheelLeadingRight;
    [Header("Центр тяжести")]
    public Transform centerMass;
    private Rigidbody rigidBody;
    [HideInInspector]
    public float curentSpeed;
    [HideInInspector]
    public bool reversForward;
    [HideInInspector]
    public bool reversBack;
    float _hor;
    [HideInInspector]
    public float rpm;
    public void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        for (int i = 0; i < wheelCollidersLeft.Length; i++)
        {

            leftJointsPos[i] = leftJoints[i].localPosition;
            rightJointsPos[i] = rightJoints[i].localPosition;
        }
    }

    void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime;
        ///////////////////////////////////////
        float trackRpm;
        trackRpm = CalculateSmoothRpm(wheelCollidersLeft);
        leftTrackTextureOffset = Mathf.Repeat(leftTrackTextureOffset + delta * trackRpm * trackTextureSpeed / 60.0f, 1.0f);
        leftTrackRender.material.SetTextureOffset("_MainTex", new Vector2(0, -leftTrackTextureOffset));

        trackRpm = CalculateSmoothRpm(wheelCollidersRight);
        rightTrackTextureOffset = Mathf.Repeat(rightTrackTextureOffset + delta * trackRpm * trackTextureSpeed / 60.0f, 1.0f);
        rightTrackRender.material.SetTextureOffset("_MainTex", new Vector2(0, -rightTrackTextureOffset));

        for (int i = 0; i < wheelCollidersLeft.Length; i++)
        {
            ApplyLocalPositionToVisuals(wheelCollidersLeft[i], wheelMeshLeft[i], wheelLeadingLeft);//Лево
            ApplyLocalPositionToVisuals(wheelCollidersRight[i], wheelMeshRight[i], wheelLeadingRight);//Право
        }

    }

    public void ApplyLocalPositionToVisuals(WheelCollider wheelCollider, Transform wheelMesh, Transform[] wheelLeading) //Помещаем видимые колёса в позицую физических
    {
        Vector3 position; //Локальный переменные
        Quaternion rotation;
        wheelCollider.GetWorldPose(out position, out rotation); //Вписываем в переменные позицию и поворот физических колёс
        position.y += WheelPosY;
        wheelMesh.position = position; // Присваеваем соответствующему мешу колеса позицию физическому колеса
        wheelMesh.rotation = rotation;
        for (int i = 0; i < wheelLeading.Length; i++)
        {
            wheelLeading[i].rotation = rotation;
        }
    }
    public void FixedUpdateTwo(float Ver, float Hor) // Локальный
    {
        float motor = maxMotorTorque * Ver; //Сразу создаём локальную переменную с Осью W/S, A/D
        float steering = Steering * Hor;
        _hor = Hor;
        for (int i = 0; i < wheelCollidersLeft.Length; i++)
        {
            Acceleration(wheelCollidersLeft[i], motor, steering);
            Acceleration(wheelCollidersRight[i], motor, -steering);
        }
        if (wheelCollidersLeft[2].isGrounded || wheelCollidersRight[2].isGrounded) // Доп. сила вращения
        {
            if (Mathf.Abs(rigidBody.angularVelocity.y) < 2f)
            {
                if (curentSpeed < 11)
                    rigidBody.AddRelativeTorque((Vector3.up * Hor) * auxiliaryRotationForceInPlace, ForceMode.Acceleration);
                else
                    rigidBody.AddRelativeTorque((Vector3.up * Hor) * auxiliaryRotationForceInMotion, ForceMode.Acceleration);
            }
        }
        if (CalculateSmoothRpm(wheelCollidersLeft) > 100 && motor < 0 && Mathf.Abs(Hor) < 0.1f) reversForward = true;
        if (CalculateSmoothRpm(wheelCollidersLeft) < -100 && motor > 0 && Mathf.Abs(Hor) < 0.1f) reversBack = true;

        if (motor == 0.0f && (wheelCollidersLeft[2].isGrounded || wheelCollidersRight[2].isGrounded))
        {
            rigidBody.drag = 1;
        }
        else
        {
            rigidBody.drag = 0;
        }

        if ((wheelCollidersLeft[2].isGrounded || wheelCollidersRight[2].isGrounded) && curentSpeed < maxSpeed) // Доп.сила движения
        {
            rigidBody.AddRelativeForce(Vector3.forward * (Ver * auxiliaryForce), ForceMode.Acceleration);
        }

    }
    [Command]
    public void CmdFixedUpdateTwo(float Ver, float Hor)
    {
        RpcFixedUpdateTwo(Ver, Hor);
    }
    [ClientRpc]
    public void RpcFixedUpdateTwo(float Ver, float Hor)
    {
        float motor = maxMotorTorque * Ver; //Сразу создаём локальную переменную с Осью W/S, A/D
        float steering = Steering * Hor;

        for (int i = 0; i < wheelCollidersLeft.Length; i++)
        {
            Acceleration(wheelCollidersLeft[i], motor, steering);
            Acceleration(wheelCollidersRight[i], motor, -steering);
        }
    }

    private void Update()
    {
        rigidBody.centerOfMass = centerMass.localPosition; //Центр масс
        for (int i = 0; i < wheelMeshLeft.Length; i++) //Синхронизация костей и колес
        {
            SyncJoins(leftJoints[i], wheelMeshLeft[i].transform.localPosition.y, leftJointsPos[i]);
            SyncJoins(rightJoints[i], wheelMeshRight[i].transform.localPosition.y, rightJointsPos[i]);
        }
        curentSpeed = rigidBody.velocity.magnitude * 3;
        if (curentSpeed < 3 || !wheelCollidersLeft[3].isGrounded || !wheelCollidersRight[3].isGrounded)
        {
            reversForward = reversBack = false;
        }
        rpm = CalculateSmoothRpm(wheelCollidersLeft);
    }
    void Acceleration(WheelCollider col, float motor, float steering) //Движение
    {

        if (curentSpeed > maxSpeed || !col.isGrounded)
        {
            col.motorTorque = 0;
        }
        else
        {
            if (motor == 0.0f)
            {
                col.motorTorque = steering;
                if (5 > curentSpeed && steering != 0)
                    rigidBody.velocity = Vector3.zero;
            }
            else
            {
                if (steering < 0 && motor > 0 || steering > 0 && motor < 0) //Если steering < 0 на правой гусенице то мы поварачиваем на право и наоборот
                { 
                    col.motorTorque = 0;
                }
                else if ((steering > 0 && motor > 0) || (steering < 0 && motor < 0))
                {
                    col.motorTorque = motor * 1.04f;
                }
                else 

                {
                    col.motorTorque = motor;
                }
            }
            col.brakeTorque = 0;
        }

        if ((motor == 0 && steering == 0) || reversForward || reversBack)
        {
            col.motorTorque = 0;
            col.brakeTorque = decelerationForce;
        }
        // Прижимная сила
        // col.center = new Vector3(0, (curentSpeed/maxSpeed * maxDownPressure) + (minDownPressure * (1 - (curentSpeed / maxSpeed))), 0);
        col.forceAppPointDistance = reversForward || reversBack || curentSpeed < 25f ? 0 : curentSpeed / maxSpeed * maxDownPressure;
    }

    private float CalculateSmoothRpm(WheelCollider[] w)
    { 
        float rpm = 0.0f;

        List<int> grWheelsInd = new List<int>();

        for (int i = 0; i < w.Length; i++)
        { 
            if (w[i].isGrounded)
            {  
                grWheelsInd.Add(i); 
            }
        }

        if (grWheelsInd.Count == 0)
        {  
            foreach (WheelCollider wd in w)
            {  
                rpm += wd.rpm; 			 
            }

            rpm /= w.Length;

        }
        else
        {  

            for (int i = 0; i < grWheelsInd.Count; i++)
            {  
                rpm += w[grWheelsInd[i]].rpm; 
            }

            rpm /= grWheelsInd.Count; 
        }

        return rpm; 
    }

    void SyncJoins(Transform Join, float y, Vector3 startPos)
    {
        Join.localPosition = new Vector3(startPos.x, y - downWheel, startPos.z);
    }


}
