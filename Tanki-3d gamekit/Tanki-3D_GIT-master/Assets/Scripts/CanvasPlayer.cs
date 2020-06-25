using UnityEngine;
using UnityEngine.UI;
public class CanvasPlayer : MonoBehaviour
{

    public GameObject health;
    public GameObject recharger;

    [Header("Text никнейма")]
    public Text nicknameText;

    [Header("Коррекция баров")]
    public float y_ForRemote;
    public float size = 30;
    public Transform turretPlayer;
    public float visibleDistance = 100;
    [Header("Цвета баров")]
    public Color redTeam;
    public Color blueTeam;

    private RectTransform healthBar;
    private RectTransform rechargeBar;
    private Player _player;
    private Turret turret;
    private Transform turretTransform;
    //private RectTransform panel;
    private Canvas canvas;
    [HideInInspector]
    public bool _isLocalPlayer;
    private RectTransform canvasRectTr;
    private bool visible = false;
    private Animator animator;
    private LayerMask layer; //Все слои кроме игрока (для проверки видимости)
    private Image imageHealth;
    private void Start()
    {
        //panel = transform.GetChild(0).GetComponent<RectTransform>();
        rechargeBar = recharger.transform.GetChild(0).GetComponent<RectTransform>();
        healthBar = health.transform.GetChild(0).GetComponentInChildren<RectTransform>();
        canvas = GetComponent<Canvas>();
        canvasRectTr = canvas.GetComponent<RectTransform>();
        _player = GetComponentInParent<Player>();
        turret = GetComponentInParent<Turret>();
        recharger.SetActive(false);
        animator = GetComponent<Animator>();
        turretTransform = turret.transform;
        layer = turret.layerMask;
        imageHealth = healthBar.GetComponent<Image>();
        //health.SetActive(false);
    }

    void Update()
    {
        nicknameText.text = _player.nickname;
        Camera _camera = Camera.main;
        if (_camera == null) return;
        canvas.worldCamera = _camera;
        if (_player.teamId == 0) imageHealth.color = redTeam;
        else imageHealth.color = blueTeam;

        float distance = Vector3.Distance(transform.position, _camera.transform.position);
        visible = distance < visibleDistance && !Physics.Linecast(_camera.transform.position, turretTransform.position, layer) && !_player.IsDeath;
        
        if (visible)
        {
            transform.LookAt(transform.position + _camera.transform.rotation * Vector3.forward, _camera.transform.rotation * Vector3.up);
            Vector3 pos = Vector3.zero;
            pos.y = y_ForRemote;
            canvas.transform.localPosition = pos;
            // Размер
            float _size = distance / size * 0.1f;
            canvasRectTr.localScale = new Vector2(_size, _size);
            //
            HealthSync(); //Синхронизация жизней
        }
        animator.SetBool("visible", visible);
    }

    void RechargeSync()
    {
        rechargeBar.localScale = new Vector3(turret.GetRecharge(), 1, 1);
    }

    void HealthSync()
    {
        healthBar.localScale = new Vector3(_player.GetHpToBar, 1, 1);
    }
}
