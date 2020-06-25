using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class Turret : NetworkBehaviour
{
    [Header("Башня")]
    public Transform turret;        //Башня
    public float speedTurret;       //Скорость поворота башни
    private Quaternion turretMiddle;
    private bool pressKeyC;

    [Header("Время разжега")]
    public float _timeIgnition;
    [Header("Время перезарядки")]
    public float _timeRecharge;
    public float _maxDistance;
    public float _forceReturn;
    public float _impactForce;
    [Header("Урон")]
    public int _damage;
    [Header("Все Layer`ы кроме противников")]
    public LayerMask layerMask;
    [Header("Layer противников")]
    public LayerMask layerMaskEnemy;
    public Transform gunpoint;
    public LineRenderer lineRenderer;
    public Camera cam;


    private bool _recharge = false;
    private bool _fire = true;
    private Animator _anim;
    private Rigidbody rb;
    private float valueBar;
    private Player _player;

    public GameObject fire;

    private Vector3 startPosGunpoint;
    void Start()
    {
        turretMiddle = turret.localRotation;
        rb = GetComponent<Rigidbody>();
        valueBar = _timeIgnition;
        _player = GetComponent<Player>();
        startPosGunpoint = gunpoint.localPosition;
    }

    public void Update()
    {
        if (_player.hp != 0)
        {
            if ((Input.GetButtonDown("Fire1") || Input.GetButton("Fire1")) && _fire) StartCoroutine(Ignition());

            Debug.DrawRay(gunpoint.position, gunpoint.forward * 200);

            if (Input.GetKeyDown(KeyCode.C)) pressKeyC = !pressKeyC;
            if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.X)) pressKeyC = false;
           // if (pressKeyC) TurretTurnMiddle();
        }
        else
        {
            pressKeyC = false;
        }
    }
    private void FixedUpdate()
    {
        float turRotation;
        if (_player.hp != 0)
        {
        ////////////////////////////////////////////////////////////////// 
        //БАШНЯ
        turRotation = Input.GetAxis("HorT") * speedTurret * Time.fixedDeltaTime;
        TurretRotation(turRotation);
        }
        else
        {
            turRotation = 0;
        }
    }
    void TurretRotation(float rotat) //Поворот башни!
    {
        turret.rotation *= turretMiddle * Quaternion.AngleAxis(rotat, Vector3.up);
    }

    IEnumerator Ignition()
    {
        //Разжег
        layerMaskEnemy = _player.teamId == 0 ? 1 << 11 : 1 << 10; //10 - Layer красной команды
        valueBar = _timeIgnition;
        _fire = false;
        fire.SetActive(true);
        CmdIgnition();
        yield return new WaitForSeconds(_timeIgnition);
        fire.SetActive(false);
        Shoot();
        yield return new WaitForSeconds(1.3f);
        lineRenderer.enabled = false;
        CmdLineVisibleFalse();
    }
    void Shoot()
    {
        gunpoint.localPosition = startPosGunpoint;
        cam.enabled = true;
        Vector3 target = gunpoint.forward;
        RaycastHit hitChek;
        string targetName = "";
        if (Physics.Linecast(turret.position, gunpoint.position,out hitChek, layerMask))
        {
            gunpoint.localPosition = new Vector3(gunpoint.localPosition.x, gunpoint.localPosition.y,-(Vector3.Distance(turret.position, gunpoint.position) - hitChek.distance));
        }
        else
        {
            
            if (GameManager.countPlayers() > 1) //Если игроков 2 и более
            {
                Collider[] allCol = GameManager.GetPlayersColliders(transform.name);
                List<Vector3> visible = new List<Vector3>();
                List<GameObject> playersVisible = new List<GameObject>();
                Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
                for (int i = 0; i < allCol.Length; i++)
                {
                    if (GeometryUtility.TestPlanesAABB(planes, allCol[i].bounds))
                    {
                        Vector3 center = planes[0].ClosestPointOnPlane(allCol[i].bounds.center);
                        Vector3 up = planes[0].ClosestPointOnPlane(allCol[i].bounds.max) - Vector3.up * 0.1f;
                        Vector3 down = planes[0].ClosestPointOnPlane(allCol[i].bounds.min) + Vector3.up * 0.1f;
                        Vector3 center2 = planes[0].ClosestPointOnPlane(allCol[i].bounds.center) - Vector3.up * 0.5f;
                        if (!Physics.Linecast(gunpoint.position, center, layerMask) && Physics.Linecast(gunpoint.position, center, layerMaskEnemy))
                        {
                            visible.Add(center);
                            playersVisible.Add(allCol[i].gameObject);
                            print("center");
                        }
                        else if (!Physics.Linecast(gunpoint.position, up, layerMask) && Physics.Linecast(gunpoint.position, up, layerMaskEnemy))
                        {
                            visible.Add(up);
                            playersVisible.Add(allCol[i].gameObject);
                            print("up");
                        }
                        else if (!Physics.Linecast(gunpoint.position, down, layerMask) && Physics.Linecast(gunpoint.position, down, layerMaskEnemy))
                        {
                            visible.Add(down);
                            playersVisible.Add(allCol[i].gameObject);
                            print("down");
                        }
                        else if (!Physics.Linecast(gunpoint.position, center2, layerMask) && Physics.Linecast(gunpoint.position, center2, layerMaskEnemy))
                        {
                            visible.Add(center2);
                            playersVisible.Add(allCol[i].gameObject);
                            print("center2");
                        }
                    }
                }
                if (visible.Count > 0)
                {
                    float[] y = new float[visible.Count]; //Массив вектора Y
                    for (int i = 0; i < visible.Count; i++)
                    {

                        Vector3 relativPos = visible[i] - gunpoint.position;
                        y[i] = Mathf.Abs(gunpoint.rotation.eulerAngles.x - Quaternion.LookRotation(relativPos).eulerAngles.x);
                        y[i] = y[i] > 30 ? Mathf.Abs(y[i] - 360) : y[i]; //Без этого не всегда корректно работает, а используя Quternion`Ы вообще не работает
                    }
                    for (int i = 0; i < y.Length; i++)
                    {
                        if (y[i] == Mathf.Min(y)) //Ищем минимальное значение
                        {
                            target = visible[i] - gunpoint.position; //Назначаем цель
                            targetName = playersVisible[i].transform.name;
                            break;
                        }
                    }
                }
            }
        }
        lineRenderer.enabled = true;
        RaycastHit hitInfo;
        if (Physics.Raycast(gunpoint.position, target, out hitInfo))
        {
            lineRenderer.SetPosition(0, gunpoint.position);
            lineRenderer.SetPosition(1, hitInfo.point);
        }
        else
        {
            lineRenderer.SetPosition(0, gunpoint.position);
            lineRenderer.SetPosition(1, transform.position + gunpoint.forward * _maxDistance);
        }
        rb.AddForceAtPosition(-turret.forward * _forceReturn, gunpoint.position ,ForceMode.VelocityChange);
        CmdShoot(target, targetName);

        cam.enabled = false;
        target = gunpoint.forward;
        //lineRenderer.enabled = true;
        Invoke("FireTrue", _timeRecharge);
        _recharge = true;
    }
    [Command]
    void CmdIgnition()
    {
        RpcIgnition();
    }
    [ClientRpc]
    void RpcIgnition()
    {
        if (isLocalPlayer) return;
        fire.SetActive(true);
    }
    [Command]
    void CmdShoot(Vector3 direction, string _PlayerID)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(gunpoint.position, direction, out hitInfo))
        {
            RpcShoot(gunpoint.position, hitInfo.point);
        }
        else
        {
            RpcShoot(gunpoint.position, transform.position + gunpoint.forward * _maxDistance);
        }
        if (_PlayerID != "")
        {
            Player _RemotePlayer = GameManager.GetPlayer(_PlayerID);
            _RemotePlayer.takeDamage(_damage);
            if (_RemotePlayer.hp == 0)
            {
                _player.killing++;
                RpcMessageKill(_player.nickname, _RemotePlayer.nickname);
            }
        }
    }
    [ClientRpc]
    void RpcShoot(Vector3 start, Vector3 point)
    {
        if (isLocalPlayer)
            return;
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, point);
        fire.SetActive(false);
    }
    [Command]
    void CmdLineVisibleFalse()
    {
        RpcLineVisibleFalse();
    }
    [ClientRpc]
    void RpcLineVisibleFalse()
    {
        lineRenderer.enabled = false;
    }
    [ClientRpc]
    void RpcMessageKill(string killer, string victim)
    {
        CanvasLocal.canvasLocal.Notification(killer + " уничтожил " + victim);
    }

    void FireTrue()
    {
        _fire = true;
        _recharge = false;
        valueBar = _timeIgnition;
    }
    public float GetRecharge()
    {
        if (valueBar < _timeRecharge && _recharge)
        {
            valueBar += Time.deltaTime;
            valueBar = valueBar > _timeRecharge ? _timeRecharge : valueBar;
            return valueBar / _timeRecharge;
        }
        else if (valueBar > 0 && !_fire)
        {
            _recharge = false;
            valueBar -= Time.deltaTime;
            valueBar = valueBar < 0 ? 0 : valueBar;
        }
        return valueBar / _timeIgnition;

    }

    public void RotationZero()
    {
        turret.localRotation = Quaternion.identity;
    }

}
