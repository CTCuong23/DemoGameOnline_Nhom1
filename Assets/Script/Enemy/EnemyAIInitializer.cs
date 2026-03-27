using Fusion;
using UnityEngine;
using Unity.VisualScripting; // Script này sử dụng Unity Visual Scripting (Component: StateMachine)

[RequireComponent(typeof(NetworkObject))] // Đã bỏ bắt buộc StateMachine vì người dùng xài Behavior Agent
public class EnemyAIInitializer : NetworkBehaviour
{
    public override void Spawned()
    {
        // 🚨 CHÌA KHÓA MULTIPLAYER AI NẰM Ở ĐÂY:
        // Nếu cái máy người chơi này KHÔNG PHẢI LÀ HOST (tức là không có quyền State Authority)
        // Thì tắt luôn khối óc của con quái đi!
        if (!HasStateAuthority)
        {
            // Tắt Unity Visual Scripting (Nếu dùng)
            var stateMachine = GetComponent<StateMachine>();
            if (stateMachine != null)
            {
                stateMachine.enabled = false;
                Debug.Log($"[{gameObject.name}] Đã tắt StateMachine trên Client!");
            }

            // Tắt Unity 6 Behavior Agent (Nếu dùng)
            var behaviorAgent = GetComponent("BehaviorAgent") as UnityEngine.Behaviour;
            if (behaviorAgent != null)
            {
                behaviorAgent.enabled = false;
                Debug.Log($"[{gameObject.name}] Đã tắt BehaviorAgent trên Client!");
            }
        }
    }
}
