using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
public class CanvasLocal : NetworkBehaviour
{

    public static CanvasLocal canvasLocal;

    [SerializeField]
    private RectTransform healthBar;
    [SerializeField]
    private RectTransform rechargeBar;
    [SerializeField]
    private GameObject statsPanel;
    [SerializeField]
    private GameObject prefabPlayerPanel;
    [SerializeField]
    private Transform targetInstantiatePlayerPanelRed;
    [SerializeField]
    private Transform targetInstantiatePlayerPanelBlue;
    [SerializeField]
    private Transform targetInstantiatePlayerPanelSpect;
    [SerializeField]
    private GameObject notificationPanelPrefab;
    [SerializeField]
    private Transform targetInstantiateNotificationPanel;
    [SerializeField]
    private GameObject chooseTeam;


    [Header("Цвета бара")]
    public Color redTeam;
    public Color blueTeam;

    private Player player;
    private Turret turret;
    private static Dictionary<string, PlayerStatPanel> playerPanels = new Dictionary<string, PlayerStatPanel>();
    private void Awake()
    {
        if (canvasLocal == null) canvasLocal = this;
        chooseTeam.SetActive(true);
    }

    public void Setup(Player _player)
    {
        player = _player;
        GetComponent<Canvas>().enabled = true; // Если кансвас остается включенным то выбивает ошибку, она не на что не влияет но неприятно
        turret = player.GetComponent<Turret>();
    }

    public void SelectTeam(int teamId) // 0 = Red //  1 = Blue
    {
        if (teamId != player.teamId)
        {
            player.CmdSetupTeam(teamId);
            if (player.isPlay)
                player.GetComponent<PlayerSetup>().CmdDelete();
            chooseTeam.SetActive(false);
            healthBar.GetComponent<Image>().color = teamId == 0 ? redTeam : blueTeam;
            healthBar.parent.gameObject.SetActive(true);
            rechargeBar.parent.gameObject.SetActive(true);
        }

    }

    void Update()
    {
        if (turret)
        {
            RechargeSync();
            HealthSync();
            statsPanel.SetActive(Input.GetKey(KeyCode.Tab));
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            chooseTeam.SetActive(!chooseTeam.activeInHierarchy);
        }
    }

    void RechargeSync()
    {
        rechargeBar.localScale = new Vector3(turret.GetRecharge(), 1, 1);
    }

    void HealthSync()
    {
        healthBar.localScale = new Vector3(player.GetHpToBar, 1, 1);
    }

    public void InstantiatePlayerStat()
    {
        Player[] _players = GameManager.GetPlayersArray();
        if (playerPanels.Count > 0 && playerPanels != null)
        {
            foreach (string PlayerID in playerPanels.Keys)
            {
                Destroy(playerPanels[PlayerID].gameObject);
            }
            playerPanels.Clear();
        }

        for (int i = 0; i < _players.Length; i++)
        {
            print("Create panel Stats");
            if (_players[i].teamId == 0)
            playerPanels.Add(_players[i].name, Instantiate(prefabPlayerPanel, targetInstantiatePlayerPanelRed).GetComponent<PlayerStatPanel>());
            else if (_players[i].teamId == 1)
                playerPanels.Add(_players[i].name, Instantiate(prefabPlayerPanel, targetInstantiatePlayerPanelBlue).GetComponent<PlayerStatPanel>());
            else
                playerPanels.Add(_players[i].name, Instantiate(prefabPlayerPanel, targetInstantiatePlayerPanelSpect).GetComponent<PlayerStatPanel>());
            playerPanels[_players[i].name]._player = _players[i];
        }
    }

    public void Leave()
    {
        player.GetComponent<PlayerSetup>().Disconnected();
        playerPanels.Clear();
    }

    public void Notification(string notif)
    {
        Transform panel = Instantiate(notificationPanelPrefab, targetInstantiateNotificationPanel).transform;
        panel.GetComponentInChildren<Text>().text = notif;
        Destroy(panel.gameObject, 10);
    }
}
