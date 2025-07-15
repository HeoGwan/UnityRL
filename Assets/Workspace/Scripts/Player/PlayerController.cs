using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 플레이어를 관리하는 컨트롤러
/// </summary>
public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// 플레이어 사망 처리
    /// </summary>
    public void Die()
    {
        Debug.Log("사망처리");
        
        GetComponent<PlayerMoveController>().AddReward(-1f);
        GetComponent<PlayerMoveController>().EndEpisode();
        WallManager.Instance.Initialize();
        // // 게임 다시 시작을 위해 Game 씬을 로드함
        // SceneManager.LoadScene("Game");
    }
}