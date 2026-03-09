using UnityEngine;

public class FollowHeadUI : MonoBehaviour
{
    public Transform head;         // 拖入 Main Camera（XR Origin 下的 Camera）
    public float distance = 2f;    // 距离玩家多远
    public Vector3 offset = new Vector3(0, -0.3f, 0);  // 可选：垂直或横向偏移

    void LateUpdate()
    {
        if (head == null) return;

        // 设置位置：始终在玩家前方 distance 米
        Vector3 targetPosition = head.position + head.forward * distance + offset;
        transform.position = targetPosition;

        // 设置朝向：面向玩家（反向朝向玩家前方）
        Vector3 lookDirection = transform.position - head.position;
        lookDirection.y = 0; // 可选：只绕 Y 轴旋转
        if (lookDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(lookDirection);
    }
}
