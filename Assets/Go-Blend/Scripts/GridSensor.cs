using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Go_Blend.Scripts
{
    public class GridSensor : MonoBehaviour, ISensor
    {
        public int gridWidth = 11; // The number of cells in width.
        public int gridHeight = 11; // The number of cells in height.
        public float cellSize = 1.0f; // The size of each cell.
    
        public byte[] GetCompressedObservation()
        {
            return new byte[0]; // Compression is not used in this example.
        }

        public CompressionSpec GetCompressionSpec()
        {
            return new CompressionSpec(SensorCompressionType.None);
        }

        public ObservationSpec GetObservationSpec()
        {
            return ObservationSpec.Visual(gridHeight, gridWidth, 1);
        }
    
        public int Write(ObservationWriter writer)
        {
            var bottomLeft = transform.position + new Vector3(cellSize / 2, cellSize / 2, 0) - new Vector3(gridWidth / 2.0f, gridHeight / 2.0f) * cellSize;
            var index = 0;
            for (var i = 0; i < gridHeight; i++)
            {
                for (var j = 0; j < gridWidth; j++)
                {
                    var cellCenter = bottomLeft + new Vector3(j, i) * cellSize;
                    var highestPriorityEntityType = GetHighestPriorityEntityType(cellCenter);
                    writer[index] = highestPriorityEntityType;
                    index++;
                }
            }
            return gridWidth * gridHeight;
        }
    
        private int GetHighestPriorityEntityType(Vector3 cellCenter)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(cellCenter, cellSize / 2.0f);

            int highestPriority = int.MaxValue;

            foreach (var collider in colliders)
            {
                int currentPriority = GetEntityType(collider);
                if (currentPriority < highestPriority)
                {
                    highestPriority = currentPriority;
                }
            }

            return highestPriority == int.MaxValue ? 0 : highestPriority;
        }

        private int GetEntityType(Collider2D entityCollider)
        {
            if (entityCollider == null) return 0;
            return entityCollider.tag switch
            {
                "Player" => 1,
                "Enemy" => 2,
                "PickUp" => 3,
                "Death" => 4,
                "Bonus" => 5,
                "Obstacle" => 6,
                _ => 7
            };
        }

        public SensorCompressionType GetCompressionType()
        {
            return SensorCompressionType.None;
        }

        public string GetName()
        {
            return "GridSensor";
        }

        public void Update() { }

        private void OnDrawGizmosSelected()
        {
            Vector3 bottomLeft = transform.position + new Vector3(cellSize / 2, cellSize / 2, 0) - new Vector3(gridWidth / 2.0f, gridHeight / 2.0f) * cellSize;

            for (int i = 0; i < gridHeight; i++)
            {
                for (int j = 0; j < gridWidth; j++)
                {
                    Vector3 cellCenter = bottomLeft + new Vector3(j, i) * cellSize;
                    int highestPriorityEntityType = GetHighestPriorityEntityType(cellCenter);

                    // Color based on entity type
                    Gizmos.color = GetColorForEntityType(highestPriorityEntityType);
                    Gizmos.DrawCube(cellCenter, new Vector3(cellSize, cellSize, 0.1f)); // Using DrawCube for filled squares
                    Gizmos.color = Color.black; // Reset to black for wireframe
                    Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize, cellSize, 0.1f));
                }
            }
        }

        private static Color GetColorForEntityType(int entityType)
        {
            return entityType switch
            {
                1 => Color.blue,
                2 => Color.red,
                3 => Color.green,
                4 => Color.magenta,
                5 => Color.yellow,
                6 => Color.grey,
                _ => Color.white
            };
        }
    
        public void Reset() { }
    }
}
