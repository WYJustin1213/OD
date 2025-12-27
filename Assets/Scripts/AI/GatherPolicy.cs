using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class GatherPolicy : IAIPolicy
{
    // Tuning knobs
    private readonly float _towardAlliesWeight;
    private readonly float _towardPlayerWeight;

    public GatherPolicy(float towardAlliesWeight = 1.0f, float towardPlayerWeight = 0.6f)
    {
        _towardAlliesWeight = Mathf.Max(0f, towardAlliesWeight);
        _towardPlayerWeight = Mathf.Max(0f, towardPlayerWeight);
    }

    public AIIntent Decide(in AIContext ctx)
    {
        Vector2 toAllies = (ctx.alliesCentroid - ctx.selfPos);
        Vector2 toPlayer = (ctx.playerPos - ctx.selfPos);

        Vector2 dir = Vector2.zero;

        if (toAllies.sqrMagnitude > 0.0001f)
        {
            dir += toAllies.normalized * _towardAlliesWeight;
        }

        if (toPlayer.sqrMagnitude > 0.0001f)
        {
            dir += toPlayer.normalized * _towardPlayerWeight;
        }

        if (dir.sqrMagnitude > 0.0001f)
        {
            dir = dir.normalized;
        }

        return new AIIntent
        {
            desiredVelocity = dir * ctx.moveSpeed,
            wantsAttack = false // set true when within attack range in a later pass
        };
    }
}
