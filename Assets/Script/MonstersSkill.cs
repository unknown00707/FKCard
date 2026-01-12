public class Biting : IMonasterEffect
{
    public void ApplyEffect(EnemyCulGroup enemyCulGroup, int selfID, int currentTurn)
    {
        EnemyCardData enemyCardData = enemyCulGroup.activeMonsterDic[selfID];
        AboutDamages aboutDamages = new()
        {
            targetID = enemyCulGroup.FindPlayerToAttack(),
            damageAmount = 0.5f * enemyCardData.enemyDamage,
            numberOfHits = 1,
            takenDamage = 0f,
            currentTurn = currentTurn
        };
        enemyCulGroup.Attack(aboutDamages, selfID, false, currentTurn, currentTurn);
    }
}