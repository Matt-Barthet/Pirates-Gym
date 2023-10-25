using Unity.MLAgents.Sensors;

namespace Go_Blend.Scripts
{
    public class GridSensorComponent : SensorComponent
    {
        public GridSensor gridSensorScript;

        public override ISensor[] CreateSensors()
        {
            return new ISensor[] { gridSensorScript }; // Return an array of sensors
        }
    }
}