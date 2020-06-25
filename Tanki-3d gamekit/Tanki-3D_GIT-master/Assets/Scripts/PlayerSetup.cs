using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Player))]
public class PlayerSetup : NetworkBehaviour {

    [SerializeField]
    Behaviour[] componentsToDisable;
    [SerializeField]
    GameObject[] gameObjectsToDisable;

    private TankController _tankController;
    private Player _player;
    public override void OnStartClient()
    {
        base.OnStartClient();
        string _netID = GetComponent<NetworkIdentity>().netId.ToString();
        Player _player = GetComponent<Player>();
        GameManager.RegisterPlayer(_netID, _player, GetComponent<Player>().colliders);
        print("Connect " + transform.name);
    }

    private void Start()
    {
        _player = GetComponent<Player>();
        if (!isLocalPlayer)
        {
            for (int i =0; i < componentsToDisable.Length; i++)
            {
                componentsToDisable[i].enabled = false;
            }
            for (int i = 0; i < gameObjectsToDisable.Length; i++)
            {
                gameObjectsToDisable[i].SetActive(false);
            }
        }
        else
        {
            CanvasLocal.canvasLocal.Setup(_player);
            _tankController = GetComponentInChildren<TankController>();
            _player.CmdRename(PlayerPrefs.GetString("Name"));
            GetComponentInChildren<CanvasPlayer>().gameObject.SetActive(false);
        }
        _player.Setup();
        CanvasLocal.canvasLocal.InstantiatePlayerStat();
        Invoke("ShowNotification", 0.2f);
    }
    void ShowNotification()
    {
        if (!_player.inBattle)//Для того что бы игроку не выводилось сообщения о уже находящихся в битве
            CanvasLocal.canvasLocal.Notification(_player.nickname + " встпупил в битву");
        _player.inBattle = true;
    }

    //Под сомнением!!!! _______________-----------------------
    private void FixedUpdate()
    {
        if (_player.hp != 0)
        {
            _tankController.FixedUpdateTwo(Input.GetAxis("Ver"), Input.GetAxis("Hor"));
            _tankController.CmdFixedUpdateTwo(Input.GetAxis("Ver"), Input.GetAxis("Hor"));
        }
        else
        {
            _tankController.FixedUpdateTwo(0,0);
            _tankController.CmdFixedUpdateTwo(0,0);
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete) && !_player.IsDeath) CmdDelete();
    }
    [Command]
   public void CmdDelete()
    {
        _player.takeDamage(_player.hp);
        RpcDelete();
    }
    [ClientRpc]
   public void RpcDelete()
    {
        CanvasLocal.canvasLocal.Notification(_player.nickname+" самоуничтожился");
    }

    private void OnDestroy()
    {
        if (!isLocalPlayer)
        CanvasLocal.canvasLocal.Notification(string.Format("{0} покинул беседу", _player.nickname));

        GameManager.UnRegisterPlayer(transform.name);
        if (CanvasLocal.canvasLocal == null) return;
        CanvasLocal.canvasLocal.InstantiatePlayerStat();
    }

    public void Disconnected()
    {
        if (isServer)
        {
            NetworkManager.singleton.StopHost();
            print("Стоп хост");
        }
        else
        {
            NetworkManager.singleton.StopClient();
            print("Стоп клиент");
        }

        foreach (GameObject a in gameObjectsToDisable)
        {
            Destroy(a);
        }
    }

    void OnGUI()
    {
        GUIStyle myStyle = new GUIStyle();
        myStyle.fontSize = 32;
        myStyle.normal.textColor = Color.black;
        GUI.Label(new Rect(10, 10, 200, 200), "spd: " + (int)_tankController.curentSpeed, myStyle);

        GUI.Label(new Rect(10, 200, 200, 200), "rpm: " + (int)_tankController.rpm, myStyle);
        GUI.Label(new Rect(10, 100, 200, 200), "revers: " + (_tankController.reversBack || _tankController.reversForward), myStyle);
    }

}
