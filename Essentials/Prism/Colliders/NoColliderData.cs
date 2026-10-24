
namespace Starlight.Prism.Colliders;

public class NoColliderData : ColliderData
{
    public override Collider AddToGameObject(GameObject obj)
    {
        return null;
    }
}