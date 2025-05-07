using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public class EnemyHealth : MonoBehaviour
{
    /// <summary>
    /// 
    /// ParryKatana.csの内容からHPゲージの機能を分離
    /// このスクリプトに移植する
    /// 
    /// </summary>

    [SerializeField] private Transform _waistBone;
    [SerializeField] private string _targetTag = "Bullet"; // ヒット対象タグ

    [Header("仰け反り設定")]
    [SerializeField] private float _tiltPower = 10f;       // 仰け反りの強さ（度数）
    [SerializeField] private float _tiltInDuration = 0.1f; // 仰け反りに入るまでの時間
    [SerializeField] private float _tiltOutDuration = 0.2f; // 戻るまでの時間

    private Quaternion _originRotation;
    private Vector3 _currentTiltOffset;
    private CancellationTokenSource _CancelTokenSource;
    private bool _isTilting = false;


    private void Start()
    {
        _originRotation = _waistBone.localRotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(_targetTag)) return;
        if (_isTilting) return;

        var hitColliderAngles = other.transform.eulerAngles;
        hitColliderAngles.x = 0f;
        HitTiltWaistAsync(Quaternion.Euler(hitColliderAngles) * Vector3.forward).Forget();
    }

    private async UniTaskVoid HitTiltWaistAsync(Vector3 vec)
    {
        _CancelTokenSource?.Cancel();
        _CancelTokenSource = new CancellationTokenSource();
        _isTilting = true;

        vec = transform.InverseTransformVector(vec);
        var tiltAngles = new Vector3(0f, vec.x, -vec.z).normalized * _tiltPower;

        try
        {
            await AnimateAngles(Vector3.zero, tiltAngles, _tiltInDuration, _CancelTokenSource.Token);
            await AnimateAngles(tiltAngles, Vector3.zero, _tiltOutDuration, _CancelTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // キャンセル時は無視
        }
        finally
        {
            _currentTiltOffset = Vector3.zero;
            _isTilting = false;
        }
    }

    private async UniTask AnimateAngles(Vector3 from, Vector3 to, float duration, CancellationToken token)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _currentTiltOffset = Vector3.Lerp(from, to, t);
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        _currentTiltOffset = to;
    }

    private void LateUpdate()
    {
        _waistBone.localRotation = _originRotation * Quaternion.Euler(_currentTiltOffset);
    }
}
