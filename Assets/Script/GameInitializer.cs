using UnityEngine;
using Unity.Netcode;

public class GameInitializer : MonoBehaviour
{
    // 인스펙터에서 GameManager 프리팹을 여기다 끌어다 놓으세요.
    public GameObject gameManagerPrefab; 

    void Start()
    {
        // 씬이 켜졌을 때, "내가 방장(Server)이라면" GameManager를 소환한다.
        // 클라이언트는 이 코드를 실행하지 않고, 방장이 소환해주길 기다린다.
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            SpawnGameManager();
        }
    }

    void SpawnGameManager()
    {
        // 1. 프리팹을 게임 세상에 생성 (아직은 나 혼자만 보임)
        GameObject gmInstance = Instantiate(gameManagerPrefab);
        
        // 2. 네트워크에 등록! (이제 모두에게 보이고 공유됨) ⭐중요⭐
        gmInstance.GetComponent<NetworkObject>().Spawn();
        
        Debug.Log("GameManager가 스폰되었습니다!");
    }
}