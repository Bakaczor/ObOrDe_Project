using Base_Representation;
using System.Text;

namespace Hashmap_Representation
{
    public class HashLine
    {
        private static Dictionary<long, string> LineHashMap = new();

        private readonly long _numberHex;
        private readonly long _numberDec;
        private readonly long _commonName;

        public List<int> stops;
        public List<int> vehicles;

        public string NumberHex
        {
            get => LineHashMap[_numberHex];
            set => LineHashMap[_numberHex] = value;
        }
        public int NumberDec
        {
            get => int.Parse(LineHashMap[_numberDec]);
            set => LineHashMap[_numberDec] = value.ToString();
        }
        public string CommonName
        {
            get => LineHashMap[_commonName];
            set => LineHashMap[_commonName] = value;
        }

        public HashLine(string numberHex, int numberDec, string commonName, List<Stop> stops, List<Vehicle> vehicles)
        {
            _numberHex = numberHex.GetHashCode(); LineHashMap.Add(_numberHex, numberHex);
            _numberDec = numberDec.ToString().GetHashCode(); LineHashMap.Add(_numberDec, numberDec.ToString());
            _commonName = commonName.GetHashCode(); LineHashMap.Add(_commonName, commonName);

            this.stops = new List<int>();
            foreach (Stop stop in stops)
                this.stops.Add(stop.Id);

            this.vehicles = new List<int>();
            foreach (Vehicle vehicle in vehicles)
                this.vehicles.Add(vehicle.Id);
        }
        public override string ToString() //<numerHex>(<numerDec>)`<commonName>`@<stop id>,...!<vehicle id>,...
        {
            StringBuilder text = new($"{LineHashMap[_numberHex]}({LineHashMap[_numberDec]})`{LineHashMap[_commonName]}`@");
            foreach (int stopId in stops)
                text.Append($"{stopId},");
            text.Remove(text.Length - 1, 1);
            text.Append('!');
            foreach (int vehicleId in vehicles)
                text.Append($"{vehicleId},");
            text.Remove(text.Length - 1, 1);
            return text.ToString();
        }
    }

    public class HashStop
    {
        private static Dictionary<long, string> StopHashMap = new();

        private readonly long _id;
        private readonly long _name;
        private readonly long _type;

        public List<int> lines;

        public int Id
        {
            get => int.Parse(StopHashMap[_id]);
            set => StopHashMap[_id] = value.ToString();
        }
        public string Name
        {
            get => StopHashMap[_name];
            set => StopHashMap[_name] = value;
        }
        public EType Type
        {
            get => (EType)Enum.Parse(typeof(EType), StopHashMap[_type]);
            set => StopHashMap[_type] = value.ToString();
        }

        public HashStop(int id, List<Line> lines, string name, EType type)
        {
            _id = id.ToString().GetHashCode(); StopHashMap.Add(_id, id.ToString());
            _name = name.GetHashCode(); StopHashMap.Add(_name, name);
            _type = type.ToString().GetHashCode();
            if (!StopHashMap.ContainsKey(_type)) StopHashMap.Add(_type, type.ToString());

            this.lines = new List<int>();
            foreach (Line line in lines)
                this.lines.Add(line.NumberDec);
        }
        public override string ToString() //#<id>(<line id>,...)<name>/<typ>
        {
            StringBuilder text = new($"#{StopHashMap[_id]}(");
            foreach (int lineId in lines)
                text.Append($"{lineId},");
            text.Remove(text.Length - 1, 1);
            text.Append($"){StopHashMap[_name]}/{StopHashMap[_type]}");
            return text.ToString();
        }
    }
    public abstract class HashVehicle
    {
        protected static Dictionary<long, string> VehicleHashMap = new();

        protected readonly long _id;
        public int Id
        {
            get => int.Parse(VehicleHashMap[_id]);
            set => VehicleHashMap[_id] = value.ToString();
        }
        public HashVehicle(int id)
        {
            _id = id.ToString().GetHashCode(); VehicleHashMap.Add(_id, id.ToString());
        }
        public override abstract string ToString();
    }
    public class HashBytebus : HashVehicle
    {
        private readonly long _engineClass;

        public List<int> lines;

        public EEngineClass EngineClass
        {
            get => (EEngineClass)Enum.Parse(typeof(EEngineClass), VehicleHashMap[_engineClass]);
            set => VehicleHashMap[_engineClass] = value.ToString();
        }
      
        public HashBytebus(int id, List<Line> lines, EEngineClass engineClass) : base(id)
        {
            _engineClass = engineClass.ToString().GetHashCode();
            if (!VehicleHashMap.ContainsKey(_engineClass)) VehicleHashMap.Add(_engineClass, engineClass.ToString());

            this.lines = new List<int>();
            foreach (Line line in lines)
                this.lines.Add(line.NumberDec);
        }
        public override string ToString() //#<id>^<engineClass>*<line id>,...
        {
            StringBuilder text = new($"#{VehicleHashMap[_id]}^{VehicleHashMap[_engineClass]}*");
            foreach (int lineId in lines)
                text.Append($"{lineId},");
            text.Remove(text.Length - 1, 1);
            return text.ToString();
        }
    }
    public class HashTram : HashVehicle
    {
        private readonly long _carsNumber;

        public int line;
        public int CarsNumber
        {
            get => int.Parse(VehicleHashMap[_carsNumber]);
            set => VehicleHashMap[_carsNumber] = value.ToString();
        }
        public HashTram(int id, int carsNumber, Line line) : base(id)
        {
            _carsNumber = carsNumber.ToString().GetHashCode(); 
            if (!VehicleHashMap.ContainsKey(_carsNumber)) VehicleHashMap.Add(_carsNumber, carsNumber.ToString()); //jeżeli liczba wagonów może być zmienna, trzeba zastosować inną logikę
            this.line = line.NumberDec;
        }
        public override string ToString() //#<id>(<carsNumber>)<line id>,...
        {
            StringBuilder text = new($"#{VehicleHashMap[_id]}({VehicleHashMap[_carsNumber]})");
            text.Append($"{line}");
            return text.ToString();
        }
    }
    public class HashDriver
    {
        private static Dictionary<long, string> DriverHashMap = new();

        private readonly long _name;
        private readonly long _surname;
        private readonly long _seniority;

        public List<int> vehicles;
        public string Name
        {
            get => DriverHashMap[_name];
            set => DriverHashMap[_name] = value;
        }
        public string Surname
        {
            get => DriverHashMap[_surname];
            set => DriverHashMap[_surname] = value;
        }
        public int Seniority
        {
            get => int.Parse(DriverHashMap[_seniority]);
            set => DriverHashMap[_seniority] = value.ToString();
        }
        public HashDriver(List<Vehicle> vehicles, string name, string surname, int seniority)
        {
            _name = name.GetHashCode(); 
            if (!DriverHashMap.ContainsKey(_name)) DriverHashMap.Add(_name, name);

            _surname = surname.GetHashCode();
            if (!DriverHashMap.ContainsKey(_surname)) DriverHashMap.Add(_surname, surname);

            _seniority = seniority.ToString().GetHashCode();
            if (!DriverHashMap.ContainsKey(_seniority)) DriverHashMap.Add(_seniority, seniority.ToString());

            this.vehicles = new List<int>();
            foreach (Vehicle vehicle in vehicles)
                this.vehicles.Add(vehicle.Id);
        }
        public override string ToString() //<name> <surname>(<seniority>)@<vehicle id>,...
        {
            StringBuilder text = new($"{DriverHashMap[_name]} {DriverHashMap[_surname]}({DriverHashMap[_seniority]})@");
            foreach (int vehicleId in vehicles)
                text.Append($"{vehicleId},");
            text.Remove(text.Length - 1, 1);
            return text.ToString();
        }
    }
    public class HashmapRep
    {
        public Dictionary<int, HashLine> lines = new();
        public Dictionary<int, HashStop> stops = new();
        public Dictionary<int, HashVehicle> vehicles = new();
        public Dictionary<string, HashDriver> drivers = new();

        public HashmapRep(List<Line> lines, List<Stop> stops, List<Vehicle> vehicles, List<Driver> drivers)
        {
            foreach (Line line in lines) this.lines.Add(line.NumberDec, new HashLine(line.NumberHex, line.NumberDec, line.CommonName, line.stops, line.vehicles));

            foreach (Stop stop in stops) this.stops.Add(stop.Id, new HashStop(stop.Id, stop.lines, stop.Name, stop.Type));

            foreach (Vehicle vehicle in vehicles)
            {
                if (vehicle is Bytebus b) this.vehicles.Add(b.Id, new HashBytebus(b.Id, b.lines, b.EngineClass));
                else if (vehicle is Tram t) this.vehicles.Add(t.Id, new HashTram(t.Id, t.CarsNumber, t.line));
            }
            foreach (Driver driver in drivers) this.drivers.Add(driver.Name + "-" + driver.Surname, new HashDriver(driver.vehicles, driver.Name, driver.Surname, driver.Seniority));

        }
        public override string ToString()
        {
            StringBuilder text = new();
            text.Append("Lines:\n");
            foreach (HashLine line in lines.Values) text.Append(line.ToString() + "\n");
            text.Append("\nStops:\n");
            foreach (HashStop stop in stops.Values) text.Append(stop.ToString() + "\n");
            text.Append("\nVehicles:\n");
            foreach (HashVehicle vehicle in vehicles.Values) text.Append(vehicle.ToString() + "\n");
            text.Append("\nDrivers:\n");
            foreach (HashDriver driver in drivers.Values) text.Append(driver.ToString() + "\n");
            return text.ToString();
        }
    }
}
