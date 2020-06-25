using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankPos : MonoBehaviour
{
    private Transform tank;
    public Transform TarCamRot;
    private Transform tarCam;
    [Header("Скорость следования за вращением башни вокруг оси танка")]
    public float speed = 0.13f;
    [Space]
    public float minAngle;
    public float maxAngle;
    public float speedCameraTarget;
    [Space]
    public float maxHight = -28;
    public float minHight = -11;
    [Space]
    public LayerMask layerWall;
    [Space]
    public float correction;

    private float angleHor;
    private Quaternion originRotation;
    [Header("Коррекция TarCam")]
    public Vector2 startPos;
    private Player _player;
    void Start()
    {
        transform.parent = null; //Выход из зависимости
        tarCam = transform.GetChild(0);
        tank = TarCamRot.transform.parent;
        transform.rotation = Quaternion.identity;
        originRotation = transform.rotation;
        tarCam.rotation = tank.rotation;
        _player = tank.parent.GetComponent<Player>();
    }

    void FixedUpdate()
    {
        transform.position = tank.position;

        angleHor += _player.IsDeath ? 0 : Input.GetAxis("QE") * speedCameraTarget; //Присваиваем вращение переменной
        angleHor = Mathf.Clamp(angleHor, minAngle, maxAngle); //Ограничиваем вращение
        Quaternion rotationX = Quaternion.AngleAxis(angleHor, Vector3.right); //Опускаем подымаем камеру
        Quaternion rotationY = Quaternion.AngleAxis(TarCamRot.eulerAngles.y, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation,originRotation * rotationY * rotationX, speed); //Присваем вращение к камере

        float normal = (angleHor + Mathf.Abs(minAngle)) / (Mathf.Abs(minAngle)+maxAngle); //Нормализованный вектор QE

        Vector3 vec = new Vector3(startPos.x, startPos.y, ((normal) * maxHight) - Mathf.Abs(minHight) + (Mathf.Abs(minHight) * normal));
        tarCam.localPosition = vec;
        
        RaycastHit ray;
        Vector3 start = TarCamRot.position - (transform.TransformPoint(vec) - TarCamRot.position) * correction;
        Debug.DrawLine(start, transform.TransformPoint(vec),Color.blue);
        if (Physics.Linecast(start, transform.TransformPoint(vec), out ray, layerWall))
        {
            tarCam.position = ray.point;
        }
    }
}
