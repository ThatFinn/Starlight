
namespace Starlight.Prism.Colliders;

public class SphereColliderData : ColliderData
{
    public float Radius;
    public float GetRadius() => Radius;
    public override Collider AddToGameObject(GameObject obj)
    {
        var col = obj.AddComponent<SphereCollider>();
        col.radius = GetRadius();
        return col;
    }
}