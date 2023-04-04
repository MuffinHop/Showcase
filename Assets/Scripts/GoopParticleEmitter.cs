using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GoopParticleEmitter : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private ParticleSystem.Particle[] m_particles;
    private Vector4[] _list;
    private float[] _size;
    void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void LateUpdate()
    {
        if (m_particles == null)
        {
            m_particles = new ParticleSystem.Particle[_particleSystem.main.maxParticles];
            _list = new Vector4[_particleSystem.main.maxParticles];
            _size = new float[_particleSystem.main.maxParticles];
        }
        int numParticlesAlive = _particleSystem.GetParticles(m_particles);
        
        for (int i = 0; i < numParticlesAlive; i++)
        {
            
            _list[i] = m_particles[i].position + transform.position;
            _size[i] = m_particles[i].GetCurrentSize(_particleSystem) * (m_particles[i].remainingLifetime < 0.1f ? 0f : 1f);
        }
        Shader.SetGlobalFloatArray( "DropletSizeArray", _size);
        Shader.SetGlobalVectorArray( "DropletPositionArray", _list);
    }
}

