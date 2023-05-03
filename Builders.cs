using Base_Representation;
using TupleStack_Representation;
using Adapters;

namespace Builders
{
    interface IBuilder
    {
        void Reset();
        void Add(string property, object value);
        object Build(string representation);
    }
    public class LineBuilder : IBuilder
    {
        private string _numberHex;
        private int _numberDec;
        private string _commonName;
        private List<Stop> _stops;
        private List<Vehicle> _vehicles;

        public LineBuilder()
        {
            Reset();
        }
        public void Reset()
        {
            _numberHex = string.Empty;
            _numberDec = 0;
            _commonName = string.Empty;
            _stops = new List<Stop>();
            _vehicles = new List<Vehicle>();
        }
        public void Add(string property, object value)
        {
            switch(property)
            {
                case "NumberHex":
                    _numberHex = (string)value;
                    break;
                case "NumberDec":
                    _numberDec = (int)value;
                    break;
                case "CommonName":
                    _commonName = (string)value;
                    break;
                default:
                    throw new ArgumentException("Próba dodania nieoczekiwanego pola.");
            }
        }
        public object Build(string representation)
        {
            if (representation == "base") return new Line(_numberHex, _numberDec, _commonName, _stops, _vehicles);
            else return new LineTS2B(new TSLine(_numberHex, _numberDec, _commonName, _stops, _vehicles));
        }
    }
    public class StopBuilder : IBuilder
    {
        private int _id;
        private string _name;
        private EType _type;
        private List<Line> _lines;

        public StopBuilder()
        {
            Reset();
        }
        public void Reset()
        {
            _id = 0;
            _name = string.Empty;
            _type = 0;
            _lines = new List<Line>();
        }
        public void Add(string property, object value)
        {
            switch (property)
            {
                case "Id":
                    _id = (int)value;
                    break;
                case "Name":
                    _name = (string)value;
                    break;
                case "Type":
                    _type = (EType)value;
                    break;
                default:
                    throw new ArgumentException("Próba dodania nieoczekiwanego pola.");
            }
        }
        public object Build(string representation)
        {
            if (representation == "base") return new Stop(_id, _lines, _name, _type);
            else return new StopTS2B(new TSStop(_id, _lines, _name, _type));
        }
    }
    public class VehicleBuilder : IBuilder
    {
        private int _id;

        //Bytebus
        private EEngineClass _engineClass;
        private List<Line> _lines;

        //Tram
        private int _carsNumber;
        private Line _line;

        public VehicleBuilder()
        {
            Reset();
        }
        public void Reset()
        {
            _id = 0;
            _engineClass = 0;
            _lines = new List<Line>();
            _carsNumber = 0;
            _line = new Line(string.Empty, 0, string.Empty, new List<Stop>(), new List<Vehicle>());
        }
        public void Add(string property, object value)
        {
            if (property == "Id") _id = (int)value;
            else throw new ArgumentException("Próba dodania nieoczekiwanego pola.");
        }
        public object Build(string representation)
        {
            //it is irrelevant for this task which one will be returned
            Random random = new();
            if (representation == "base")
            {
                if (random.Next(100) % 2 == 0) return new Bytebus(_id, _lines, _engineClass);
                else return new Tram(_id, _carsNumber, _line);
            }
            else
            {
                if (random.Next(100) % 2 == 0) return new VehicleTS2B(new TSBytebus(_id, _lines, _engineClass));
                else return new VehicleTS2B(new TSTram(_id, _carsNumber, _line));
            }
        }
    }
    public class DriverBuilder : IBuilder
    {
        private string _name;
        private string _surname;
        private int _seniority;
        private List<Vehicle> _vehicles;

        public DriverBuilder()
        {
            Reset();
        }
        public void Reset()
        {
            _name = string.Empty;
            _surname = string.Empty;
            _seniority = 0;
            _vehicles = new List<Vehicle>();
        }
        public void Add(string property, object value)
        {
            switch (property)
            {
                case "Name":
                    _name = (string)value;
                    break;
                case "Surname":
                    _surname = (string)value;
                    break;
                case "Seniority":
                    _seniority = (int)value;
                    break;
                default:
                    throw new ArgumentException("Próba dodania nieoczekiwanego pola.");
            }
        }
        public object Build(string representation)
        {
            if (representation == "base") return new Driver(_vehicles, _name, _surname, _seniority);
            else return new DriverTS2B(new TSDriver(_vehicles, _name, _surname, _seniority));
        }
    }
}
