
namespace Starlight.Prism.Colliders;

public class CapsuleColliderData : ColliderData
{
    public float Radius;
    public float Height;

    public float GetRadius() => Radius;
    public float GetHeight() => Height;
    
    public override Collider AddToGameObject(GameObject obj)
    {
        var col = obj.AddComponent<CapsuleCollider>();
        col.radius = GetRadius();
        col.height = GetHeight();
        col.direction = 0;
        return col;
    }
}