// 파일명: DefenderEffects.cs
public class DefenderCard0Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        // 원본: UpStateForIDUser(toUserID, currentTrun, currentTrun + 1, (float)15/100, 0, 0, 0, 0 , job, cardIndex);
        skillManager.UpStateForIDUser(skillManager.toUserID, currentTurn, currentTurn + 1, 0.15f, 0, 0, 0, 0, skillManager.job, skillManager.cardIndex);
    }
}
public class DefenderCard1Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.GiveDamageForIDUser(skillManager.toEnemyID, true, currentTurn, currentTurn + 1, 0.10f, 0, 0, 1);
    }
}
public class DefenderCard2Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        // -1 mean infinite
        skillManager.UpStateForIDUser(skillManager.toUserID, currentTurn, -1, 0.05f, 0, 0, 0, 0, skillManager.job, skillManager.cardIndex);
    }
}
public class DefenderCard3Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.GiveDamageForIDUser(skillManager.toEnemyID, true, currentTurn + 1, currentTurn + 2, 0, 0, 0.50f, 1);
    }
}
public class DefenderCard4Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.UpStateForIDUser(skillManager.toUserID, currentTurn + 1, currentTurn + 2, 0.50f, 0, 0, 0, 0, skillManager.job, skillManager.cardIndex);
    }
}
public class DefenderCard5Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.UpStateForIDUser(skillManager.toUserID, currentTurn, currentTurn + 3, 0, 0, 0, 0.2f, 0, skillManager.job, skillManager.cardIndex);
    }
}
