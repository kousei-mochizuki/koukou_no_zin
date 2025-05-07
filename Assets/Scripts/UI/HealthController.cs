using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    // 体力ゲージ（表面の常に見える部分）
    [SerializeField] private GameObject _gauge;
    // 猶予ゲージ（体力が減ったとき一瞬見える部分）
    [SerializeField] private GameObject _graceGauge;

    // 最大HP
    [SerializeField] private int _HP;
    // HP1あたりの幅
    private float _HPwidth;
    // 体力ゲージが減った後裏ゲージが減るまでの待機時間
    private float _waitingTime = 0.5f;


    void Awake(){
        // スプライトの幅を最大HPで割ってHP1あたりの幅を”_HP1”に入れておく
        _HPwidth = _gauge.GetComponent<RectTransform>().sizeDelta.x / _HP;
    }

    // 攻撃力をそれぞれのボタンで設定
    public void BeInjured(int atacck){
        // 攻撃力と体力1あたりの幅の積が実際に体力ゲージから減らす幅
        float damege = _HPwidth * atacck;

        // 減らす幅を設定してコルーチン”damegeEm”を呼び出し
        StartCoroutine(damegeEm(damege));
    }
    
    // 体力ゲージを減らすコルーチン
    IEnumerator damegeEm(float damege){
        // 体力ゲージの幅と高さをVector2で取り出す(Width,Height)
        Vector2 nowsafes = _gauge.GetComponent<RectTransform>().sizeDelta;
        // 体力ゲージの幅からダメージ分の幅を引く
        nowsafes.x -= damege;
        // 体力ゲージに計算済みのVector2を設定する
        _gauge.GetComponent<RectTransform>().sizeDelta = nowsafes;

        // ”_waitingTime”秒待つ
        yield return new WaitForSeconds(_waitingTime);
        // 猶予ゲージに計算済みのVector2を設定する
        _graceGauge.GetComponent<RectTransform>().sizeDelta = nowsafes;
    }
}