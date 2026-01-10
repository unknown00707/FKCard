public class KnightCard0Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.StoreDamageData(skillManager.toEnemyID, true, currentTurn, currentTurn + 1, 0, 0.50f, 0, 2);
    }
}
public class KnightCard1Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.StoreDamageData(skillManager.toEnemyID, true, currentTurn, currentTurn + 1, 0, 1.20f, 0, 1);
    }
}
public class KnightCard2Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.StoreDamageData(skillManager.toEnemyID, true, currentTurn, -1, 0, 0.20f, 0, 1);
    }
}
public class KnightCard3Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.turnManager.RequsetDelayTurn(4);
        skillManager.StoreDamageData(skillManager.toEnemyID, true, currentTurn + 3, currentTurn + 4, 0, 1.00f, 0, 3);
    }
}
public class KnightCard4Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.turnManager.RequsetDelayTurn(2);
        skillManager.UpStateByBuffe(skillManager.toUserID, currentTurn + 1, currentTurn + 2, 0, 0, 40f, 0, 0, skillManager.job, skillManager.cardIndex);
    }
}
public class KnightCard5Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.StoreDamageData(skillManager.toEnemyID, true, currentTurn, currentTurn + 1, 0, 0, 0.30f, 1);
    }
}
