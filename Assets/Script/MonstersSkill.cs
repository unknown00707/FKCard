public class Biting : IMonasterEffect
{
    public void ApplyEffect(EnemyCulGroup enemyCulGroup, int selfID, int currentTurn)
    {
        EnemyCardData enemyCardData = enemyCulGroup.activeMonsterDic[selfID];
        AboutDamages aboutDamages = new()
        {
            targetID = enemyCulGroup.FindPlayerToAttack(),
            isTimeAttack = false,
            damageAmount = 0.5f * enemyCardData.enemyDamage,
            numberOfHits = 1,
            startTrunNum = currentTurn,
            endTrunNum = currentTurn
        };
        enemyCulGroup.Attack(aboutDamages, selfID);
    }
}