using UnityEngine;

namespace HexMap
{

    public class HexChunk : MonoBehaviour
    {
        public HexMesh Terrain;
        public HexMesh Rivers;
        public HexMesh Roads;
        public HexMesh Water;
        public HexMesh WaterShore;
        public HexMesh Estuary;
        public HexFeature Features;
        public void Init()
        {
            Terrain = new HexMesh();
            Rivers = new HexMesh();
            Roads = new HexMesh();
            Water = new HexMesh();
            WaterShore = new HexMesh();
            Estuary = new HexMesh();
            Features = new HexFeature();
        }
    }
}