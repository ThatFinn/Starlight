
namespace Starlight.Prism.Colliders;

public class CubeColliderData : ColliderData
{
    public Vector3 Size;

    public float GetMagnitude() => Size.magnitude;
    public float GetWidth() => Size.x;
    public float GetHeight() => Size.y;
    public float GetDepth() => Size.z;
    
    public override Collider AddToGameObject(GameObject obj)
    {
        var col = obj.AddComponent<BoxCollider>();
        col.size = new Vector3(GetWidth(), GetHeight(), GetDepth());
        return col;
    }
}