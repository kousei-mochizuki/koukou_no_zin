using UnityEngine;
using UnityEngine.UI;

public class KatanaRotationTracker : MonoBehaviour
{
    [Header("UI")]
    public Image progressRing;
    public float finishDelay = 1.0f;

    [Header("Input")]
    public KeyCode activationKey = KeyCode.Mouse0;

    private float cumulativeRotation = 0f;
    private bool isTracking = false;
    public bool isFullyCharged = false;

    private Vector2 screenCenter;
    private Vector2 lastDirection;
    private bool isClockwise = true;

    private bool isCharging = false;
    public float currentProgress = 0f;

    private void Start()
    {
        screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        ResetGauge();
    }

    private void Update()
    {
        if (Input.GetKeyDown(activationKey))
        {
            StartTracking();
        }
        else if (Input.GetKeyUp(activationKey))
        {
            StopTracking();
        }

        if (isTracking && !isFullyCharged)
        {
            TrackMouseRotation();
        }
    }

private void StartTracking()
{
    isTracking = true;
    isFullyCharged = false;
    isCharging = true;
    cumulativeRotation = 0f;
    isClockwise = true;

    Vector2 mousePos = Input.mousePosition;
    lastDirection = (mousePos - screenCenter).normalized;

    // クリックした方向にUIを回転
    float angleToMouse = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
    progressRing.transform.eulerAngles = new Vector3(0, 0, angleToMouse - 90f); // UI回転

    // fillClockwiseの設定とfillOriginの初期化（見た目に合わせて）
    progressRing.fillClockwise = isClockwise;
    progressRing.fillOrigin = 0; // 上（0）を基準に回す。UI回転で見た目は対応できる
}


    private void StopTracking()
    {
        isTracking = false;
        CancelTimer();
    }

    private void TrackMouseRotation()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 currentDirection = (mousePos - screenCenter).normalized;

        float angle = Vector2.SignedAngle(lastDirection, currentDirection);
        bool currentIsClockwise = angle < 0;

        if (currentIsClockwise == isClockwise)
        {
            cumulativeRotation += Mathf.Abs(angle);
        }
        else
        {
            cumulativeRotation -= Mathf.Abs(angle);

            if (cumulativeRotation <= 0f)
            {
                cumulativeRotation = 0f;
                isClockwise = !isClockwise; // 反転
                progressRing.fillClockwise = isClockwise; // UIも反転
            }
        }

        lastDirection = currentDirection;

        if (cumulativeRotation >= 360f)
        {
            CompleteCharge();
        }
        else
        {
            float progress = cumulativeRotation / 360f;
            UpdateProgress(progress);
        }
    }

    private void UpdateProgress(float progress)
    {
        if (isCharging && !isFullyCharged)
        {
            currentProgress = progress;
            progressRing.fillAmount = currentProgress;
        }
    }

    private void CompleteCharge()
    {
        isCharging = false;
        isFullyCharged = true;
        currentProgress = 1f;
        progressRing.fillAmount = 1f;
    }

    private void CancelTimer()
    {
        isCharging = false;
        ResetGauge();
    }

    private void ResetGauge()
    {
        isCharging = false;
        isFullyCharged = false;
        progressRing.fillAmount = 0f;
        currentProgress = 0f;
        progressRing.fillClockwise = true;
    }

    public void ResetGaugeForced()
    {
        ResetGauge();
    }
}
