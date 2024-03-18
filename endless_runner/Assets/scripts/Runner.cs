using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Runner : MonoBehaviour
{
    [SerializeField]
    Light pointLight;

    [SerializeField]
    ParticleSystem explosionSystem, trailSystem;

    [SerializeField, Min(0f)]
    float startSpeedX = 5f, maxSpeedX = 40f, jumpAcceleration = 100f, gravity = 40f;

    [SerializeField, Min(0f)]
    float extents = 0.5f;

    [SerializeField]
    FloatRange jumpDuration = new FloatRange(0.1f, 0.2f);

    [SerializeField]
    AnimationCurve runAccelerationCurve;

    [SerializeField, Min(0f)]
    float spinDuration = 0.75f;

    float spinTimeRemaining;

    Vector3 spinRotation;

    MeshRenderer meshRenderer;

    Vector2 position, velocity;

    public Vector2 Position => position;
    public float SpeedX
    {
        get => velocity.x;
        set => velocity.x = value;
    }

    SkylineObject currentObstacle;
    bool grounded, transitioning;
    float jumpTimeRemaining;
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.enabled = false;
        pointLight.enabled = false;
    }

    public void StartNewGame(SkylineObject obstacle)
    {
        currentObstacle = obstacle;
        while (currentObstacle.MaxX < extents)
        {
            currentObstacle = currentObstacle.Next;
        }
        position =  new Vector2(0f, currentObstacle.GapY.min + extents);
        transform.SetPositionAndRotation(position, Quaternion.identity);
        meshRenderer.enabled = true;
        pointLight.enabled = true;
        explosionSystem.Clear();
        SetTrailEmission(true);
        trailSystem.Clear();
        trailSystem.Play();
        transitioning = false;

        grounded = true;
        jumpTimeRemaining = 0f;
        spinTimeRemaining = 0f;
        velocity = new Vector2(startSpeedX, 0f);
    }

    void Explode()
    {
        meshRenderer.enabled = false;
        pointLight.enabled = false;
        SetTrailEmission(false);
        transform.localPosition = position;
        explosionSystem.Emit(explosionSystem.main.maxParticles);
    }
    void SetTrailEmission(bool enabled)
    {
        ParticleSystem.EmissionModule emission = trailSystem.emission;
        emission.enabled = enabled;
    }

    public bool Run(float dt)
    {
        Move(dt);
        if (position.x + extents < currentObstacle.MaxX)
        {
            ConstrainY(currentObstacle);
        }
        else
        {
            bool isStillInsideCurrentObstacle = position.x - extents < currentObstacle.MaxX;
            if (isStillInsideCurrentObstacle)
            {
                ConstrainY(currentObstacle);
            }

            if (!transitioning)
            {
                if (CheckCollision())
                {
                    return false;
                }
                transitioning = true;
            }

            ConstrainY(currentObstacle.Next);
            if (!isStillInsideCurrentObstacle)
            {
                currentObstacle = currentObstacle.Next;
                transitioning = false;
            }
        }
        
        return true;
    }
    
    public void UpdateVisualization()
    {
        transform.localPosition = position;
        if (spinTimeRemaining > 0f)
        {
            spinTimeRemaining = Mathf.Max(spinTimeRemaining - Time.deltaTime, 0f);
            transform.localRotation = Quaternion.Euler(
                Vector3.Lerp(spinRotation, Vector3.zero, spinTimeRemaining / spinDuration)
            );
        }
    }

    void ConstrainY(SkylineObject obstacle)
    {
        FloatRange openY = obstacle.GapY;
        if (position.y - extents <= openY.min)
        {
            position.y = openY.min + extents;
            velocity.y = Mathf.Max(velocity.y, 0f);
            jumpTimeRemaining = 0f;
            grounded = true;
        }
        else if (position.y + extents >= openY.max)
        {
            position.y = openY.max - extents;
            velocity.y = Mathf.Min(velocity.y, 0f);
            jumpTimeRemaining = 0f;
        }
        obstacle.Check(this);
    }

    bool CheckCollision()
    {
        Vector2 transitionPoint;
        transitionPoint.x = currentObstacle.MaxX - extents;
        transitionPoint.y =
            position.y - velocity.y * (position.x - transitionPoint.x) / velocity.x;

        float shrunkExtents = extents - 0.01f;
        FloatRange gapY = currentObstacle.Next.GapY;
        if (
            transitionPoint.y - shrunkExtents < gapY.min ||
            transitionPoint.y + shrunkExtents > gapY.max
        )        
        {
            position = transitionPoint;
            Explode();
            return true;
        }

        return false;
    }

    void Move(float dt)
    {
        if (jumpTimeRemaining > 0f)
        {
            jumpTimeRemaining -= dt;
            velocity.y += jumpAcceleration * Mathf.Min(dt, jumpTimeRemaining);
        }
        else
        {
            velocity.y -= gravity * dt;
        }

        if (grounded)
        {
            velocity.x = Mathf.Min(
                velocity.x + runAccelerationCurve.Evaluate(velocity.x / maxSpeedX) * dt,
                maxSpeedX
            );
            grounded = false;
        }
        position += velocity * dt;
    }

    public void StartJumping()
    {
        if (grounded)
        {
            jumpTimeRemaining = jumpDuration.max;
            if (spinTimeRemaining <= 0f)
            {
                spinTimeRemaining = spinDuration;
                spinRotation = Vector3.zero;
                spinRotation[Random.Range(0, 3)] = Random.value < 0.5f ? -90f : 90f;
            }
        }
    }

    public void EndJumping() => jumpTimeRemaining += jumpDuration.min - jumpDuration.max;
}
