using System.Text;
using Interfaces;

namespace Base_Representation
{
    public enum EType
    {
        bus = 1, tram = 2, other = 3
    }
    public enum EEngineClass
    {
        Byte5 = 1, bisel20 = 2, gibgaz = 3
    }
    public class Line : ILine
    {
        public List<Stop> stops;
        public List<Vehicle> vehicles;
        public string NumberHex { get; set; }
        public int NumberDec { get; set; }
        public string CommonName { get; set; }
        public List<int> GetStopIds
        {
            get
            {
                List<int> result = new();
                foreach (Stop stop in stops) { result.Add(stop.Id); }
                return result;
            }
        }
        public List<int> GetVehicleIds
        {
            get
            {
                List<int> result = new();
                foreach (Vehicle vehicle in vehicles) { result.Add(vehicle.Id); }
                return result;
            }
        }
        public Line(string numberHex, int numberDec, string commonName, List<Stop> stops, List<Vehicle> vehicles)
        {
            NumberHex = numberHex;
            NumberDec = numberDec;
            CommonName = commonName;
            this.stops = stops;
            this.vehicles = vehicles;
        }
        public override string ToString() //<numerHex>(<numerDec>)`<commonName>`@<stop id>,...!<vehicle id>,...
        {
            StringBuilder text = new($"{NumberHex}({NumberDec})`{CommonName}`@");
            foreach (Stop stop in stops)
                text.Append($"{stop.Id},");
            text.Remove(text.Length - 1, 1);
            text.Append('!');
            foreach (Vehicle vehicle in vehicles)
                text.Append($"{vehicle.Id},");
            text.Remove(text.Length - 1, 1);
            return text.ToString();
        }
    }
    public class Stop : IStop
    {
        public List<Line> lines;
        public int Id { get; set; }
        public string Name { get; set; }
        public EType Type { get; set; }
        public List<int> GetLineIds
        {
            get
            {
                List<int> result = new();
                foreach (Line line in lines) { result.Add(line.NumberDec); }
                return result;
            }
        }
        public Stop(int id, List<Line> lines, string name, EType type)
        {
            Id = id;
            Name = name;
            Type = type;
            this.lines = lines;
        }
        public override string ToString() //#<id>(<line id>,...)<name>/<typ>
        {
            StringBuilder text = new($"#{Id}(");
            foreach(Line line in lines)
                text.Append($"{line.NumberDec},");
            text.Remove(text.Length - 1, 1);
            text.Append($"){Name}/{Type}");
            return text.ToString();
        }
    }
    public abstract class Vehicle : IVehicle
    {
        public Vehicle(int id) { Id = id; }
        public int Id { get; set; }
        public abstract List<int> GetLineIds { get; }
        public override abstract string ToString();
    }
    public class Bytebus : Vehicle, IVehicle
    {
        public List<Line> lines;
        public EEngineClass EngineClass { get; set; }
        public override List<int> GetLineIds
        {
            get
            {
                List<int> result = new();
                foreach (Line line in lines) { result.Add(line.NumberDec); }
                return result;
            }
        }
        public Bytebus(int id, List<Line> lines, EEngineClass engineClass) : base(id)
        {
            EngineClass = engineClass;
            this.lines = lines;
        }
        public override string ToString() //#<id>^<engineClass>*<line id>,...
        {
            StringBuilder text = new($"#{Id}^{EngineClass}*");
            foreach (Line line in lines)
                text.Append($"{line.NumberDec},");
            text.Remove(text.Length - 1, 1);
            return text.ToString();
        }
    }
    public class Tram : Vehicle, IVehicle
    {
        public Line line;
        public int CarsNumber { get; set; }
        public override List<int> GetLineIds { get => new() { line.NumberDec }; }
        public Tram(int id, int carsNumber, Line line) : base(id)
        {
            CarsNumber = carsNumber;
            this.line = line;
        }
        public override string ToString() //#<id>(<carsNumber>)<line id>,...
        {
            StringBuilder text = new($"#{Id}({CarsNumber}){line.NumberDec}");
            return text.ToString();
        }
    }
    public class Driver : IDriver
    {
        public List<Vehicle> vehicles;
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Seniority { get; set; }
        public List<int> GetVehicleIds
        {
            get
            {
                List<int> result = new();
                foreach (Vehicle vehicle in vehicles) { result.Add(vehicle.Id); }
                return result;
            }
        }
        public Driver(List<Vehicle> vehicles, string name, string surname, int seniority)
        {
            Name = name;
            Surname = surname;
            Seniority = seniority;
            this.vehicles = vehicles;       
        }
        public override string ToString() //<name> <surname>(<seniority>)@<vehicle id>,...
        {
            StringBuilder text = new($"{Name} {Surname}({Seniority})@");
            foreach (Vehicle vehicle in vehicles)
                text.Append($"{vehicle.Id},");
            text.Remove(text.Length - 1, 1);
            return text.ToString();
        }
    }

    public class BaseRep: IBaseRep
    {
        public Dictionary<int, Line> lines = new();
        public Dictionary<int, Stop> stops = new();
        public Dictionary<int, Vehicle> vehicles = new();
        public Dictionary<string, Driver> drivers = new();

        public BaseRep(List<Line> lines, List<Stop> stops, List<Vehicle> vehicles, List<Driver> drivers)
        {
            foreach (Line line in lines) this.lines.Add(line.NumberDec, new Line(line.NumberHex, line.NumberDec, line.CommonName, line.stops, line.vehicles));

            foreach (Stop stop in stops) this.stops.Add(stop.Id, new Stop(stop.Id, stop.lines, stop.Name, stop.Type));

            foreach (Vehicle vehicle in vehicles)
            {
                if (vehicle is Bytebus b) this.vehicles.Add(b.Id, new Bytebus(b.Id, b.lines, b.EngineClass));
                else if (vehicle is Tram t) this.vehicles.Add(t.Id, new Tram(t.Id, t.CarsNumber, t.line));
            }
            foreach (Driver driver in drivers) this.drivers.Add(driver.Name + "-" + driver.Surname, new Driver(driver.vehicles, driver.Name, driver.Surname, driver.Seniority));
        }
        public override string ToString()
        {
            StringBuilder text = new();
            text.Append("Lines:\n");
            foreach (Line line in lines.Values) text.Append(line.ToString() + "\n");
            text.Append("\nStops:\n");
            foreach (Stop stop in stops.Values) text.Append(stop.ToString() + "\n");
            text.Append("\nVehicles:\n");
            foreach (Vehicle vehicle in vehicles.Values) text.Append(vehicle.ToString() + "\n");
            text.Append("\nDrivers:\n");
            foreach (Driver driver in drivers.Values) text.Append(driver.ToString() + "\n");
            return text.ToString();
        }
    }
}
