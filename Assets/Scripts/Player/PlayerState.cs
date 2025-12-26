using UnityEngine;

public abstract class PlayerState
{
    protected Player player;
    protected Animator animator;
    protected Rigidbody2D rb;

    protected bool JumpPressed { get => player.jumpPressed;set => player.jumpPressed = value; }
    protected bool SprintPressed => player.sprintPressed;
    protected bool AttackOnePressed => player.attackOnePressed;
    protected Vector2 MoveInput => player.moveInput;

    public PlayerState (Player player)
    {
        this.player = player;
        this.animator = player.animator;
        this.rb = player.rb;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }

    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void AttackAnimationFished() { }
}
