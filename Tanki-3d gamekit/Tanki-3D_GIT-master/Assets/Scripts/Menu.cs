using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public class Menu : MonoBehaviour {
    [Header("Никнейм")]
    public InputField inputName;
    [Header("Присоединиться")]
    public InputField _IpAdress;
    public InputField _PortJoin;
    [Header("Создать")]
    public InputField _PortHost;

    private NetworkManager _networkManager;
    private const string unnamed = "unnamed";

    void Start()
    {
        _networkManager = NetworkManager.singleton;
        _PortHost.text = _networkManager.networkPort.ToString();
        _IpAdress.text = _networkManager.networkAddress;
        _PortJoin.text = _networkManager.networkPort.ToString();

        if (PlayerPrefs.GetString("Name") != "")
            inputName.text = PlayerPrefs.GetString("Name");
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void HostGame()
    {
        _networkManager.networkPort = int.Parse(_PortHost.text);
        _networkManager.StartHost();
        Unnamed();
    }

    public void JoinGame()
    {
        _networkManager.networkAddress = _IpAdress.text;
        _networkManager.networkPort = int.Parse(_PortJoin.text);
        _networkManager.StartClient();
        Unnamed();
    }
    void Unnamed()
    {
        if (PlayerPrefs.GetString("Name") == "")
            PlayerPrefs.SetString("Name", unnamed);
    }
    public void ApplyName()
    {
        PlayerPrefs.SetString("Name", inputName.text);
    }

}
