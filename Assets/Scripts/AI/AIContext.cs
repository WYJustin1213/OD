using UnityEngine;

public struct AIContext
{
    public Transform self;
    public Transform player;

    public Vector2 selfPos;
    public Vector2 playerPos;

    public float moveSpeed;

    // Perception info (precomputed by EnemyBrain or a Perception component)
    public Vector2 alliesCentroid;
    public int allyCount;

    public float time;
    public float deltaTime;
}
