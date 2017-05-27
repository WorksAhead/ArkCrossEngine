
namespace ArkCrossEngine
{
    public enum SendMessageOptions
    {
        RequireReceiver = 0,
        DontRequireReceiver = 1
    }

    public enum PrimitiveType
    {

        Sphere = 0,

        Capsule = 1,

        Cylinder = 2,

        Cube = 3,

        Plane = 4,

        Quad = 5
    }

    public enum CameraClearFlags
    {

        Skybox = 1,
        Color = 2,

        SolidColor = 2,

        Depth = 3,

        Nothing = 4
    }

    public enum CollisionFlags
    {
        None = 0,
        Sides = 1,
        CollidedSides = 1,
        Above = 2,
        CollidedAbove = 2,
        Below = 4,
        CollidedBelow = 4
    }
}
