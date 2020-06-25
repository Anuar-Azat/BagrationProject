using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public MatchSettings matchSettings;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    #region Player
    private const string PLAYER_ID_PREFIX = "Player ";

    private static Dictionary<string, Player> players = new Dictionary<string, Player>();
    private static Dictionary<string, Collider[]> playersColliders = new Dictionary<string, Collider[]>();

    public static void RegisterPlayer(string _netID, Player _player, Collider[] _colliders)
    {
        string _playerID = PLAYER_ID_PREFIX + _netID;
        players.Add(_playerID, _player);
        _player.transform.name = _playerID;
        playersColliders.Add(_playerID, _colliders);
        foreach (string I_playerID in playersColliders.Keys)
        {
            foreach (Collider col in playersColliders[I_playerID]) // Все объекты с коллайдером должны иметь имя игрока которому они принадлежат
            {
                col.transform.name = I_playerID;
            }
        }
    }
    public static void UnRegisterPlayer(string _playerID)
    {
        players.Remove(_playerID);
        playersColliders.Remove(_playerID);
    }

    public static Player GetPlayer(string _playerID)
    {
        return players[_playerID];
    }

    public static int countPlayers()
    {
        return players.Count;
    }

    public static Collider[] GetPlayersColliders(string _ignorPlayerID)
    {
        List<Collider> cols = new List<Collider>(); // задаем размер массива по первому игроку (как пример)
        if (playersColliders.Count > 1)
        {
            foreach (string _playerID in playersColliders.Keys)
            {
                if (_playerID != _ignorPlayerID) //Игнорируем игрока который выполняет запрос
                {
                    foreach (Collider col in playersColliders[_playerID])
                    {
                        cols.Add(col);
                    }
                }
            }
        }
        return cols.ToArray();
    }

    public static Player[] GetPlayersArray()
    {
        List<Player> _players = new List<Player>();
        foreach (string playerID in players.Keys)
        {
            _players.Add(players[playerID]);
        }
        return _players.ToArray();
    }

    #endregion
}
