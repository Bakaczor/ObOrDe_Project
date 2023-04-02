using Base_Representation;

namespace TupleStack_Representation
{
    public class TSLine
    {
        public Tuple<int, Stack<string>> LineTuple;
        public TSLine(string numberHex, int numberDec, string commonName, List<Stop> stops, List<Vehicle> vehicles)
        {
            Stack<string> ST = new();
            LineTuple = new(numberDec, ST);
            ST.Push(numberHex);
            ST.Push(numberDec.ToString());
            ST.Push("2");
            ST.Push(commonName);

            foreach (Stop s in stops) ST.Push(s.Id.ToString());
            ST.Push(stops.Count.ToString());

            foreach (Vehicle v in vehicles) ST.Push(v.Id.ToString());
            ST.Push(vehicles.Count.ToString());
        }
    }
    public class TSStop
    {
        public Tuple<int, Stack<string>> StopTuple;
        public TSStop(int id, List<Line> lines, string name, EType type)
        {
            Stack<string> ST = new();
            StopTuple = new(id, ST);
            ST.Push(id.ToString());
            ST.Push("1");
            ST.Push(name);
            ST.Push(type.ToString());

            foreach (Line l in lines) ST.Push(l.NumberDec.ToString());
            ST.Push(lines.Count.ToString());
        }
    }
    public abstract class TSVehicle
    {
        public Tuple<int, Stack<string>> VehicleTuple;
        public TSVehicle(int id)
        {
            Stack<string> ST = new();
            VehicleTuple = new(id, ST);
            ST.Push(id.ToString());
            ST.Push("1");
        }
    }
    public class TSBytebus : TSVehicle
    {
        public TSBytebus(int id, List<Line> lines, EEngineClass engineClass) : base(id)
        {
            VehicleTuple.Item2.Push(engineClass.ToString());

            foreach (Line l in lines) VehicleTuple.Item2.Push(l.NumberDec.ToString());
            VehicleTuple.Item2.Push(lines.Count.ToString());
        }
    }
    public class TSTram : TSVehicle
    {
        public TSTram(int id, int carsNumber, Line line) : base(id)
        {
            VehicleTuple.Item2.Push(carsNumber.ToString());
            VehicleTuple.Item2.Push(line.NumberDec.ToString());
            VehicleTuple.Item2.Push("1");
        }
    }
    public class TSDriver
    {
        public Tuple<int, Stack<string>> DriverTuple;
        public TSDriver(List<Vehicle> vehicles, string name, string surname, int seniority)
        {

            Stack<string> ST = new();
            int id = (name + "-" + surname).GetHashCode();
            DriverTuple = new(id, ST);
            ST.Push(id.ToString());
            ST.Push("1");
            ST.Push(name);
            ST.Push(surname);
            ST.Push(seniority.ToString());

            foreach (Vehicle v in vehicles) ST.Push(v.Id.ToString());
            ST.Push(vehicles.Count.ToString());
        }
    }
}
