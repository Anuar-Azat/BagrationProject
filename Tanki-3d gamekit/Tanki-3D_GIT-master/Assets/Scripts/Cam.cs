using UnityEngine;
using System.Collections;

public class Cam : MonoBehaviour
{
    public Transform Rotate;
    public Transform Target;
    [Header("Скорость следования за танком")]
    private float intens;
    [SerializeField]
    private float intensDefault = 0.15f;
    [SerializeField]
    private float intensMin = 0.02f;
    [Header("Скорость следования за вращением башни")]
    public float speed = 0.13f;
    public Player _player;
    void Start()
    {
        transform.parent = null;
    }
    private void Update()
    {
        if (_player.IsDeath) intens = intensMin;
        else intens = intensDefault;
    }

    void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, Target.position, intens);

        Vector3 relativePos = Rotate.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation,speed);
    }

}
