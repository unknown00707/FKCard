public class HealerCard0Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.GiveHealToTemproy(skillManager.toUserID, currentTurn, currentTurn + 1, 0.15f);
    }
}
public class HealerCard1Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        for (int i = 0; i < GameManager.Instance.playerTotalNum.Value; i++)
        {
            skillManager.GiveHealToTemproy((ulong)i, currentTurn, currentTurn + 1, 0.075f);
        }
    }
}
public class HealerCard3Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        for (int i = 0; i < GameManager.Instance.playerTotalNum.Value; i++)
        {
            skillManager.GiveHealToTemproy((ulong)i, currentTurn, currentTurn + 1, Random.Range(3f, 16f) / 100);
        }
    }
}
public class HealerCard4Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        for (int i = 0; i < GameManager.Instance.playerTotalNum.Value; i++)
        {
            skillManager.UpStateForIDUser((ulong)i, currentTurn, currentTurn + 1, 0.05f, 0, 0, 0, 0, skillManager.job, skillManager.cardIndex);
        }
    }
}
public class HealerCard6Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.GiveHealToTemproy(skillManager.toUserID, currentTurn, -1, 0.05f);
    }
}
public class HealerCard7Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.UpStateForIDUser(skillManager.toUserID, currentTurn, currentTurn + 4, 0, 0, 0, 0, 0.5f, skillManager.job, skillManager.cardIndex);
    }
}