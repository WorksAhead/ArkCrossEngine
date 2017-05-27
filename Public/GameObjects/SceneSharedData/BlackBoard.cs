namespace ArkCrossEngine
{
    public class BlackBoard
    {
        public TypedDataCollection BlackBoardDatas
        {
            get { return m_BlackBoardDatas; }
        }
        public void Reset()
        {
            m_BlackBoardDatas.Clear();
        }

        private TypedDataCollection m_BlackBoardDatas = new TypedDataCollection();
    }
}
