using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Go_Blend.Scripts
{
    public class GridSensorComponent : SensorComponent
    {
        public GridSensor gridSensorScript;

        public override ISensor[] CreateSensors()
        {
            GetCommandLineArgs();
            return new ISensor[] { gridSensorScript }; // Return an array of sensors
        }
        
        private void GetCommandLineArgs()
        {
            var args = System.Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-gridWidth")
                {
                    if (i + 1 < args.Length)
                    {
                        gridSensorScript.gridWidth = int.Parse(args[i + 1]);
                    }
                }
                else if (args[i] == "-gridHeight")
                {
                    if (i + 1 < args.Length)
                    {
                        gridSensorScript.gridHeight = int.Parse(args[i + 1]);
                    }
                }
                else if (args[i] == "-elementSize")
                {
                    if (i + 1 < args.Length)
                    {
                        gridSensorScript.cellSize = int.Parse(args[i + 1]);
                    }
                }
            }
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