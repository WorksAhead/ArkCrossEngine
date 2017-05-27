namespace ArkCrossEngine
{
    public interface ILosBoard
    {
        bool Contains(int x, int y);
        bool IsObstacle(int x, int y);
        void Visit(int x, int y);
    }
}
