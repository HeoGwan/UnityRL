using System;
using UnityEngine;

/// <summary>
/// 벽 파편 정의 클래스
/// </summary>
public class WallPart : MonoBehaviour
{
    // 부모의 Wall 컴포넌트 저장
    public Wall parent;

    private void Awake()
    {
        // 부모 컴포넌트 가져오기
        parent = transform.parent.GetComponent<Wall>();
    }

    /// <summary>
    /// 바닥과 부딪힘 감지
    /// </summary>
    /// <param name="other"></param>
    private void OnCollisionEnter2D(Collision2D other)
    {
        // Debug.Log("OnTriggerEnter2D");
        if (other.gameObject.CompareTag("Platform"))
        {
            parent.TriggerToPlatform();
        }
        else if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().Die();
        }
    }
}