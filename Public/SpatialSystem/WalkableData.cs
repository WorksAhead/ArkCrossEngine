using System.IO;
using ArkCrossEngine;

namespace ArkCrossEngineSpatial.Cow
{
    public class WalkableData
    {
        public Vector2 unclampedGridSize { get; private set; }
        public Vector2 gridSize { get; private set; }
        public Vector3 gridCoordinateCenter { get; private set; }

        public float nodeSize { get; private set; }
        public int maxNodeNumInWidth { get; private set; }
        public int maxNodeNumInDepth { get; private set; }
        public int nodeNumInWidth { get; private set; }
        public int nodeNumInDepth { get; private set; }

        bool m_NodeSizeSelfAdaption = true;
        byte[] m_Nodes = null;

        Matrix4x4 m_Matrix = Matrix4x4.identity;
        Matrix4x4 m_InverseMatrix = Matrix4x4.identity;

        public bool Initial(string filename)
        {
            bool result = false;
            using (MemoryStream ms = FileReaderProxy.ReadFileAsMemoryStream(filename))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    float unclampedGridSizeX = br.ReadSingle();
                    float unclampedGridSizeY = br.ReadSingle();
                    unclampedGridSize = new Vector2(unclampedGridSizeX, unclampedGridSizeY);

                    float gridCoordinateCenterX = br.ReadSingle();
                    float gridCoordinateCenterY = br.ReadSingle();
                    float gridCoordinateCenterZ = br.ReadSingle();
                    gridCoordinateCenter = new Vector3(gridCoordinateCenterX, gridCoordinateCenterY, gridCoordinateCenterZ);

                    nodeSize = br.ReadSingle();

                    maxNodeNumInWidth = br.ReadInt32();
                    maxNodeNumInDepth = br.ReadInt32();

                    m_NodeSizeSelfAdaption = br.ReadBoolean();

                    result = GenerateMatrix();
                    if (result)
                    {
                        m_Nodes = new byte[nodeNumInWidth * nodeNumInDepth];
                        for (int z = 0; z < nodeNumInDepth; z++)
                        {
                            for (int x = 0; x < nodeNumInWidth; x++)
                            {
                                byte walkable = br.ReadByte();
                                m_Nodes[z * nodeNumInWidth + x] = walkable;
                            }
                        }
                    }

                    br.Close();
                }
            }

            return result;
        }

        bool GenerateMatrix()
        {
            Vector2 newSize = unclampedGridSize;

            newSize.x *= Mathf.Sign(newSize.x);
            newSize.y *= Mathf.Sign(newSize.y);

            if (nodeSize < newSize.x / (float)maxNodeNumInWidth || nodeSize < newSize.y / (float)maxNodeNumInDepth)
            {
                if (m_NodeSizeSelfAdaption)
                {
                    LogSystem.Debug("Gird width and depth are not able to over num " + maxNodeNumInWidth + " and " + maxNodeNumInDepth + ".\n" +
                                     "Node size will be scaled automatically to fit this constraint.");
                }
                else
                {
                    LogSystem.Debug("Gird width and depth are not able to over num " + maxNodeNumInWidth + " and " + maxNodeNumInDepth);
                    return false;
                }
            }

            float nodeWidthSize = Mathf.Clamp(nodeSize, newSize.x / (float)maxNodeNumInWidth, Mathf.Infinity);
            float nodeDepthSize = Mathf.Clamp(nodeSize, newSize.y / (float)maxNodeNumInDepth, Mathf.Infinity);
            nodeSize = Mathf.Max(nodeWidthSize, nodeDepthSize);

            newSize.x = newSize.x < nodeSize ? nodeSize : newSize.x;
            newSize.y = newSize.y < nodeSize ? nodeSize : newSize.y;

            gridSize = newSize;

            nodeNumInWidth = Mathf.FloorToInt(gridSize.x / nodeSize);
            nodeNumInDepth = Mathf.FloorToInt(gridSize.y / nodeSize);

            if (Mathf.Approximately(gridSize.x / nodeSize, Mathf.CeilToInt(gridSize.x / nodeSize)))
            {
                nodeNumInWidth = Mathf.CeilToInt(gridSize.x / nodeSize);
            }

            if (Mathf.Approximately(gridSize.y / nodeSize, Mathf.CeilToInt(gridSize.y / nodeSize)))
            {
                nodeNumInDepth = Mathf.CeilToInt(gridSize.y / nodeSize);
            }

            float remainderX = gridSize.x - ((float)nodeNumInWidth * nodeSize);
            float remainderY = gridSize.y - ((float)nodeNumInDepth * nodeSize);

            Vector3 newCenter = new Vector3(gridCoordinateCenter.x + remainderX * 0.5f, gridCoordinateCenter.y + 0.5f, gridCoordinateCenter.z + remainderY * 0.5f);

            Matrix4x4 m = Matrix4x4.TRS(newCenter, Quaternion.Euler(Vector3.zero), new Vector3(nodeSize, 1, nodeSize));

            m_Matrix = m;
            m_InverseMatrix = m.inverse;

            return true;
        }

        public Int3 GetWorldPosition(int xIndex, int zIndex, float height)
        {
            return (Int3)m_Matrix.MultiplyPoint3x4(new Vector3(xIndex + 0.5f, height, zIndex + 0.5f));
        }

        public void GetGridIndex(Vector3 worldPosition, out int xIndex, out int zIndex)
        {
            Vector3 gridPos = m_InverseMatrix.MultiplyPoint3x4(worldPosition);

            xIndex = Mathf.FloorToInt(gridPos.x);
            zIndex = Mathf.FloorToInt(gridPos.z);
        }

        public byte GetWalkableStatus(int xIndex, int zIndex)
        {
            return m_Nodes[zIndex * nodeNumInWidth + xIndex];
        }
    }
}