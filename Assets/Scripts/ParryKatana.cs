using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ParryKatana : MonoBehaviour
{
    /// <summary>
    /// 
    /// HPゲージの機能が入ってややこしくなっているので後々修正
    /// 別途新しいスクリプトを作成して対応
    /// 
    /// </summary>

    // 刀の操作に関するもの
    [SerializeField] private Transform m_motiteAngle;
    [SerializeField] private GameObject m_attackEffect;
    [SerializeField] private string m_targetTag;
    [SerializeField] private string m_enemyTag;
    [SerializeField] private float angleThreshold = 10f; // 90度からの許容誤差
    [SerializeField] private float effectLifetime = 1.2f;


    // 攻撃力を決定するもの
    [Header("攻撃力関連")]
    public int Damage;
    public int numCurrentAttack; // 基礎攻撃力
    public int numCurrentParry;  // パリィ成功回数
    [SerializeField] TextMeshProUGUI m_parryCounter; // パリィカウンタUI

    // サウンド系
    [Header("サウンド関連")]
    [SerializeField] private AudioSource m_audioSource;
    [SerializeField] private AudioClip m_parrySE;
    [SerializeField] private AudioClip m_damageSE;

    [Header("エネミーHPバー")]
    [SerializeField] private Slider m_enemyHpbar;
    private float maxHp = 100f;
    private float currentHp;


    private void Start()
    {
        m_attackEffect.SetActive(false);
        m_enemyHpbar.value = maxHp; //Sliderを満タンにする。
        currentHp = maxHp; //現在のHPを最大HPと同じに。
        numCurrentParry = 0; //パリィカウンタの回数をリセット
    }

    private void FixedUpdate()
    {
        string numP = numCurrentParry.ToString();
        m_parryCounter.text = numP;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(m_targetTag))
        {
            print("敵の斬撃に当たった");

            Vector3 motiteRight = m_motiteAngle.right;
            Vector3 otherRight = other.transform.right;

            motiteRight = new Vector3(motiteRight.x, motiteRight.y, 0).normalized;
            otherRight = new Vector3(otherRight.x, otherRight.y, 0).normalized;

            float dotProduct = Vector3.Dot(motiteRight, otherRight);
            float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;
            float differenceFrom90 = Mathf.Abs(90f - angle);

            string parryRank = GetParryRank(differenceFrom90);
            Debug.Log($"Parry Rank: {parryRank} (Angle diff: {differenceFrom90})");

            switch (parryRank)
            {
                case "SS":
                case "S":
                case "A":
                    {
                        Vector3 hitPosition = other.ClosestPoint(transform.position);
                        m_attackEffect.transform.position = hitPosition;
                        m_attackEffect.SetActive(true);

                        numCurrentParry += 1;
                        m_audioSource.PlayOneShot(m_parrySE);
                        other.gameObject.SetActive(false);

                        StartCoroutine(HideEffectAfterDelay(effectLifetime));
                        break;
                    }
                case "X":
                    {
                        ApplyKnockback(); // 角度が悪いパリィにノックバック処理
                        break;
                    }
            }
        }

        if (other.CompareTag(m_enemyTag))
        {
            currentHp -= (numCurrentAttack * numCurrentParry);
            m_enemyHpbar.value = currentHp;

            numCurrentParry = 0;
            m_audioSource.PlayOneShot(m_damageSE);
        }
    }

    private string GetParryRank(float difference)
    {
        if (difference <= 5f) return "SS";      // 神パリィ
        if (difference <= 10f) return "S";
        if (difference <= 30f) return "A";
        return "X"; // パリィ失敗に近い → ノックバック
    }

    private void ApplyKnockback()
    {
        Debug.Log("ノックバック！角度が悪かった");

        // ここにノックバック処理を書く
        // 例：Rigidbody に力を加える、位置を後ろに下げる、硬直する、等
        // GetComponent<Rigidbody>().AddForce(-transform.forward * 10f, ForceMode.Impulse);
    }

    private IEnumerator HideEffectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        m_attackEffect.SetActive(false);
    }
}
