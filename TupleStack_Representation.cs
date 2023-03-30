using Base_Representation;
using System;
using System.Collections.Generic;

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
        //public EEngineClass EngineClass
        //{
        //    get
        //    {
        //        Stack<string> ST = new(VehicleTuple.Item2.Reverse());

        //        int num = int.Parse(ST.Pop());
        //        for (int i = 0; i < num; i++) ST.Pop();
        //        return (EEngineClass)Enum.Parse(typeof(EEngineClass), ST.Pop());
        //    }
        //}
        public TSBytebus(int id, List<Line> lines, EEngineClass engineClass) : base(id)
        {
            VehicleTuple.Item2.Push(engineClass.ToString());

            foreach (Line l in lines) VehicleTuple.Item2.Push(l.NumberDec.ToString());
            VehicleTuple.Item2.Push(lines.Count.ToString());
        }
    }
    public class TSTram : TSVehicle
    {
        //public int CarsNumber
        //{
        //    get
        //    {
        //        Stack<string> ST = new(VehicleTuple.Item2.Reverse());
        //        ST.Pop(); ST.Pop();
        //        return int.Parse(ST.Pop());
        //    }
        //}
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

    /*
    public class TSRep
    {
        public Dictionary<int, TSLine> lines = new();
        public Dictionary<int, TSStop> stops = new();
        public Dictionary<int, TSVehicle> vehicles = new();
        public Dictionary<string, TSDriver> drivers = new();

        public TSRep(List<Line> lines, List<Stop> stops, List<Vehicle> vehicles, List<Driver> drivers)
        {
            foreach (Line line in lines) this.lines.Add(line.NumberDec, new TSLine(line.NumberHex, line.NumberDec, line.CommonName, line.stops, line.vehicles));

            foreach (Stop stop in stops) this.stops.Add(stop.Id, new TSStop(stop.Id, stop.lines, stop.Name, stop.Type));

            foreach (Vehicle vehicle in vehicles)
            {
                if (vehicle is Bytebus b) this.vehicles.Add(b.Id, new TSBytebus(b.Id, b.lines, b.EngineClass));
                else if (vehicle is Tram t) this.vehicles.Add(t.Id, new TSTram(t.Id, t.CarsNumber, t.line));
            }
            foreach (Driver driver in drivers) this.drivers.Add(driver.Name + "-" + driver.Surname, new TSDriver(driver.vehicles, driver.Name, driver.Surname, driver.Seniority));
        }
    }
    */
}
