using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class FlockAgent : MonoBehaviour
{

    BoidFlock _agentFlock;
    public float orbitBias;
    public BoidFlock AgentFlock { get { return _agentFlock; } }

    Collider2D _agentCollider;
    public Collider2D AgentCollider { get { return _agentCollider; } }
    
    public bool avoidingPlayer = false;
    private float _speedBoostTimer = 0f;

    public Vector2 previousSpeed = Vector2.zero;
    Vector2 _currentVelocity;
    
    public void TriggerSpeedBoost(float duration)
    {
        avoidingPlayer = true;
        _speedBoostTimer = duration;
    }

    // Call this once per frame from boidflock.cs
    public void UpdateBoostTimer(float deltaTime)
    {
        if (_speedBoostTimer > 0f)
        {
            _speedBoostTimer -= deltaTime;
            if (_speedBoostTimer <= 0f)
            {
                avoidingPlayer = false;
            }
        }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        _agentCollider = GetComponent<Collider2D>();
    }

    public void Initialize(BoidFlock flock)
    {
        _agentFlock = flock;
    }

    public void Move(Vector2 velocity)
    {
        velocity = Vector2.SmoothDamp(previousSpeed, velocity, ref _currentVelocity, 0.5f);

        // Rotate toward movement direction (only if there's a strong enough direction)
        if (velocity.sqrMagnitude > 0.0001f)
            transform.up = velocity.normalized;

        // Move using the smoothed velocity
        transform.position += (Vector3)(velocity * Time.deltaTime);

        previousSpeed = velocity;
        
    }

    private void OnDrawGizmosSelected()
    {
        //throw new NotImplementedException();
    }
}
