using UnityEngine;

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
        for (int i = 0; i < GameManager.Instance.SendPlayerTotalNum(); i++)
        {
            skillManager.GiveHealToTemproy((ulong)i, currentTurn, currentTurn + 1, 0.075f);
        }
    }
}
public class HealerCard2Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.MakeSubstitute((int)skillManager.toUserID, 0.75f);
    }
}

public class HealerCard3Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        for (int i = 0; i <  GameManager.Instance.SendPlayerTotalNum(); i++)
        {
            skillManager.GiveHealToTemproy((ulong)i, currentTurn, currentTurn + 1, Random.Range(3f, 16f) / 100);
        }
    }
}
public class HealerCard4Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        for (int i = 0; i < GameManager.Instance.SendPlayerTotalNum(); i++)
        {
            skillManager.UpStateByBuffe((ulong)i, currentTurn, currentTurn + 1, 0.05f, 0, 0, 0, 0, skillManager.job, skillManager.cardIndex);
        }
    }
}
public class HealerCard5Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.ReviveTeamMember((int)skillManager.toUserID);
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
        skillManager.UpStateByBuffe(skillManager.toUserID, currentTurn, currentTurn + 4, 0, 0, 0, 0, 0.5f, skillManager.job, skillManager.cardIndex);
    }
}