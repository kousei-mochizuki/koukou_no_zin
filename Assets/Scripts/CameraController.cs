using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform m_mainCamera;
    [SerializeField] private float m_sensX = 5f;
    // [SerializeField] private float m_sensY = 5f;
    private float _yRotation, _xRotation;

    // Update is called once per frame
    void Update()
    {
        Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X") * m_sensX, 0);

        _xRotation -= mouseInput.y;

        // 頭、体の向きの適用
        m_mainCamera.localRotation = Quaternion.Euler(_xRotation, 0, 0);
    }
}
