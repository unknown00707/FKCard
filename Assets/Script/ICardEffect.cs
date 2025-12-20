public interface ICardEffect
{
    // 스킬 매니저(PlayerJobSkill)의 기능들을 써야 하므로 매개변수로 받습니다.
    void ApplyEffect(PlayerJobSkill skillManager, int currentTurn);
}