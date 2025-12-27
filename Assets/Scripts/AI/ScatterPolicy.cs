using UnityEngine;

public class ScatterPolicy : IAIPolicy
{
    private readonly float _awayFromAlliesWeight;
    private readonly float _towardPlayerWeight;

    public ScatterPolicy(float awayFromAlliesWeight = 1.2f, float towardPlayerWeight = 0.4f)
    {
        _awayFromAlliesWeight = Mathf.Max(0f, awayFromAlliesWeight);
        _towardPlayerWeight = Mathf.Max(0f, towardPlayerWeight);
    }

    public AIIntent Decide(in AIContext ctx)
    {
        Vector2 awayFromAllies = (ctx.selfPos - ctx.alliesCentroid);
        Vector2 toPlayer = (ctx.playerPos - ctx.selfPos);

        Vector2 dir = Vector2.zero;

        // Only repel if there are allies nearby; otherwise just engage player a bit.
        if (ctx.allyCount > 0 && awayFromAllies.sqrMagnitude > 0.0001f)
        {
            dir += awayFromAllies.normalized * _awayFromAlliesWeight;
        }

        if (toPlayer.sqrMagnitude > 0.0001f)
        {
            // Slightly approach player so enemies don't flee forever.
            dir += toPlayer.normalized * _towardPlayerWeight;
        }

        if (dir.sqrMagnitude > 0.0001f)
        {
            dir = dir.normalized;
        }

        return new AIIntent
        {
            desiredVelocity = dir * ctx.moveSpeed,
            wantsAttack = false
        };
    }
}
