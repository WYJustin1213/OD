using UnityEngine;

public class PlayerAttackThree : PlayerState
{
    private PlayerSfxController sfx;

    public PlayerAttackThree(Player player) : base(player) 
    {
        sfx = player.GetComponent<PlayerSfxController>();
    }

    public override void Enter()
    {
        base.Enter();

        animator.SetBool("isAttacking3", true);
        combat.attackType = PlayerAttackType.AttackThree;
        combat.damage = AttackThreeDamage;

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        sfx?.PlayAttack3Start();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void Update()
    {
        base.Update();

        if (HP <= 0)
        {
            player.ChangeState(player.deathState);
        }
    }

    public override void AttackAnimationFished()
    {
        
        player.ChangeState(player.crouchState);
    }

    public override void Exit()
    {
        base.Exit();

        animator.SetBool("isAttacking3", false);
    }
}
