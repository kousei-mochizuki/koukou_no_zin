using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

using Cysharp.Threading.Tasks;
using System.Threading;
using Cinemachine;

/// <summary>
///   TODO: カメラ振動系の処理をPlayerDamageとして別スクリプトに分割する
/// </summary>

public class PlayerController : MonoBehaviour
{
    private float _gravity = 10f; // 重力
    [SerializeField] private float _speedMove; // 移動速度
    [SerializeField] private Transform _targetCamDirection; // プレイヤーカメラのTransform
    [SerializeField] private CharacterController _characterController;

    private string _targetTag = "Attack"; // プレイヤーのダメージ判定用タグ]
    private string _knockbackTag = "Knockback";
    private Vector2 inputMove;

    [SerializeField] private Slider _playerHpbar; // HPバー
    private float maxHp = 20f;
    private float damage = 1f;
    private float currentHp;

    // （未使用）無敵処理用ブール
    private bool isInvincible;

    /// <summary>
    ///   サウンド系プロパティ
    /// </summary>
    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private AudioClip m_playerDamageSE;

    [Header("カメラシェイク")]
    [SerializeField] private CinemachineImpulseSource _impulseSource;
    [SerializeField] private Vector3 _shakeDir; // シェイク方向
    [SerializeField] private float _shakeDeceleration = 0.2f; // シェイクの減速時間
    [SerializeField] private float _shakeMaxTime = 0.2f; // シェイクの持続時間

    [Header("ノックバック力"), SerializeField]
    private float _blinkForce = 10f;

    // 必殺突きの設定
    [SerializeField] private KatanaRotationTracker chargeGauge; // 刀360回転ゲージがマックスかどうかの判定用
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashCooldown = 1f;
    public bool canDash = true;


    // PlayerInputの取得
    public void OnMove(InputAction.CallbackContext context)
    {
        inputMove = context.ReadValue<Vector2>(); // 入力値を保持しておく
    }
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.canceled && canDash && chargeGauge.isFullyCharged) StartDash();
    }
    public void OnBlink(InputAction.CallbackContext context)
    {
        if (context.performed) blinkPlayer();
    }


    private async void StartDash()
    {
        if (!canDash) return;

        Vector3 dashDirection = _targetCamDirection.forward;
        Vector3 dashDestination = transform.position + dashDirection * dashDistance;

        // レイキャストを使用して障害物をチェック
        if (Physics.Raycast(transform.position, dashDirection, out RaycastHit hit, dashDistance))
        {
            dashDestination = hit.point - dashDirection * 0.5f; // 障害物の少し手前で止まる
        }

        _characterController.enabled = false;
        transform.position = dashDestination;
        _characterController.enabled = true;

        canDash = false;
        await UniTask.Delay((int)(dashCooldown * 1000));
        canDash = true;
    }

    public void knockbackPlayer()
    {
        Vector3 cameraForward = -_targetCamDirection.forward; // カメラの前方向を取得し、Y軸を無視して正規化
        cameraForward.y = 0;
        cameraForward.Normalize();

        Vector3 moveDirection = cameraForward * 200f; // 移動方向
        Vector3 targetPosition = transform.position + moveDirection; // 移動位置の補間
        Vector3 smoothPosition = Vector3.Lerp(transform.position, targetPosition, 0.1f);

        _characterController.Move(moveDirection * Time.deltaTime);
    }

    private void blinkPlayer()
    {
        // カメラの右方向と前方向を取得し、Y軸を無視して正規化
        Vector3 cameraRight = _targetCamDirection.right;
        Vector3 cameraForward = _targetCamDirection.forward;
        cameraRight.y = 0;
        cameraForward.y = 0;
        cameraRight.Normalize();
        cameraForward.Normalize();

        // 移動方向をカメラ右方向と前方向のベクトルを組み合わせて設定
        // 前方向への割合は調整可能
        Vector3 moveDirection = (cameraRight * inputMove.x) * _blinkForce;

        // 移動位置の補間
        Vector3 targetPosition = transform.position + moveDirection;
        Vector3 smoothPosition = Vector3.Slerp(transform.position, targetPosition, 0.1f);

        _characterController.Move(moveDirection * Time.deltaTime);
    }

    /// <summary>
    ///   カメラを揺らすメソッド
    ///   <param name="dir">揺らす方向</param>
    ///   <param name="dt">減速時間</param>
    ///   <param name="maxTime">最大時間</param>
    /// </summary>
    private void ShakeCamera(Vector3 dir, float dt, float maxTime)
    {
        _impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_AttackTime = maxTime;
        _impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_DecayTime = dt;
        _impulseSource.GenerateImpulse(dir);
    }



    private void Awake()
    {
        // カメラがnullの場合、メインカメラを取得
        // if (_targetCamera == null)
        //     _targetCamera = Camera.main;
        
        _playerHpbar.value = maxHp; //Sliderを満タンにする。
        currentHp = maxHp; //現在のHPを最大HPと同じに。
    }


    private void FixedUpdate()
    {
        // カメラの前方向を取得（Y軸は無視）
        Vector3 cameraForward = _targetCamDirection.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        // カメラの右方向を取得（Y軸は無視）
        Vector3 cameraRight = _targetCamDirection.right;
        cameraRight.y = 0;
        cameraRight.Normalize();

        Vector3 moveDirection = cameraForward * inputMove.y + cameraRight * inputMove.x; // 入力に基づいて移動方向を計算
        Vector3 movement = moveDirection * _speedMove * Time.fixedDeltaTime; // 移動速度を計算
        movement.y = movement.y - (_gravity * Time.fixedDeltaTime); // 重力を適用

        // キャラクターを移動
        _characterController.Move(movement);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(_targetTag))
        {
            currentHp = currentHp - damage;
            _playerHpbar.value = currentHp;
            ShakeCamera(_shakeDir, _shakeDeceleration, _shakeMaxTime);
            m_audioSource.PlayOneShot(m_playerDamageSE);
            other.gameObject.SetActive(false);
        }

        if (other.CompareTag(_knockbackTag))
        {
            knockbackPlayer();
            other.gameObject.SetActive(false);
        }
    }
}
