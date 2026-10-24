
namespace Starlight.Prism.Colliders;

public class MeshColliderData : ColliderData
{
    public Mesh Mesh;
    public Mesh GetMesh() => Mesh;
    
    public override Collider AddToGameObject(GameObject obj)
    {
        var col = obj.AddComponent<MeshCollider>();
        col.convex = true;
        col.sharedMesh = GetMesh();
        return col;
    }
}