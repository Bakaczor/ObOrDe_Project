using System.Text;
using System.Text.RegularExpressions;
using Base_Representation;


namespace File_Readers
{
    public struct LineStruct
    {
        public string numberHex;
        public int numberDec;
        public string commonName;
        public List<int> stopsIds;
        public List<int> vehiclesIds;
        public LineStruct(string numberHex, int numberDec, string commonName, List<int> stopsIds, List<int> vehiclesIds)
        {
            this.numberHex = numberHex;
            this.numberDec = numberDec;
            this.commonName = commonName;
            this.stopsIds = stopsIds;
            this.vehiclesIds = vehiclesIds;
        }
        public override string ToString() //<numerHex>(<numerDec>)`<commonName>`@<stop id>,...!<vehicle id>,...
        {
            StringBuilder text = new($"{numberHex}({numberDec})`{commonName}`@");
            foreach (int stopId in stopsIds)
                text.Append($"{stopId},");
            text.Remove(text.Length - 1, 1);
            text.Append('!');
            foreach (int vehicleId in vehiclesIds)
                text.Append($"{vehicleId},");
            text.Remove(text.Length - 1, 1);
            return text.ToString();
        }
    }
    public struct StopStruct
    {
        public int id;
        public string name;
        public EType type;
        public List<int> linesIds;
        public StopStruct(int id, List<int> linesIds, string name, EType type)
        {
            this.id = id;
            this.name = name;
            this.type = type;
            this.linesIds = linesIds;
        }
        public override string ToString() //#<id>(<line id>,...)<name>/<typ>
        {
            StringBuilder text = new($"#{id}(");
            foreach (int lineId in linesIds)
                text.Append($"{lineId},");
            text.Remove(text.Length - 1, 1);
            text.Append($"){name}/{type}");
            return text.ToString();
        }
    }
    public struct BytebusStruct
    {
        public int id;
        public EEngineClass engineClass;
        public List<int> linesIds;
        public BytebusStruct(int id, List<int> linesIds, EEngineClass engineClass)
        {
            this.id = id;
            this.engineClass = engineClass;
            this.linesIds = linesIds;
        }
        public override string ToString() //#<id>^<engineClass>*<line id>,...
        {
            StringBuilder text = new($"#{id}^{engineClass}*");
            foreach (int lineId in linesIds)
                text.Append($"{lineId},");
            text.Remove(text.Length - 1, 1);
            return text.ToString();
        }
    }
    public struct TramStruct
    {
        public int id;
        public int carsNumber;
        public int lineId;
        public TramStruct(int id, int carsNumber, int lineId)
        {
            this.id = id;
            this.carsNumber = carsNumber;
            this.lineId = lineId;
        }
        public override string ToString() //#<id>(<carsNumber>)<line id>,...
        {
            StringBuilder text = new($"#{id}({carsNumber}){lineId}");
            return text.ToString();
        }
    }
    public struct DriverStruct
    {
        public string name;
        public string surname;
        public int seniority;
        public List<int> vehiclesIds;
        public DriverStruct(List<int> vehiclesIds, string name, string surname, int seniority)
        {
            this.name = name;
            this.surname = surname;
            this.seniority = seniority;
            this.vehiclesIds = vehiclesIds;
        }
        public override string ToString() //<name> <surname>(<seniority>)@<vehicle id>,...
        {
            StringBuilder text = new($"{name} {surname}({seniority})@");
            foreach (int vehicleId in vehiclesIds)
                text.Append($"{vehicleId},");
            text.Remove(text.Length - 1, 1);
            return text.ToString();
        }
    }
    public class BTMFileReader
    {
        public List<LineStruct> lines = new();
        public List<StopStruct> stops = new();
        public List<BytebusStruct> bytebuses = new();
        public List<TramStruct> trams = new();
        public List<DriverStruct> drivers = new();

        public BTMFileReader(string path)
        {
            using (StreamReader reader = new(path))
            {
                string? fileline;

                while ((fileline = reader.ReadLine()) != null)
                {
                    if(fileline.Contains('-'))
                    {
                        string[] heads = fileline.Split(" - ");
                        switch(heads[0])
                        {
                            case "Line":
                                while (!string.IsNullOrWhiteSpace(fileline = reader.ReadLine()))
                                {
                                    Match match = Regex.Match(fileline, @"\d+\.\s+(\w+),\s+(\d+),\s+([\w\s-]+),\s+\[(\d+(,\s+\d+)*)\],\s+\[(\d+(,\s+\d+)*)\]");

                                    string numberHex = match.Groups[1].Value;
                                    int numberDec = int.Parse(match.Groups[2].Value);
                                    string commonName = match.Groups[3].Value;

                                    string[] stopsStrs = (match.Groups[4].Value).Split(", ");
                                    List<int> stopsIds = new();
                                    foreach (string stopId in stopsStrs)
                                        stopsIds.Add(int.Parse(stopId));

                                    string[] vehiclesStrs = (match.Groups[6].Value).Split(", ");
                                    List<int> vehiclesIds = new();
                                    foreach (string vehicleId in vehiclesStrs)
                                        vehiclesIds.Add(int.Parse(vehicleId));

                                    lines.Add(new LineStruct(numberHex, numberDec, commonName, stopsIds, vehiclesIds));
                                }
                                break;
                            case "Stop":
                                while (!string.IsNullOrWhiteSpace(fileline = reader.ReadLine()))
                                {
                                    Match match = Regex.Match(fileline, @"\d+\.\s+(\d+),\s+([\w\s-]+),\s+(\w+),\s+\[(\d+(,\s+\d+)*)\]");

                                    int id = int.Parse(match.Groups[1].Value);
                                    string name = match.Groups[2].Value;
                                    EType type = (EType)Enum.Parse(typeof(EType), match.Groups[3].Value);

                                    string[] linesStrs = (match.Groups[4].Value).Split(", ");
                                    List<int> linesIds = new();
                                    foreach (string lineId in linesStrs)
                                        linesIds.Add(int.Parse(lineId));

                                    stops.Add(new StopStruct(id, linesIds, name, type));
                                }
                                break;
                            case "Bytebus":
                                while (!string.IsNullOrWhiteSpace(fileline = reader.ReadLine()))
                                {
                                    Match match = Regex.Match(fileline, @"\d+\.\s+(\d+),\s+\[?(\d+(,\s+\d+)*)\]?,\s+(\w+)");

                                    int id = int.Parse(match.Groups[1].Value);
                                    EEngineClass engineClass = (EEngineClass)Enum.Parse(typeof(EEngineClass), match.Groups[4].Value);

                                    string[] linesStrs = (match.Groups[2].Value).Split(", ");
                                    List<int> linesIds = new();
                                    foreach (string lineId in linesStrs)
                                        linesIds.Add(int.Parse(lineId));

                                    bytebuses.Add(new BytebusStruct(id, linesIds, engineClass));
                                }
                                break;
                            case "Tram":
                                while (!string.IsNullOrWhiteSpace(fileline = reader.ReadLine()))
                                {
                                    Match match = Regex.Match(fileline, @"\d+\.\s+(\d+),\s+(\d+),\s+(\d+)");

                                    int id = int.Parse(match.Groups[1].Value);
                                    int carsNumber = int.Parse(match.Groups[2].Value);
                                    int lineId = int.Parse(match.Groups[3].Value);

                                    trams.Add(new TramStruct(id, carsNumber, lineId));
                                }
                                break;
                            case "Driver":
                                while (!string.IsNullOrWhiteSpace(fileline = reader.ReadLine()))
                                {
                                    Match match = Regex.Match(fileline, @"\d+\.\s+(\w+)\s+(\w+),\s+(\d+),\s+\[(\d+(,\s+\d+)*)\]");

                                    string name = match.Groups[1].Value;
                                    string surname = match.Groups[2].Value;
                                    int seniority = int.Parse(match.Groups[3].Value);

                                    string[] vehiclesStrs = (match.Groups[4].Value).Split(", ");
                                    List<int> vehiclesIds = new();
                                    foreach (string vehicleId in vehiclesStrs)
                                        vehiclesIds.Add(int.Parse(vehicleId));

                                    drivers.Add(new DriverStruct(vehiclesIds, name, surname, seniority));
                                }
                                break;
                        }
                    }
                }
            }
        }
    }
}
