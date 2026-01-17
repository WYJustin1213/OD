using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerDeath : PlayerState
{
    public PlayerDeath(Player player) : base(player) { }


    public override void Enter()
    {
        base.Enter();

        animator.SetTrigger("isDead");

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        player.RestartText.SetActive(true);
        player.isDead = true;

        //player.playerCollider.enabled = false;
    }

    public override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
