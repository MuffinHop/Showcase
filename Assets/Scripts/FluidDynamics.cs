using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using RocketNet;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Unity.Burst;



[RequireComponent(typeof(ParticleSystem))]
public class FluidDynamics : MonoBehaviour
{
    [SerializeField,Range(0.01f,100f)] private float particleMass = 1f;
    [SerializeField,Range(0.01f,10f)] private float particleStiffness = 1f;
    [SerializeField,Range(0.01f,1f)] private float particleDamping = 0.2f;
    [SerializeField,Range(0.001f,113f)] private float particleRestDensity = 0.1f;
    [SerializeField,Range(0.01f,1f)] private float particleViscosity = 0.1f;
    [SerializeField] private DeviceController _deviceController;
    private ParticleSystem.Particle[] particles;
    private ParticleSystem particleSystem;
    private Track _fluidDynamics;

    private void Awake()
    {
        _fluidDynamics = _deviceController.Device.GetTrack("FluidDynamics");
        particleSystem = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
        StartCoroutine(Execute());
    }

    [BurstCompile]
    private IEnumerator Execute()
    {
        int threadsCount = Process.GetCurrentProcess().Threads.Count;
        while (true)
        {
            if (_deviceController.GetValue(_fluidDynamics) == 0f)
            {
                yield return null;
            }
            var watch = new Stopwatch();
            var mainCamera = Camera.main;
            watch.Start();
            float deltaTime = Time.deltaTime;
            int numParticles = particleSystem.GetParticles(particles);
            var particleRadius = particles[0].startSize;
            int onScreen = 0;
            for (int i = Time.frameCount%4; i < numParticles; i+=4)
            {
                if (particles[i].remainingLifetime <= 0.0f ||
                    float.IsNaN(particles[i].position.x) ||
                    float.IsNaN(particles[i].position.y) ||
                    float.IsNaN(particles[i].position.z))
                {
                    continue;
                }
                var pos = mainCamera.WorldToScreenPoint(particles[i].position);
 
                var outOfBounds = !Screen.safeArea.Contains(pos);
                
                if (outOfBounds)
                {
                    continue; //not on screen, continue;
                } else {
                    onScreen++;
                }
                
                Vector3 position = particles[i].position;
                Vector3 velocity = particles[i].velocity;

                // Calculate the density of the particle based on its neighbors
                float density = particleRestDensity * particleRadius;
                int step = numParticles / threadsCount;
                var tasks = new List<Task>();
                Parallel.For(0, threadsCount, threadId =>
                {
                    for (int j = threadId * step; j < (threadId + 1) * step; j++)
                    {
                        if (i == j)
                            continue;
                        float distance = Vector3.Distance(position, particles[j].position);
                        float radius = particleRadius * 2f;

                        if (distance < radius)
                        {
                            float q = 1f - distance / radius;
                            density += particleMass * q * q * q;
                        }
                    }
                });

                // Calculate the pressure of the particle based on its density
                float pressure = particleStiffness * (density - particleRestDensity);

                // Calculate the force of the particle based on its neighbors
                Vector3 force = Vector3.zero;
                if(density>particleRestDensity * particleRadius)
                Parallel.For(0, threadsCount, threadId =>
                {
                    for (int j = threadId * step; j < (threadId + 1) * step; j++)
                    {
                        if (i == j)
                            continue;
                        Vector3 direction = position - particles[j].position;
                        float distance = direction.magnitude;
                        float radius = particleRadius * 2f;

                        if (distance < radius)
                        {
                            float q = 1f - distance / radius;
                            float pressureForce = -particleMass * (pressure + particleStiffness * (1f - q)) / density;
                            float viscosityForce = particleViscosity * particleMass *  (particles[j].velocity.magnitude - velocity.magnitude) / density;
                            force += (pressureForce + viscosityForce) * direction.normalized;
                        }
                    }
                });

                // Apply damping to the particle
                force -= particleDamping * velocity;
                density = Mathf.Max(density, 4f);
                // Update the velocity and position of the particle
                Vector3 prepVelocity = force / density * deltaTime;
                prepVelocity = prepVelocity.normalized * Mathf.Min(prepVelocity.magnitude, 5.0f);
                velocity +=prepVelocity;
                position += velocity * deltaTime;

                particles[i].position = position;
                particles[i].velocity = velocity;
            }

            particleSystem.SetParticles(particles, numParticles);
            watch.Stop();
            yield return new WaitForEndOfFrame();
        }
    }
}