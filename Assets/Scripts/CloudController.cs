using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudController : MonoBehaviour {

    ParticleSystem cloudSystem;
    public Color colour;
    public Color lining;
    bool painted = false;
    public int numberOfParticles;
    public float minSpeed;
    public float maxSpeed;
    public float distance;
    Vector3 startPosition;
    float speed;


    // Use this for initialization
	void Start () {
        cloudSystem = this.GetComponent<ParticleSystem>();
        Spawn();
	}

    void Spawn()
    {
        //extend the range of the scale on either side of the manager centre
        float xpos = UnityEngine.Random.Range(-0.5f, 0.5f);
        float ypos = UnityEngine.Random.Range(-0.5f, 0.5f);
        float zpos = UnityEngine.Random.Range(-0.5f, 0.5f);
        this.transform.localPosition = new Vector3(xpos, ypos, zpos);
        speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
        startPosition = this.transform.position;
    }

    void Paint()
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[cloudSystem.particleCount];
        cloudSystem.GetParticles(particles);
        if (particles.Length > 0)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].startColor = Color.Lerp(lining, colour, particles[i].position.y / 
                                                     cloudSystem.shape.scale.y);
            }
            painted = true;
            cloudSystem.SetParticles(particles, particles.Length);
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (!painted)
        {
            Paint();
        }

        this.transform.Translate(0, 0, speed);

        if (Vector3.Distance(this.transform.position, startPosition) > distance)
        {
            Spawn();
        }
	}
}
