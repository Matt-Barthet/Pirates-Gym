using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Go_Blend.Scripts
{
    public class GridSensorComponent : SensorComponent
    {
        public GridSensor gridSensorScript;

        public override ISensor[] CreateSensors()
        {
            Debug.LogError("Reinitializing sensor!");
            return new ISensor[] { gridSensorScript }; // Return an array of sensors
        }
        
        public void ReplaceAndInitializeGridSensorComponent(int newGridWidth, int newGridHeight, float newCellSize)
        {
            // Remove the old GridSensor
            if (gridSensorScript != null)
            {
                Destroy(gridSensorScript);
            }

            // Attach new GridSensor to this GameObject
            gridSensorScript = gameObject.AddComponent<GridSensor>();
            gridSensorScript.gridWidth = newGridWidth;
            gridSensorScript.gridHeight = newGridHeight;
            gridSensorScript.cellSize = newCellSize;

            // Reinitialize the SensorComponent with the new GridSensor
            CreateSensors();
        }
    }
}