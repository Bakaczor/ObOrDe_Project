using System.Text;
using Interfaces;

namespace Base_Representation
{
    public enum EType
    {
        bus, tram, other
    }
    public enum EEngineClass
    {
        Byte5, bisel20, gibgaz
    }
    public class Line : ILine
    {
        private string _numberHex;
        private int _numberDec;
        private string _commonName;

        public List<Stop> stops;
        public List<Vehicle> vehicles;

        public string NumberHex { get => _numberHex; set => _numberHex = value; }
        public int NumberDec { get => _numberDec; set => _numberDec = value; }
        public string CommonName { get => _commonName; set => _commonName = value; }
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
            _numberHex = numberHex;
            _numberDec = numberDec;
            _commonName = commonName;
            this.stops = stops;
            this.vehicles = vehicles;
        }
        public override string ToString() //<numerHex>(<numerDec>)`<commonName>`@<stop id>,...!<vehicle id>,...
        {
            StringBuilder text = new($"{_numberHex}({_numberDec})`{_commonName}`@");
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
        private int _id;
        private string _name;
        private EType _type;

        public List<Line> lines;
        public int Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public EType Type { get => _type; set => _type = value; }
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
            _id = id;
            _name = name;
            _type = type;
            this.lines = lines;
        }
        public override string ToString() //#<id>(<line id>,...)<name>/<typ>
        {
            StringBuilder text = new($"#{_id}(");
            foreach(Line line in lines)
                text.Append($"{line.NumberDec},");
            text.Remove(text.Length - 1, 1);
            text.Append($"){_name}/{_type}");
            return text.ToString();
        }
    }
    public abstract class Vehicle : IVehicle
    {
        protected int _id;
        public Vehicle(int id) { _id = id; }
        public int Id { get => _id; set => _id = value; }
        public abstract List<int> GetLineIds { get; }
        public override abstract string ToString();
    }
    public class Bytebus : Vehicle, IVehicle
    {
        private EEngineClass _engineClass;

        public List<Line> lines;
        public EEngineClass EngineClass { get => _engineClass; set => _engineClass = value; }
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
            _engineClass = engineClass;
            this.lines = lines;
        }
        public override string ToString() //#<id>^<engineClass>*<line id>,...
        {
            StringBuilder text = new($"#{_id}^{_engineClass}*");
            foreach (Line line in lines)
                text.Append($"{line.NumberDec},");
            text.Remove(text.Length - 1, 1);
            return text.ToString();
        }
    }
    public class Tram : Vehicle, IVehicle
    {
        private int _carsNumber;

        public Line line;
        public int CarsNumber { get => _carsNumber; set => _carsNumber = value; }
        public override List<int> GetLineIds { get => new List<int> { line.NumberDec }; }
        public Tram(int id, int carsNumber, Line line) : base(id)
        {
            _carsNumber = carsNumber;
            this.line = line;
        }
        public override string ToString() //#<id>(<carsNumber>)<line id>,...
        {
            StringBuilder text = new($"#{_id}({_carsNumber}){line.NumberDec}");
            return text.ToString();
        }
    }
    public class Driver : IDriver
    {
        private string _name;
        private string _surname;
        private int _seniority;

        public List<Vehicle> vehicles;
        public string Name { get => _name; set => _name = value; }
        public string Surname { get => _surname; set => _surname = value; }
        public int Seniority { get => _seniority; set => _seniority = value; }
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
            _name = name;
            _surname = surname;
            _seniority = seniority;
            this.vehicles = vehicles;       
        }
        public override string ToString() //<name> <surname>(<seniority>)@<vehicle id>,...
        {
            StringBuilder text = new($"{_name} {_surname}({_seniority})@");
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
