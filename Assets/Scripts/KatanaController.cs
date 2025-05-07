using UnityEngine;
//using UnityEngine.InputSystem;
using System.Collections;

public class KatanaController : MonoBehaviour
{
    [SerializeField] private Transform katanaHandle;
    [SerializeField] private Transform katanaOrigin;
    [SerializeField] private float moveSmoothSpeed = 5f;
    [SerializeField] private float rotateSmoothSpeed = 5f;
    [SerializeField] private float targetZOffset = 0.5f;
    [SerializeField] private float screenCenterYOffset = -50f; // 画面中央のY軸オフセット
    [SerializeField] private KeyCode activationKey = KeyCode.Mouse0;
    [SerializeField] private Camera mainCamera;
    private float currentTargetZAngle;

    [Header("通常攻撃設定")]
    [SerializeField] private float slashTiltAngle = 145f;
    [SerializeField] private float slashDuration = 0.3f;
    [SerializeField] private AnimationCurve slashCurve;

    private bool isSlashing = false;
    private float slashTimer = 0f;
    private Quaternion slashStartRot;
    private Quaternion slashEndRot;

    [Header("必殺技")]
    /// <summary>
    ///   必殺モードへの移行条件
    ///   - パリィ成功回数が1回以上
    ///   - 刀を左右どちらかに1回転させゲージを溜める
    /// 
    ///   必殺モード中における技の発動条件
    ///   必殺技1「穿突」の発動条件
    ///   - 必殺モード中、前方向の移動入力をした状態でクリックし、離す
    ///   必殺技2「」の発動条件
    ///   - 
    /// </summary>
    private bool isSpacialMode = false;
    private int parryCount = 0;


    private void FixedUpdate()
    {
        if (Input.GetKey(activationKey))
        {
            Vector3 targetPosition = CalculateTargetPosition();
            MoveKatana(targetPosition);
            UpdateTargetAngle(targetPosition);
            RotateKatanaSmooth();
        }
        else if (Input.GetKeyUp(activationKey))
        {
            // 位置が十分近い場合のみスラッシュを開始
            float distance = Vector3.Distance(katanaHandle.position, CalculateTargetPosition());
            if (!isSlashing && distance < 0.1f) // 0.1f は許容誤差
            {
                StartSlash();
            }
        }
        else
        {
            if (!isSlashing)
            {
                katanaHandle.position = Vector3.Lerp(katanaHandle.position, katanaOrigin.position, moveSmoothSpeed * Time.deltaTime);
                katanaHandle.rotation = Quaternion.Lerp(katanaHandle.rotation, katanaOrigin.rotation, moveSmoothSpeed * Time.deltaTime);
            }
        }

        if (isSlashing)
        {
            UpdateSlash();
        }
    }


    private Vector3 CalculateTargetPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        return ray.origin + ray.direction * targetZOffset;
    }

    private void MoveKatana(Vector3 targetPosition)
    {
        katanaHandle.position = Vector3.Slerp(katanaHandle.position, targetPosition, rotateSmoothSpeed * Time.deltaTime);
    }

    private void UpdateTargetAngle(Vector3 targetPosition)
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f + screenCenterYOffset, targetZOffset);
        Vector3 worldCenter = mainCamera.ScreenToWorldPoint(screenCenter);

        Vector3 directionToCenterLocal = mainCamera.transform.InverseTransformDirection(worldCenter - targetPosition);
        directionToCenterLocal.z = 0;

        float zAngle = Mathf.Atan2(directionToCenterLocal.y, directionToCenterLocal.x) * Mathf.Rad2Deg - 90f;

        currentTargetZAngle = zAngle;
    }

    private void RotateKatanaSmooth()
    {
        Quaternion yRotation = Quaternion.LookRotation(mainCamera.transform.forward, Vector3.up);
        Quaternion targetRotation = yRotation * Quaternion.Euler(0, 0, currentTargetZAngle);

        // 今の回転を目標回転に滑らかに近づける
        katanaHandle.rotation = Quaternion.Lerp(katanaHandle.rotation, targetRotation, rotateSmoothSpeed * Time.deltaTime);
    }

    private void StartSlash()
    {
        if (isSlashing) return; // 二重発動防止

        isSlashing = true;
        slashTimer = 0f;

        slashStartRot = katanaHandle.rotation;
        slashEndRot = katanaHandle.rotation * Quaternion.Euler(slashTiltAngle, 0f, 0f);
    }

    private void UpdateSlash()
    {
        slashTimer += Time.deltaTime;
        float t = slashTimer / slashDuration;

        if (t >= 1f)
        {
            t = 1f;
            isSlashing = false;
        }

        float curvedT = slashCurve.Evaluate(t);
        katanaHandle.rotation = Quaternion.Slerp(slashStartRot, slashEndRot, curvedT);
    }

    private void EnterSpecialMode()
    {
        if (parryCount >= 1 || isSpacialMode == true)
        {
            // 制限時間タイマーをスタートさせる
            // 制限時間が来たら、通常攻撃に戻す
        }
    }
}
