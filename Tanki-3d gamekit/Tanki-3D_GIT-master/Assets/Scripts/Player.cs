using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
public class Player : NetworkBehaviour
{
    [SyncVar]
    private bool _isDeath = true;
    public bool IsDeath
    {
        get { return _isDeath; }
        protected set { _isDeath = value; }
    }
    [SyncVar]
    private bool _isPlay = false; //Играем ли мы за какую-то из команд
    public bool isPlay
    {
        get { return _isPlay; }
        protected set { _isPlay = value; }
    }

    [SerializeField]
    private int maxHp;

    [SyncVar]
    public int hp;
    [SyncVar]
    public string nickname;
    [SyncVar]
    public int dead = 0;
    [SyncVar]
    public int killing = 0;
    [HideInInspector]
    public float GetHpToBar;
    [SyncVar]
    public bool inBattle;
    [SerializeField]
    public Collider[] colliders;

    [SerializeField]
    private GameObject[] disableGameObjectOnDeat;
    [SerializeField]
    private Behaviour[] disableOnDeath;
    [SyncVar]
    public int teamId = -1; // 0 - Red  1 - blue
    [SerializeField]
    private Color red;
    [SerializeField]
    private Color blue;
    [SerializeField]
    private MeshRenderer[] meshRenderersForColor;

    public void Setup()
    {

        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = false;
        }
        for (int i = 0; i < disableGameObjectOnDeat.Length; i++)
        {
            disableGameObjectOnDeat[i].SetActive(false);
        }
        GetComponent<Rigidbody>().isKinematic = true;
    }

    public void takeRegeneration(int _regeneration)
    {
        hp = (hp + _regeneration) > maxHp ? maxHp : (hp + _regeneration);
    }
    [Command]
    public void CmdSetupTeam(int _teamId)
    {
        RpcSetupTeam(_teamId);
    }
    [ClientRpc]
    public void RpcSetupTeam(int _teamId)
    {
        teamId = _teamId;
        StartCoroutine(DisableGFX());
        isPlay = true;
        CanvasLocal.canvasLocal.InstantiatePlayerStat();
    }

    public void takeDamage(int _damage)
    {
        hp = (hp - _damage) < 0 ? 0 : (hp - _damage);
        if (hp == 0) RpcDead();
    }

    public void SetDefaults()
    {
        GetComponent<Rigidbody>().isKinematic = false;
        hp = maxHp;
        IsDeath = false;
        if (isLocalPlayer)
        {
            for (int i = 0; i < disableOnDeath.Length; i++)
            {
                disableOnDeath[i].enabled = true;
            }
        }
        else
        {
            disableOnDeath[0].enabled = true; //Для удалённого игрока включать ТОЛЬКО TankController! 

            if (teamId == 0)
            {
                print("Регистрация 0");
                for (int i = 0; i < colliders.Length; i++)
                {
                    colliders[i].gameObject.layer = LayerMask.NameToLayer("Red"); // для правильной работы объекты должны иметь нужный layer
                }
            }
            if (teamId == 1)
            {
                print("Регистрация 1");
                for (int i = 0; i < colliders.Length; i++)
                {
                    colliders[i].gameObject.layer = LayerMask.NameToLayer("Blue"); // для правильной работы объекты должны иметь нужный layer
                }
            }
        }

        for (int i = 0; i < disableGameObjectOnDeat.Length; i++)
        {
            disableGameObjectOnDeat[i].SetActive(true);
        }
        if (teamId == 0)
        {
            meshRenderersForColor[0].materials[2].color = red;
            meshRenderersForColor[1].materials[0].color = red;
        }
        if (teamId == 1)
        {
            meshRenderersForColor[0].materials[2].color = blue;
            meshRenderersForColor[1].materials[0].color = blue;
        }
    }

    public void Update()
    {
        GetHpToBar = (float)hp / (float)maxHp;
        if (!IsDeath && disableOnDeath[0].enabled == false)
            SetDefaults(); //Вкл танка
    }
    [Command]
    public void CmdRename(string newName)
    {
        nickname = newName;
    }
    [ClientRpc]
    public void RpcDead()
    {
        dead++;
        IsDeath = true;
        StartCoroutine(DisableGFX());
    }

    IEnumerator DisableGFX()
    {
        if (_isPlay)
        {

            yield return new WaitForSeconds(GameManager.instance.matchSettings.disableTankTime); // Исчезновение танка
            for (int i = 0; i < disableOnDeath.Length; i++)
            {
                disableOnDeath[i].enabled = false;
            }
            for (int i = 0; i < disableGameObjectOnDeat.Length; i++)
            {
                disableGameObjectOnDeat[i].SetActive(false);
            }
            GetComponent<Rigidbody>().isKinematic = true;
            GetComponent<Turret>().RotationZero();
        }
        if (isLocalPlayer)
            StartCoroutine(spawn());
    }
    IEnumerator spawn()
    {
        Transform spawn;
        if (teamId == 0)
        {
            GameObject[] _spawn = GameObject.FindGameObjectsWithTag("SpawnRed");
            int rand = Random.Range(0, _spawn.Length);
            spawn = _spawn[rand].transform;
        }
        else
        {
            GameObject[] _spawn = GameObject.FindGameObjectsWithTag("SpawnBlue");
            int rand = Random.Range(0, _spawn.Length);
            spawn = _spawn[rand].transform;
        }

        transform.position = spawn.transform.position;
        transform.rotation = spawn.transform.rotation;
        yield return new WaitForSeconds(GameManager.instance.matchSettings.respawnTime);
        CmdDefault();
    }
    [Command]
    void CmdDefault()
    {
        RpcDefault();
    }
    [ClientRpc]
    void RpcDefault()
    {
        SetDefaults();
    }

}
