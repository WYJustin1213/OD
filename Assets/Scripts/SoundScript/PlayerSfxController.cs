using UnityEngine;

public class PlayerSfxController : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private PlayerSfxProfile profile;

    private void Awake()
    {
        if (player == null) player = GetComponent<Player>();
    }

    // ---------- START SOUNDS (called by states) ----------

    public void PlayAttack1Start()
        => SfxManager.Instance?.Play2D(profile?.attack1Start);

    public void PlayAttack2Start()
        => SfxManager.Instance?.Play2D(profile?.attack2Start);

    public void PlayAttack3Start()
        => SfxManager.Instance?.Play2D(profile?.attack3Start);

    // ---------- HIT SOUNDS (called by animation events) ----------

    public void PlayAttack1Hit()
        => SfxManager.Instance?.Play2D(profile?.attack1Hit);

    public void PlayAttack2Hit()
        => SfxManager.Instance?.Play2D(profile?.attack2Hit);

    public void PlayAttack3Hit()
        => SfxManager.Instance?.Play2D(profile?.attack3Hit);

    // ---------- MOVE SOUNDS (called by animation events) ----------

    public void PlayerLand()
        => SfxManager.Instance?.Play2D(profile?.land);

    public void PlayerWalk()
        => SfxManager.Instance?.Play2D(profile?.walk);

    public void PlayerRun()
        => SfxManager.Instance?.Play2D(profile?.run);

    // ---------- HEALTH SOUNDS (called by animation events) ----------

    public void PlayerHit()
        => SfxManager.Instance?.Play2D(profile?.hit);

    public void PlayerDeath()
        => SfxManager.Instance?.Play2D(profile?.death);
}
