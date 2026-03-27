using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Check Enemy Distance", story: "Khoảng cách giữa [Self] và [Target] có nhỏ hơn [Distance] mét?", category: "Action/HaoScriptAI", id: "hao_distance_check_12345")]
public partial class CheckDistanceAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Distance;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        // Thất bại nếu không truyền đủ Object
        if (Self.Value == null || Target.Value == null) 
        {
            return Status.Failure;
        }
        
        // Đo khoảng cách
        float dist = Vector3.Distance(Self.Value.transform.position, Target.Value.transform.position);
        
        // Nếu nhỏ hơn thông số yêu cầu -> SUCCESS (Trúng Điều Kiện)
        if (dist < Distance.Value)
        {
            return Status.Success;
        }
        
        // Nếu xa hơn -> FAILURE (Sai Điều Kiện)
        return Status.Failure;
    }
}
