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

    // 게임 카드 발동 관련
    public void ReciveSignCardBatchOnStage(Transform cardData)
    {
        if (!IsOwner) return;

        RequestCardBatchTopublicServerRpc(cardData);
    }
    [ServerRpc]
    private void RequestCardBatchTopublicServerRpc(Transform cardData)
    {
        if(GameManager.Instance != null)
            GameManager.Instance.RequsetMakeSameCardPublic(cardData, OwnerClientId);
    }

    public void Damage(float damage)
    {
       
    }

}