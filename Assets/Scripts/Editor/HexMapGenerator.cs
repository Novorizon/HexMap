//using HexMap;
//using Sirenix.OdinInspector;
//using System.Collections.Generic;
//using UnityEngine;

//namespace WorldMapEditor
//{
//    public partial class HexMapEditor
//    {


//        [LabelText("固定随机数"), PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Property), LabelWidth(100), ReadOnly, VerticalGroup("Random"), ShowIf("isRandom")] 
//        public bool useFixedSeed;

//        [LabelText("启发式抖动"), PropertySpace(SpaceBefore = 3), PropertyOrder((int)Property.Property), LabelWidth(100), ReadOnly, VerticalGroup("Random"), ShowIf("isRandom")]
//        [Range(0f, 0.5f)]
//        public float jitterProbability = 0.25f;

//        [LabelText("启发式抖动"), PropertySpace(SpaceBefore = 3), PropertyOrder((int)Property.Property), LabelWidth(100), ReadOnly, VerticalGroup("Random"), ShowIf("isRandom")]
//        [Range(20, 200)]
//        public int chunkSizeMin = 30;

//        [LabelText("固定随机数"), PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Property), LabelWidth(100), ReadOnly, VerticalGroup("Random"), ShowIf("isRandom")]
//        [Range(20, 200)]
//        public int chunkSizeMax = 100;

//        [LabelText("固定随机数"), PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Property), LabelWidth(100), ReadOnly, VerticalGroup("Random"), ShowIf("isRandom")]
//        [Range(0f, 1f)]
//        public float highRiseProbability = 0.25f;

//        [LabelText("固定随机数"), PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Property), LabelWidth(100), ReadOnly, VerticalGroup("Random"), ShowIf("isRandom")]
//        [Range(0f, 0.4f)]
//        public float sinkProbability = 0.2f;

//        [LabelText("固定随机数"), PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Property), LabelWidth(100), ReadOnly, VerticalGroup("Random"), ShowIf("isRandom")]
//        [Range(5, 95)]
//        public int landPercentage = 50;

//        [LabelText("固定随机数"), PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Property), LabelWidth(100), ReadOnly, VerticalGroup("Random"), ShowIf("isRandom")]
//        [Range(1, 5)]
//        public int waterLevel = 3;

//        [LabelText("固定随机数"), PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Property), LabelWidth(100), ReadOnly, VerticalGroup("Random"), ShowIf("isRandom")]
//        [Range(-4, 0)]
//        public int elevationMinimum = -2;

//        [LabelText("固定随机数"), PropertySpace(SpaceBefore = 10), PropertyOrder((int)Property.Property), LabelWidth(100), ReadOnly, VerticalGroup("Random"), ShowIf("isRandom")]
//        [Range(6, 10)]
//        public int elevationMaximum = 8;

//        HexCellPriorityQueue searchFrontierGenerator;

//        int searchFrontierPhaseGenerator;

//        [Button("随机", ButtonSizes.Medium), HorizontalGroup("Random")]
//        public void RandomSeed()
//        {
//            seed = Random.Range(0, int.MaxValue);
//            seed ^= (int)System.DateTime.Now.Ticks;
//            seed ^= (int)Time.unscaledTime;
//            seed &= int.MaxValue;

//            changed = true;
//            UType = UpdateType.ReCreate;
//        }

//        public void GenerateMap(int x, int z)
//        {
//            Random.State originalRandomState = Random.state;
//            if (!useFixedSeed)
//            {
//                seed = Random.Range(0, int.MaxValue);
//                seed ^= (int)System.DateTime.Now.Ticks;
//                seed ^= (int)Time.unscaledTime;
//                seed &= int.MaxValue;
//            }
//            Random.InitState(seed);

//            cellCount = x * z;
//            GenerateRandomMap(x, z);
//            if (searchFrontierGenerator == null)
//            {
//                searchFrontierGenerator = new HexCellPriorityQueue();
//            }
//            for (int i = 0; i < cellCount; i++)
//            {
//                GetCell(i).WaterLevel = waterLevel;
//            }
//            CreateLand();
//            SetTerrainType();
//            for (int i = 0; i < cellCount; i++)
//            {
//                GetCell(i).SearchPhase = 0;
//            }

//            Random.state = originalRandomState;
//        }

//        void CreateLand()
//        {
//            int landBudget = Mathf.RoundToInt(cellCount * landPercentage * 0.01f);
//            while (landBudget > 0)
//            {
//                int chunkSize = Random.Range(chunkSizeMin, chunkSizeMax - 1);
//                if (Random.value < sinkProbability)
//                {
//                    landBudget = SinkTerrain(chunkSize, landBudget);
//                }
//                else
//                {
//                    landBudget = RaiseTerrain(chunkSize, landBudget);
//                }
//            }
//        }

//        int RaiseTerrain(int chunkSize, int budget)
//        {
//            searchFrontierPhaseGenerator += 1;
//            HexCell firstCell = GetRandomCell();
//            firstCell.SearchPhase = searchFrontierPhaseGenerator;
//            firstCell.Distance = 0;
//            firstCell.SearchHeuristic = 0;
//            searchFrontierGenerator.Enqueue(firstCell);
//            HexCoordinates center = firstCell.coordinates;

//            int rise = Random.value < highRiseProbability ? 2 : 1;
//            int size = 0;
//            while (size < chunkSize && searchFrontierGenerator.Count > 0)
//            {
//                HexCell current = searchFrontierGenerator.Dequeue();
//                int originalElevation = current.Elevation;
//                int newElevation = originalElevation + rise;
//                if (newElevation > elevationMaximum)
//                {
//                    continue;
//                }
//                current.Elevation = newElevation;
//                if (
//                    originalElevation < waterLevel &&
//                    newElevation >= waterLevel && --budget == 0
//                )
//                {
//                    break;
//                }
//                size += 1;

//                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
//                {
//                    HexCell neighbor = current.GetNeighbor(d);
//                        if (neighbor != null && neighbor.SearchPhase < searchFrontierPhaseGenerator)
//                        {
//                        neighbor.SearchPhase = searchFrontierPhaseGenerator;
//                        neighbor.Distance = neighbor.coordinates.DistanceTo(center);
//                        neighbor.SearchHeuristic =
//                            Random.value < jitterProbability ? 1 : 0;
//                        searchFrontierGenerator.Enqueue(neighbor);
//                    }
//                }
//            }
//            searchFrontierGenerator.Clear();
//            return budget;
//        }

//        int SinkTerrain(int chunkSize, int budget)
//        {
//            searchFrontierPhaseGenerator += 1;
//            HexCell firstCell = GetRandomCell();
//            firstCell.SearchPhase = searchFrontierPhaseGenerator;
//            firstCell.Distance = 0;
//            firstCell.SearchHeuristic = 0;
//            searchFrontierGenerator.Enqueue(firstCell);
//            HexCoordinates center = firstCell.coordinates;

//            int sink = Random.value < highRiseProbability ? 2 : 1;
//            int size = 0;
//            while (size < chunkSize && searchFrontierGenerator.Count > 0)
//            {
//                HexCell current = searchFrontierGenerator.Dequeue();
//                int originalElevation = current.Elevation;
//                int newElevation = current.Elevation - sink;
//                if (newElevation < elevationMinimum)
//                {
//                    continue;
//                }
//                current.Elevation = newElevation;
//                if (
//                    originalElevation >= waterLevel &&
//                    newElevation < waterLevel
//                )
//                {
//                    budget += 1;
//                }
//                size += 1;

//                for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
//                {
//                    HexCell neighbor = current.GetNeighbor(d);
//                    if (neighbor!=null && neighbor.SearchPhase < searchFrontierPhaseGenerator)
//                    {
//                        neighbor.SearchPhase = searchFrontierPhaseGenerator;
//                        neighbor.Distance = neighbor.coordinates.DistanceTo(center);
//                        neighbor.SearchHeuristic =
//                            Random.value < jitterProbability ? 1 : 0;
//                        searchFrontierGenerator.Enqueue(neighbor);
//                    }
//                }
//            }
//            searchFrontierGenerator.Clear();
//            return budget;
//        }

//        void SetTerrainType()
//        {
//            for (int i = 0; i < cellCount; i++)
//            {
//                HexCell cell = GetCell(i);
//                if (!cell.IsUnderwater)
//                {
//                    cell.TerrainTypeIndex = cell.Elevation - cell.WaterLevel;
//                }
//            }
//        }

//        HexCell GetRandomCell()
//        {
//            return GetCell(Random.Range(0, cellCount));
//        }


//        public HexCell GetCell(int xOffset, int zOffset)
//        {
//            return cells[xOffset + zOffset * cellCountX];
//        }

//        public HexCell GetCell(int cellIndex)
//        {
//            return cells[cellIndex];
//        }

//    }
//}