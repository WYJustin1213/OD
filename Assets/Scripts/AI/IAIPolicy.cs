public interface IAIPolicy
{
    AIIntent Decide(in AIContext ctx);
}
