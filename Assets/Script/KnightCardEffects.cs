public class KnightCard0Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.GiveDamageForIDUser(skillManager.toEnemyID, true, currentTurn, currentTurn + 1, 0, 0.50f, 0, 2);
    }
}
public class KnightCard1Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.GiveDamageForIDUser(skillManager.toEnemyID, true, currentTurn, currentTurn + 1, 0, 1.20f, 0, 1);
    }
}
public class KnightCard2Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.GiveDamageForIDUser(skillManager.toEnemyID, true, currentTurn, -1, 0, 0.20f, 0, 1);
    }
}
public class KnightCard3Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        // 3턴간 행동 불가 = +4
        skillManager.cardSpaceCheck.WaitToPrivateAblieTrunFC(currentTurn + 4);
        skillManager.GiveDamageForIDUser(skillManager.toEnemyID, true, currentTurn + 3, currentTurn + 4, 0, 1.00f, 0, 3);
    }
}
public class KnightCard4Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        // 1턴간 행동 불가 = +2
        skillManager.cardSpaceCheck.WaitToPrivateAblieTrunFC(currentTurn + 2);
        skillManager.UpStateForIDUser(skillManager.toUserID, currentTurn + 1, currentTurn + 2, 0, 0, 40f, 0, 0, skillManager.job, skillManager.cardIndex);
    }
}
public class KnightCard5Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.GiveDamageForIDUser(skillManager.toEnemyID, true, currentTurn, currentTurn + 1, 0, 0, 0.30f, 1);
    }
}
