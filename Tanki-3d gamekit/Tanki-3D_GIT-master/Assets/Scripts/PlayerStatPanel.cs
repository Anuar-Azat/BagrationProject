using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatPanel : MonoBehaviour {

    [HideInInspector]
    public Player _player;

    [SerializeField]
    private Text _name;

    [SerializeField]
    private Text killing;

    [SerializeField]
    private Text dead;

    [SerializeField]
    private Text yp;
    [SerializeField]
    private Color redTeam;
    [SerializeField]
    private Color blueTeam;
    [SerializeField]
    private Color spectator;
    private Image image;

    private void Start()
    {
        image = GetComponent<Image>();
        if (_player.teamId == 0) image.color = redTeam;
        else if (_player.teamId == 1) image.color = blueTeam;
        else image.color = spectator;
    }

    private void Update()
    {
        _name.text = _player.nickname;
        killing.text = _player.killing.ToString();
        dead.text = _player.dead.ToString();
    }

}
