using Hashmap_Representation;
using Base_Representation;
using TupleStack_Representation;
using System.Text;
using Interfaces;

namespace Adapters
{
    public class AdapterHM2B : IBaseRep
    {
        private readonly HashmapRep _hashmaprep;
        public AdapterHM2B(HashmapRep hashmaprep)
        {
            _hashmaprep = hashmaprep;
        }
        public override string ToString() { return _hashmaprep.ToString(); }
    }

    public class LineTS2B : ILine
    {
        private readonly TSLine _line;
        public LineTS2B(TSLine line) { _line = line; }
        public string NumberHex
        {
            get
            {
                Stack<string> ST = new(_line.LineTuple.Item2.Reverse());

                int num = int.Parse(ST.Pop());
                for (int i = 0; i < num; i++) ST.Pop();

                num = int.Parse(ST.Pop());
                for (int i = 0; i < num; i++) ST.Pop();

                ST.Pop(); ST.Pop(); ST.Pop();
                return ST.Pop();
            }
            set
            {
                Stack<string> TmpST = new(_line.LineTuple.Item2);
                TmpST.Pop();
                TmpST.Push(value);
                _line.LineTuple = new(NumberDec, new(TmpST));
            }
        }
        public int NumberDec
        { 
            get => _line.LineTuple.Item1;
            set
            {
                Stack<string> TmpST = new(_line.LineTuple.Item2);
                string NHex = TmpST.Pop();
                TmpST.Pop();
                TmpST.Push(value.ToString());
                TmpST.Push(NHex);
                _line.LineTuple = new(NumberDec, new(TmpST));
            }
        }
        public string CommonName
        {
            get
            {
                Stack<string> ST = new(_line.LineTuple.Item2.Reverse());

                int num = int.Parse(ST.Pop());
                for (int i = 0; i < num; i++) ST.Pop();

                num = int.Parse(ST.Pop());
                for (int i = 0; i < num; i++) ST.Pop();

                return ST.Pop();
            }
            set
            {
                Stack<string> TmpST = new(_line.LineTuple.Item2);
                string NHex = TmpST.Pop();
                string NDec = TmpST.Pop();
                string N = TmpST.Pop();
                TmpST.Pop();
                TmpST.Push(value);
                TmpST.Push(N);
                TmpST.Push(NDec);
                TmpST.Push(NHex);
                _line.LineTuple = new(NumberDec, new(TmpST));
            }
        }
        public List<int> GetStopIds
        {
            get
            {
                List<int> result = new();
                Stack<string> ST = new(_line.LineTuple.Item2.Reverse());

                int num = int.Parse(ST.Pop());
                for (int i = 0; i < num; i++) ST.Pop();

                num = int.Parse(ST.Pop());
                for (int i = 0; i < num; i++) result.Add(int.Parse(ST.Pop()));
                return result;
            }
        }
        public List<int> GetVehicleIds
        {
            get
            {
                List<int> result = new();
                Stack<string> ST = new(_line.LineTuple.Item2.Reverse());

                int num = int.Parse(ST.Pop());
                for (int i = 0; i < num; i++) result.Add(int.Parse(ST.Pop()));
                return result;
            }
        }
        public override string ToString() //<numerHex>(<numerDec>)`<commonName>`@<stop id>,...!<vehicle id>,...
        {
            StringBuilder text = new($"{NumberHex}({NumberDec})`{CommonName}`@");
            Stack<string> ST = new(_line.LineTuple.Item2.Reverse());

            Stack<string> VehicleST = new();
            Stack<string> StopST = new();

            int num = int.Parse(ST.Pop());
            for (int i = 0; i < num; i++) VehicleST.Push(ST.Pop());

            num = int.Parse(ST.Pop());
            for (int i = 0; i < num; i++) StopST.Push(ST.Pop());

            while (StopST.Count > 0) text.Append($"{StopST.Pop()},");
            text.Remove(text.Length - 1, 1);

            text.Append('!');

            while (VehicleST.Count > 0) text.Append($"{VehicleST.Pop()},");
            text.Remove(text.Length - 1, 1);

            return text.ToString();
        }
    }

    public class StopTS2B : IStop
    {
        private readonly TSStop _stop;
        public StopTS2B(TSStop stop) { _stop = stop; }
        public int Id
        { 
            get => _stop.StopTuple.Item1;
            set
            {
                Stack<string> TmpST = new(_stop.StopTuple.Item2);
                TmpST.Pop();
                TmpST.Push(value.ToString());
                _stop.StopTuple = new(Id, new(TmpST));
            }
        }
        public string Name
        {
            get
            {
                Stack<string> ST = new(_stop.StopTuple.Item2.Reverse());

                int num = int.Parse(ST.Pop());
                for (int i = 0; i < num; i++) ST.Pop();
                ST.Pop();
                return ST.Pop();
            }
            set
            {
                Stack<string> TmpST = new(_stop.StopTuple.Item2);
                string id = TmpST.Pop();
                string N = TmpST.Pop();
                TmpST.Pop();
                TmpST.Push(value);
                TmpST.Push(N);
                TmpST.Push(id);
                _stop.StopTuple = new(Id, new(TmpST));
            }
        }
        public EType Type
        {
            get
            {
                Stack<string> ST = new(_stop.StopTuple.Item2.Reverse());

                int num = int.Parse(ST.Pop());
                for (int i = 0; i < num; i++) ST.Pop();
                return (EType)Enum.Parse(typeof(EType), ST.Pop());
            }
            set
            {
                Stack<string> TmpST = new(_stop.StopTuple.Item2);
                string id = TmpST.Pop();
                string N = TmpST.Pop();
                string name = TmpST.Pop();
                TmpST.Pop();
                TmpST.Push(value.ToString());
                TmpST.Push(name);
                TmpST.Push(N);
                TmpST.Push(id);
                _stop.StopTuple = new(Id, new(TmpST));
            }
        }
        public List<int> GetLineIds
        {
            get
            {
                List<int> result = new();
                Stack<string> ST = new(_stop.StopTuple.Item2.Reverse());

                int num = int.Parse(ST.Pop());
                for (int i = 0; i < num; i++) result.Add(int.Parse(ST.Pop()));
                return result;
            }        
        }
        public override string ToString() //#<id>(<line id>,...)<name>/<typ>
        {
            StringBuilder text = new($"#{Id}(");
            Stack<string> ST = new(_stop.StopTuple.Item2.Reverse());

            Stack<string> LineST = new();

            int num = int.Parse(ST.Pop());
            for (int i = 0; i < num; i++) LineST.Push(ST.Pop());

            while (LineST.Count > 0) text.Append($"{LineST.Pop()},");
            text.Remove(text.Length - 1, 1);

            text.Append($"){Name}/{Type}");
            return text.ToString();
        }
    }

    public class VehicleTS2B : IVehicle
    {
        private readonly TSVehicle _vehicle;
        public VehicleTS2B(TSVehicle vehicle) { _vehicle = vehicle; }
        public int Id
        { 
            get => _vehicle.VehicleTuple.Item1;
            set
            {
                Stack<string> TmpST = new(_vehicle.VehicleTuple.Item2);
                TmpST.Pop();
                TmpST.Push(value.ToString());
                _vehicle.VehicleTuple = new(Id, new(TmpST));
            }
        }
        public List<int> GetLineIds
        {
            get
            {
                List<int> result = new();
                Stack<string> ST = new(_vehicle.VehicleTuple.Item2.Reverse());

                int num = int.Parse(ST.Pop());
                for (int i = 0; i < num; i++) result.Add(int.Parse(ST.Pop()));
                return result;
            }
        }
        public override string ToString() //#<id>^<engineClass>*<line id>,...
        {
            StringBuilder text = new();
            Stack<string> ST = new(_vehicle.VehicleTuple.Item2.Reverse());

            if (_vehicle is TSBytebus)
            {
                Stack<string> LineST = new();
                int num = int.Parse(ST.Pop());
                for (int i = 0; i < num; i++) LineST.Push(ST.Pop());

                string engineClass = ST.Pop();
                text.Append($"#{Id}^{engineClass}*");
                while (LineST.Count > 0) text.Append($"{LineST.Pop()},");
                text.Remove(text.Length - 1, 1);
            }
            if(_vehicle is TSTram)
            {
                ST.Pop();
                string line = ST.Pop();
                string carsNumber = ST.Pop();
                text.Append($"#{Id}({carsNumber}){line}");
            }
            return text.ToString();
        }
    }
    public class DriverTS2B : IDriver
    {
        private readonly TSDriver _driver;
        public DriverTS2B(TSDriver driver) { _driver = driver; }
        public string Name
        {
            get
            {
                Stack<string> ST = new(_driver.DriverTuple.Item2.Reverse());
                int num = int.Parse(ST.Pop());
                for (int i = 0; i < num; i++) { ST.Pop(); }
                ST.Pop(); ST.Pop();
                return ST.Pop();
            }
            set
            {
                Stack<string> TmpST = new(_driver.DriverTuple.Item2);
                string id = TmpST.Pop();
                string N = TmpST.Pop();
                TmpST.Pop();
                TmpST.Push(value);
                TmpST.Push(N);
                TmpST.Push(id);
                _driver.DriverTuple = new((Name + "-" + Surname).GetHashCode(), new(TmpST));
            }

        }
        public string Surname
        {
            get
            {
                Stack<string> ST = new(_driver.DriverTuple.Item2.Reverse());
                int num = int.Parse(ST.Pop());
                for (int i = 0; i < num; i++) { ST.Pop(); }
                ST.Pop();
                return ST.Pop();
            }
            set
            {
                Stack<string> TmpST = new(_driver.DriverTuple.Item2);
                string id = TmpST.Pop();
                string N = TmpST.Pop();
                string name = TmpST.Pop();
                TmpST.Pop();
                TmpST.Push(value);
                TmpST.Push(name);
                TmpST.Push(N);
                TmpST.Push(id);
                _driver.DriverTuple = new((Name + "-" + Surname).GetHashCode(), new(TmpST));
            }
        }
        public int Seniority
        {
            get
            {
                Stack<string> ST = new(_driver.DriverTuple.Item2.Reverse());
                int num = int.Parse(ST.Pop());
                for (int i = 0; i < num; i++) { ST.Pop(); }
                return int.Parse(ST.Pop());
            }
            set
            {
                Stack<string> TmpST = new(_driver.DriverTuple.Item2);
                string id = TmpST.Pop();
                string N = TmpST.Pop();
                string name = TmpST.Pop();
                string surname = TmpST.Pop();
                TmpST.Pop();
                TmpST.Push(value.ToString());
                TmpST.Push(surname);
                TmpST.Push(name);
                TmpST.Push(N);
                TmpST.Push(id);
                _driver.DriverTuple = new((Name + "-" + Surname).GetHashCode(), new(TmpST));
            }
        }
        public List<int> GetVehicleIds
        {
            get
            {
                List<int> result = new();
                Stack<string> ST = new(_driver.DriverTuple.Item2.Reverse());

                int num = int.Parse(ST.Pop());
                for (int i = 0; i < num; i++) result.Add(int.Parse(ST.Pop()));
                return result;
            }
        }
        public override string ToString() //<name> <surname>(<seniority>)@<vehicle id>,...
        {
            StringBuilder text = new($"{Name} {Surname}({Seniority})@");
            Stack<string> ST = new(_driver.DriverTuple.Item2.Reverse());

            Stack<string> VehicleST = new();

            int num = int.Parse(ST.Pop());
            for (int i = 0; i < num; i++) VehicleST.Push(ST.Pop());

            while (VehicleST.Count > 0) text.Append($"{VehicleST.Pop()},");
            text.Remove(text.Length - 1, 1);

            return text.ToString();
        }
    }

    /*
    public class ConverterTS2B
    {
        private TSRep _tsrep;

        public Dictionary<int, Line> lines = new();
        public Dictionary<int, Stop> stops = new();
        public Dictionary<int, Vehicle> vehicles = new();
        public Dictionary<string, Driver> drivers = new();

        public ConverterTS2B(TSRep tsrep)
        {
            _tsrep = tsrep;

            List<(int, int)> line_stop = new();
            List<(int, int)> line_vehicles = new();

            foreach (TSLine l in _tsrep.lines.Values)
            {
                lines.Add(l.NumberDec, new(l.NumberHex, l.NumberDec, l.CommonName, new List<Stop>(), new List<Vehicle>()));
                Stack<string> ST = new(l.LineTuple.Item2.Reverse());

                int num = int.Parse(ST.Pop());
                for (int i = 0; i < num; i++)
                {
                    string vehicle = ST.Pop();
                    line_vehicles.Add((l.NumberDec, int.Parse(vehicle)));
                }

                num = int.Parse(ST.Pop());
                for (int i = 0; i < num; i++)
                {
                    string stop = ST.Pop();
                    line_stop.Add((l.NumberDec, int.Parse(stop)));
                }
            }

            foreach (TSStop s in _tsrep.stops.Values)
            {
                stops.Add(s.Id, new(s.Id, new List<Line>(), s.Name, s.Type));
            }

            foreach ((int lkey, int skey) in line_stop)
            {
                lines[lkey].stops.Add(stops[skey]);
                stops[skey].lines.Add(lines[lkey]);
            }

            foreach (TSVehicle v in _tsrep.vehicles.Values)
            {
                if (v is TSBytebus b) vehicles.Add(b.Id, new Bytebus(b.Id, new List<Line>(), b.EngineClass));
                else if (v is TSTram t)
                {
                    Stack<string> ST = new(t.VehicleTuple.Item2.Reverse()); ST.Pop();
                    vehicles.Add(t.Id, new Tram(t.Id, t.CarsNumber, lines[int.Parse(ST.Pop())] ));
                }
            }

            foreach ((int lkey, int vkey) in line_vehicles)
            {
                lines[lkey].vehicles.Add(vehicles[vkey]);
                if (vehicles[vkey] is Bytebus b) b.lines.Add(lines[lkey]);
            }

            foreach (TSDriver d in _tsrep.drivers.Values)
            {
                List<Vehicle> V = new();
                Stack<string> ST = new(d.DriverTuple.Item2.Reverse());

                int num = int.Parse(ST.Pop());
                for (int i = 0; i < num; i++)
                {
                    string v = ST.Pop();
                    V.Add(vehicles[int.Parse(v)]);
                }
                drivers.Add(d.Name + "-" + d.Surname, new(V, d.Name, d.Surname, d.Seniority));
            }
        }
        public override string ToString() { return _tsrep.ToString(); }
    }

    public class ConverterHM2B
    {
        private HashmapRep _hashmaprep;

        public Dictionary<int, Line> lines = new();
        public Dictionary<int, Stop> stops = new();
        public Dictionary<int, Vehicle> vehicles = new();
        public Dictionary<string, Driver> drivers = new();

        public ConverterHM2B(HashmapRep hashmaprep)
        {
            _hashmaprep = hashmaprep;

            List<(int, int)> line_stop = new();
            List<(int, int)> line_vehicles = new();

            foreach (HashLine l in _hashmaprep.Lines.Values)
            {
                lines.Add(l.NumberDec, new(l.NumberHex, l.NumberDec, l.CommonName, new List<Stop>(), new List<Vehicle>()));
                foreach (int stopId in l.stops) line_stop.Add((l.NumberDec, stopId));
                foreach (int vehicleId in l.vehicles) line_vehicles.Add((l.NumberDec, vehicleId));
            }

            foreach (HashStop s in _hashmaprep.Stops.Values)
            {
                stops.Add(s.Id, new(s.Id, new List<Line>(), s.Name, s.Type));
            }

            foreach ((int lkey, int skey) in line_stop)
            {
                lines[lkey].stops.Add(stops[skey]);
                stops[skey].lines.Add(lines[lkey]);
            }

            foreach (HashVehicle v in _hashmaprep.Vehicles.Values)
            {
                if (v is HashBytebus b) vehicles.Add(b.Id, new Bytebus(b.Id, new List<Line>(), b.EngineClass));
                else if (v is HashTram t) vehicles.Add(t.Id, new Tram(t.Id, t.CarsNumber, lines[t.line]));
            }

            foreach ((int lkey, int vkey) in line_vehicles)
            {
                lines[lkey].vehicles.Add(vehicles[vkey]);
                if (vehicles[vkey] is Bytebus b) b.lines.Add(lines[lkey]);
            }

            foreach (HashDriver d in _hashmaprep.Drivers.Values)
            {
                List<Vehicle> V = new();
                foreach (int v in d.vehicles) V.Add(vehicles[v]);
                drivers.Add(d.Name + "-" + d.Surname, new(V, d.Name, d.Surname, d.Seniority));
            }
        }
        public override string ToString() { return _hashmaprep.ToString(); }
    }
    */
}
