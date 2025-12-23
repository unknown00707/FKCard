public class WizardCard0Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        for (int i = 0; i < skillManager.enemyCulGroup.enemyPrefabs.Length; i++)
        {
            if (skillManager.enemyCulGroup.enemyPrefabs[i].gameObject.activeInHierarchy)
                skillManager.GiveDamageForIDUserServerRpc((ulong)i, true, currentTurn, currentTurn + 1, 0, 0.70f, 0, 1);
        }
    }
}
public class WizardCard1Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.UpStateForIDUserServerRpc(skillManager.toUserID, currentTurn, currentTurn + 4, 0, 0.30f, 0, 0, 0, skillManager.job, skillManager.cardIndex);
    }
}
public class WizardCard2Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.GiveDamageForIDUserServerRpc(skillManager.toEnemyID, true, currentTurn, currentTurn + 6, 0, 0.30f, 0, 1);
    }
}
public class WizardCard3Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        skillManager.GiveDamageForIDUserServerRpc(skillManager.toUserID, false, currentTurn, currentTurn + 1, 0.50f, 0, 0, 1);
        for (int i = 0; i < skillManager.enemyCulGroup.enemyPrefabs.Length; i++)
        {
            if (skillManager.enemyCulGroup.enemyPrefabs[i].gameObject.activeInHierarchy)
                skillManager.GiveDamageForIDUserServerRpc((ulong)i, true, currentTurn, currentTurn + 1, 0, 1.50f, 0, 1);
        }
    }
}
public class WizardCard4Effect : ICardEffect
{
    public void ApplyEffect(PlayerJobSkill skillManager, int currentTurn)
    {
        for (int i = 0; i < skillManager.enemyCulGroup.enemyPrefabs.Length; i++)
        {
            if (skillManager.enemyCulGroup.enemyPrefabs[i].gameObject.activeInHierarchy)
                skillManager.GiveDamageForIDUserServerRpc((ulong)i, true, currentTurn, currentTurn + 1, 0, 3.00f, 0, 1);
        }
        for (int i = 0; i < GameManager.Instance.SendPlayerTotalNum(); i++)
        {
            skillManager.GiveDamageForIDUserServerRpc((ulong)i, false, currentTurn, currentTurn + 1, 0.30f, 0, 0, 1);
        }
    }
}