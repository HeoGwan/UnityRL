using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 벽 정의 클래스
/// </summary>
public class Wall : MonoBehaviour
{
    // 움직임을 위한 컴포넌트
    public Rigidbody2D rigid;
    
    // 속도 조정 변수
    public float upSpeed = 3f;
    
    // 기본 y 좌표
    public float y = 10f;
    
    // 바닥에 맞은 뒤 다 올라가면 호출하는 콜백함수
    public Action onEndUp;
    
    // 안전지대 위치
    public Vector3 safeAreaPosition;
    
    private void Awake()
    {
        // 컴포넌트 가져오기
        rigid = GetComponent<Rigidbody2D>();
        
        // 벽이 떨어지지 않게 설정
        rigid.gravityScale = 0f;
        
        // 기본 위치 설정
        transform.position = new Vector3(0, y, 0);
        
        // 벽 비활성화
        gameObject.SetActive(false);
        
        // 안전지대 위치 설정
        safeAreaPosition = transform.Find("SafeArea").position;
    }


    public Wall Initialize(float fallSpeed, Action cb)
    {
        // 중력 설정
        rigid.gravityScale = fallSpeed;
        
        // 콜백 함수 설정
        onEndUp = cb;
        
        return this;
    }
    
    /// <summary>
    /// 벽 파편이 바닥에 닿였을 경우 호출되는 함수
    /// </summary>
    public void TriggerToPlatform()
    {
        // 바닥에 닿았을 경우 떨어지는 것을 멈춤
        rigid.gravityScale = 0;
        
        // 올라가는 코루틴 실행
        if (gameObject.activeSelf) StartCoroutine(GoUp());
    }
    
    /// <summary>
    /// 생존할 수 있는 구멍의 X좌표를 가져옴
    /// </summary>
    /// <returns></returns>
    public float GetHoleX()
    {
        // 모든 WallPart의 X 좌표를 기준으로 gap의 중심을 추정
        var parts = GetComponentsInChildren<WallPart>();
        if (parts.Length != 2)
        {
            Debug.LogWarning("WallPart가 2개가 아님");
            return 0f; // fallback
        }

        float leftX = parts[0].transform.position.x;
        float rightX = parts[1].transform.position.x;

        // 예: gap이 벽 중앙일 경우 => WallPart 둘 사이의 중간
        if (leftX > rightX)
        {
            (leftX, rightX) = (rightX, leftX);
        }

        float wallWidth = rightX - leftX;
        float wallPartWidth = parts[0].GetComponent<SpriteRenderer>().bounds.size.x;
        float gapX = leftX + wallPartWidth + (wallWidth - 2 * wallPartWidth) / 2f;

        return gapX;
    }

    /// <summary>
    /// 바닥에 닿고 올라가는 로직 구현
    /// </summary>
    IEnumerator GoUp()
    {
        // 특정 y 좌표가 되기 전이면 계속해서 올라감
        Vector3 move = Vector3.up;
        while (transform.position.y < y)
        {
            transform.position += (move * (upSpeed * Time.deltaTime));
            yield return null;
        }
        
        transform.position = new Vector3(0, y, 0);
        
        // 다 올라갔음을 알려주기 위한 호출
        // WallManager에게 알려줌
        onEndUp?.Invoke();
    }
}