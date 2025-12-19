using UnityEngine;
using Unity.Netcode; // 이게 있어야 네트워크 기능 사용 가능


// MonoBehaviour 대신 NetworkBehaviour를 상속받아야 해!
public class CardPlayer : NetworkBehaviour
{

    // 이 객체가 네트워크상에서 생성(Spawn)되었을 때 딱 한 번 실행됨
    public override void OnNetworkSpawn()
    {
        // IsOwner: 이 캐릭터의 주인(조종 권한이 있는 사람)이 '나'인가?
        if (IsOwner)
        {
            // 내 캐릭터는 파란색
            GetComponent<Renderer>().material.color = Color.blue;
            Debug.Log("내 캐릭터가 생성되었습니다!");
        }
        else
        {
            // 남의 캐릭터는 빨간색
            GetComponent<Renderer>().material.color = Color.red;
        }
    }

    // 직업 선택 , 변경 관련
    public void ReciveJobs(JobManager.Jobs job)
    {
       if (!IsOwner) return;
    
        RequestUserJobServerRpc(OwnerClientId, job);
    }

    [ServerRpc]
    private void RequestUserJobServerRpc(ulong index ,JobManager.Jobs job)
    {
        
        if(GameManager.Instance != null)
            GameManager.Instance.InDicUserJobValue(index, job);
    }

    // 대기 , 준비 관련
    public void ReciveReadySign(bool isReady)
    {
        if (!IsOwner) return;

        RequestUserReadySignServerRpc(isReady);
    }

    [ServerRpc]
    private void RequestUserReadySignServerRpc(bool isReady)
    {
        if(GameManager.Instance != null)
            GameManager.Instance.InPlayerReadySign(OwnerClientId, isReady);
    }

    // 게임 시작 관련
    public void ReciveGameStartSign()
    {
        if (!IsServer) return;

        RequestGameStartSignServerRpc();
    } 

    [ServerRpc]
    private void RequestGameStartSignServerRpc()
    {
        if(GameManager.Instance != null)
            GameManager.Instance.InGameStartSign();
    }

    public void ReciveGameSetReadySing(float hp, float dg, float crip, float damageTakenMultiplier, float beneficialEffectMultiplier)
    {
        if (!IsServer) return;

        RequestGameUserSetSignServerRpc(hp, dg, crip, damageTakenMultiplier, beneficialEffectMultiplier);
    }

    [ServerRpc]
    private void RequestGameUserSetSignServerRpc(float hp, float dg, float crip,float damageTakenMultiplier, float beneficialEffectMultiplier)
    {
        if(GameManager.Instance != null)
            GameManager.Instance.InGameUserJobStatSame(hp, dg, crip, damageTakenMultiplier, beneficialEffectMultiplier, OwnerClientId);
    }

    // 게임 카드 발동 관련
    public void ReciveSignCardBatchOnStage(JobManager.Jobs job, int index)
    {
        if (!IsOwner) return;

        RequestCardBatchTopublicServerRpc(job, index);
    }

    [ServerRpc]
    private void RequestCardBatchTopublicServerRpc(JobManager.Jobs job, int index)
    {
        if(GameManager.Instance != null)
            GameManager.Instance.RequsetMakeSameCardPublic(job, index, OwnerClientId);
    }
    public void ReciveSignCardEffectReady(bool isReady)
    {
        if (!IsOwner) return;

        RequsetCardReadyToEffectServerRpc(isReady);
    }

    [ServerRpc]
    private void RequsetCardReadyToEffectServerRpc(bool isReady)
    {
        if(GameManager.Instance != null)
            GameManager.Instance.InCardEffectReady(isReady, OwnerClientId);
    }

    // 유저가 발동한 카드로 인한 증가된 스텟 저장
    public void UpUserStatTemporary(ulong userId, int startDurTimeTrun, int endDurTineTrun, float hp, float dg, float critical, float damageMultiplier, float beneficialEffectMultiplier,JobManager.Jobs cardType, int cardIndex)
    {
        if (!IsOwner) return;

        UpUserStatTemporaryServerRpc(userId, startDurTimeTrun, endDurTineTrun, hp, dg, critical, damageMultiplier, beneficialEffectMultiplier,cardType, cardIndex);
    }
    [ServerRpc]
    private void UpUserStatTemporaryServerRpc(ulong userId, int startDurTimeTrun, int endDurTineTrun, float hp, float dg, float critical, float damageMultiplier, float beneficialEffectMultiplier,JobManager.Jobs cardType, int cardIndex)
    {
        if(GameManager.Instance != null)
            GameManager.Instance.InStorUserUpStatTemproy(userId, startDurTimeTrun, endDurTineTrun, hp, dg, critical, damageMultiplier, beneficialEffectMultiplier,OwnerClientId, cardType, cardIndex);
    }
    public void UpUserDamageTemporary(ulong userId, bool isForEnemy ,int startDurTimeTrun, int endDurTineTrun, float hp, float dg, float takenDg, int numberOfHits)
    {
        if (!IsOwner) return;

        UpUserDamageTemporaryServerRpc(userId, isForEnemy, startDurTimeTrun, endDurTineTrun, hp, dg, takenDg, numberOfHits);
    }
    [ServerRpc]
    private void UpUserDamageTemporaryServerRpc(ulong userId, bool isForEnemy ,int startDurTimeTrun, int endDurTineTrun, float hp, float dg, float takenDg, int numberOfHits)
    {
        if(GameManager.Instance != null)
            GameManager.Instance.InStorUserDamageTemproy(userId, isForEnemy, startDurTimeTrun, endDurTineTrun, hp, dg, takenDg, numberOfHits, OwnerClientId);
    }
    public void UpUserStatIFO(int userID ,float hpIng, float dGing, float criticalIng, float damageMultiplier, float beneficialEffectMultiplier,JobManager.Jobs cardType, int cardIndex)
    {
       if (!IsOwner) return;

        RequetUpUserStatIFOServerRpc(userID, hpIng, dGing, criticalIng, damageMultiplier, beneficialEffectMultiplier,cardType, cardIndex);
    }
    [ServerRpc]
    private void RequetUpUserStatIFOServerRpc(int userID, float hpIng, float dGing, float criticalIng, float damageMultiplier, float beneficialEffectMultiplier,JobManager.Jobs cardType, int cardIndex)
    {
        if(GameManager.Instance != null)
            GameManager.Instance.InUserUpStat(hpIng, dGing, criticalIng, damageMultiplier, beneficialEffectMultiplier, (ulong)userID,  cardType, cardIndex);
    }
    // 유저가 가할 데미지 증가
    public void UpDamageUserInOut(ulong enemyID, bool isToEnemy, float damageFromHp, float damagFromDg, float damageFromTakenDg, int numberOfHits)
    {
        if (!IsOwner) return;

        RequsetUpDamageServerRpc(enemyID, isToEnemy, damageFromHp, damagFromDg, damageFromTakenDg, numberOfHits);
    }
    [ServerRpc]
    private void RequsetUpDamageServerRpc(ulong enemyID, bool isToEnemy, float damageFromHp, float damagFromDg, float damageFromTakenDg, int numberOfHits)
    {
        if(GameManager.Instance != null)
            GameManager.Instance.InDamageToUser(enemyID , isToEnemy, OwnerClientId, damageFromHp, damagFromDg, damageFromTakenDg, numberOfHits);
    }
    // 유저가 줄 힐량 전송
    public void ToHealTemporary(ulong userId, int startDurTimeTrun, int endDurTineTrun, float healAmount)
    {
        if (!IsOwner) return;

        RequsetUpToUserHealTemporayServerRpc(userId, startDurTimeTrun, endDurTineTrun, healAmount);
    }
    [ServerRpc]
    private void RequsetUpToUserHealTemporayServerRpc(ulong userId, int startDurTimeTrun, int endDurTineTrun, float healAmount)
    {
        if(GameManager.Instance != null)
            GameManager.Instance.InComeHealTemproy(userId, startDurTimeTrun, endDurTineTrun, healAmount);
    }
    public void UpHealToUser(ulong userId, int startDurTimeTrun, int endDurTineTrun, float healAmount)
    {
        
    }
}