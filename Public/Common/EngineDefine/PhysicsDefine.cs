namespace ArkCrossEngine
{
    public struct RaycastHit
    {
        public Vector3 point { get; set; }
        public object collider { get; set; }
        public Vector3 normal { get; set; }
    }
}