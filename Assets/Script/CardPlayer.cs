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

    // 게임 카드 정보 저장 발동 관련 -----------------------------------

    [ServerRpc]
    public void RequsetStoreStateByBuffeServerRpc(UpStateData package)
    {
        if(GameManager.Instance != null)
            GameManager.Instance.InStorUserUpStatTemproy(package, OwnerClientId);
    }

    [ServerRpc]
    public void RequsetStoreDamageServerRpc(UserHitDamage package)
    {
        if(GameManager.Instance != null)
            GameManager.Instance.InStorUserDamageTemproy(package, OwnerClientId);
    }

    [ServerRpc]
    public void RequsetUpStateByBuffeServerRpc(UpStateData package)
    {
        if(GameManager.Instance != null)
            GameManager.Instance.InUserUpStat(package);
    }
    [ServerRpc]
    public void RequsetUpDamageServerRpc(UserHitDamage package)
    {
        if(GameManager.Instance != null)
            GameManager.Instance.InDamageToUser(package, OwnerClientId);
    }
    [ServerRpc]
    public void RequsetHealDataSendServerRpc(UserHealData healData)
    {
        if(GameManager.Instance != null)
            GameManager.Instance.InComeHealTemproy(healData);
    }

    // About Player Turn -----------------------------------
    public void RequsetUseCardSignal()
    {
        if (!IsOwner) return;

        RequsetUseCardSignalServerRpc();
    }
    [ServerRpc]
    private void RequsetUseCardSignalServerRpc()
    {
        if(GameManager.Instance != null)
            GameManager.Instance.InUseCardSignal(OwnerClientId);
    }

    public void RequsetDelayForNextTurn(int delayTime)
    {
        if (!IsOwner) return;

        RequsetDelayForNextTurnServerRpc(delayTime);
    }
    [ServerRpc]
    private void RequsetDelayForNextTurnServerRpc(int delayTime)
    {
        if(GameManager.Instance != null)
            GameManager.Instance.InDelayForNextTurn(delayTime, OwnerClientId);
    }
}