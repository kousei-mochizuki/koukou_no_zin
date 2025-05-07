using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource m_impulseSource;

    public void CameraShaker(Vector3 dire, float decelerationTime, float maxTime)
    {
        m_impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_AttackTime = maxTime;
        m_impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_DecayTime = decelerationTime;
        m_impulseSource.GenerateImpulse(dire);
    }
}