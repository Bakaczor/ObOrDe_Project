using Interfaces;
using Algorithms;
using Builders;
using System.Text;
using System.Reflection;

namespace Command
{
    public interface ICommand
    {
        static bool Qable { get; }
        void Execute();
    }
    public class ExitCommand : ICommand
    {
        public static bool Qable => false;
        public void Execute()
        {
            Console.Write("Do zobaczenia następny razem użytkowniku ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("<3");
            Console.ForegroundColor = ConsoleColor.Gray;
            Environment.Exit(0);
        }
    }
    public class ListCommand : ICommand
    {
        public static bool Qable => false;
        protected readonly string _name;
        protected Receiver _receiver;
        public ListCommand(string name, Receiver receiver)
        {
            _name = name;
            _receiver = receiver;
        }
        public virtual void Execute()
        {
            _receiver.List(_name);
        }
    }
    public class FindCommand : ListCommand
    {
        private (string, object, char)[] _parameters;
        public FindCommand(string name, Receiver receiver, (string, string, char)[] parameters) : base(name, receiver)
        {
            List<(string, Type)> properties = receiver.GetProperties(name);

            _parameters = new (string, object, char)[parameters.Length];
            for(int i = 0; i < parameters.Length; i++)
            {
                (string, Type) property = properties.Find(((string, Type) x) => { return x.Item1 == parameters[i].Item1; });
                if (property == default) throw new ArgumentException($"Podana klasa nie zawiera pola '{parameters[i].Item1}'.");

                object? paramValue = null;
                try
                {
                    if (property.Item2.IsEnum) paramValue = Enum.Parse(property.Item2, parameters[i].Item2);
                    else paramValue = Convert.ChangeType(parameters[i].Item2, property.Item2);
                }
                catch (Exception ex)
                { 
                    throw new ArgumentException($"Wartość pola '{parameters[i].Item1}' jest nieprawidłowa: {ex.Message}"); 
                }

                if (paramValue != null)
                {
                    if (parameters[i].Item3 != '=' && !Tools.IsString(paramValue) && !Tools.IsNumeric(paramValue))
                        throw new ArgumentException($"Operator '{parameters[i].Item3}' nie jest wspierany dla typu {property.Item2}");
                    _parameters[i] = (property.Item1, paramValue, parameters[i].Item3);
                }
            }
        }
        public override void Execute()
        {
            _receiver.Find(_name, _parameters);
        }
    }
    public class AddCommand : ICommand
    {
        public static bool Qable => false;
        private Receiver _receiver;
        private Dictionary<string, IBuilder> _builders;
        private string _name;
        private string _representation;
        private List<(string property, object value)> _parameters;
        public bool IsDone { get; }
        public AddCommand(string name, string representation, Receiver receiver, Dictionary<string, IBuilder> builders)
        {
            _receiver = receiver;
            _builders = builders;
            _name = name;
            _representation = representation;
            _parameters = new();

            StringBuilder sb = new("[Dostępne pola: ");
            List<(string, Type)> properties = _receiver.GetProperties(_name);
            foreach (var property in properties) sb.Append(property.Item1 + " (" + property.Item2.Name + "), ");
            sb.Remove(sb.Length - 2, 2);
            sb.Append(']');
            Console.WriteLine(sb.ToString());

            IsDone = true;
            while (true)
            {
                Console.Write($"> ");
                string? input = Console.ReadLine();
                if (input == "DONE") break;
                if (input == null || input == "EXIT")
                {
                    IsDone = false;
                    break;
                }
                string[] argument = input.Split("=");
                if (argument.Length != 2)
                {
                    Console.WriteLine("Błędny format argumentu - liczba znaków '=' powinna wynosić jeden.");
                    continue;
                }
                (string, Type) property = properties.Find(((string, Type) x) => { return x.Item1 == argument[0]; });
                if (property == default)
                {
                    Console.WriteLine($"Podana klasa nie zawiera pola '{argument[0]}'.");
                    continue;
                }
                argument[1] = argument[1].Replace("\"", "");
                object? paramValue = null;
                try
                {
                    if (property.Item2.IsEnum) paramValue = Enum.Parse(property.Item2, argument[1]);
                    else paramValue = Convert.ChangeType(argument[1], property.Item2);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Wartość pola '{argument[0]}' jest nieprawidłowa: {ex.Message}");
                    continue;
                }
                if (paramValue != null) _parameters.Add((argument[0], paramValue));
            }
            if (!IsDone) _parameters.Clear();
        }
        public void Execute()
        {
            IBuilder builder = _builders[_name];
            foreach (var (property, value) in _parameters) builder.Add(property, value);
            _receiver.Add(builder.Build(_representation));
            builder.Reset();
        }
    }
    public class CommandCreator
    {
        private readonly Dictionary<string, Func<string[], ICommand?>> _commands;
        private readonly Dictionary<string, IBuilder> _builders;
        private Receiver _receiver;
        public CommandCreator(Receiver receiver)
        {
            _commands = new Dictionary<string, Func<string[], ICommand?>>
            {
                { "exit", CreateExit },
                { "list", CreateList },
                { "find", CreateFind },
                { "add", CreateAdd },
            };
            _builders = new Dictionary<string, IBuilder>
            {
                { "line", new LineBuilder() },
                { "stop", new StopBuilder() },
                { "vehicle", new VehicleBuilder() },
                { "driver", new DriverBuilder() }
            };
            _receiver = receiver;
        }
        public ICommand? CreateCommand(string name, string[] arguments)
        {
            if (!_commands.ContainsKey(name)) throw new ArgumentException("Nie znaleziono podanej komendy.");
            else return _commands[name](arguments);
        }
        private ICommand CreateExit(string[] arguments)
        {
            return new ExitCommand();
        }
        private ICommand CreateList(string[] arguments)
        {
            if (arguments.Length < 1) throw new ArgumentException("Za mało argumentów! Oczekiwano nazwy klasy.");
            string name = arguments[0];
            if (!_receiver.Contains(name)) throw new ArgumentException("Podana klasa nie jest obsługiwana.");
            return new ListCommand(name, _receiver);
        }
        private ICommand CreateFind(string[] arguments)
        {
            if (arguments.Length < 1) throw new ArgumentException("Za mało argumentów! Oczekiwano przynajmniej nazwy klasy.");
            string name = arguments[0];
            if (!_receiver.Contains(name)) throw new ArgumentException("Podana klasa nie jest obsługiwana.");

            (string parameter, string value, char op)[] parameters = new (string, string, char)[arguments.Length - 1];
            for (int i = 1; i < arguments.Length; i++)
            {
                char op = Tools.CheckOperator(arguments[i]);
                if (op == '\0') throw new ArgumentException("Błędny format argumentu - brak dopuszczalnego operatora porównania.");
                string[] argument = arguments[i].Split(op);
                if (argument.Length != 2) throw new ArgumentException("Błędny format argumentu - liczba operatorów porównania powinna wynosić jeden.");

                StringBuilder sb = new();
                sb.Append(char.ToUpper(argument[0][0]));
                sb.Append(argument[0][1..]);
                parameters[i - 1] = (sb.ToString(), argument[1], op);
            }
            try
            {
                return new FindCommand(name, _receiver, parameters);
            }
            catch (Exception) { throw; }
        }
        private ICommand? CreateAdd(string[] arguments)
        {
            if (arguments.Length < 2) throw new ArgumentException("Za mało argumentów! Oczekiwano nazwy klasy i jej reprezentacji.");
            string name = arguments[0];
            string representation = arguments[1];
            if (!_receiver.Contains(name)) throw new ArgumentException("Podana klasa nie jest obsługiwana.");
            if (representation != "base" && representation != "secondary") throw new ArgumentException("Podana reprezentacja nie jest obsługiwana.");
            AddCommand command = new(name, representation, _receiver, _builders);
            if (command.IsDone) return command;
            else return null;
        }
    }
    public class Invoker
    {
        private readonly CommandCreator _commandCreator;
        private Queue<ICommand> _queue;
        public Invoker(Receiver receiver)
        {
            _commandCreator = new CommandCreator(receiver);
            _queue = new Queue<ICommand>();
        }
        private void Add(ICommand command)
        {
            _queue.Enqueue(command);
        }
        private ICommand? GetNext()
        {
            if(_queue.Count == 0) return null;
            return _queue.Dequeue();
        }
        public void Run()
        {
            Console.WriteLine("Witaj drogi użytowniku, w czym mogę służyć?");
            while (true)
            {
                Console.Write($"{Tools.Username}");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("@");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write($"{Tools.CurDir}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(" $ ");
                Console.ForegroundColor = ConsoleColor.Gray;
                string? input = Console.ReadLine();
                if (input == null) return;

                string[] frazes = Tools.SpaceSplit(input);
                ICommand? command = null;
                try
                {
                    command = _commandCreator.CreateCommand(frazes[0], frazes[1..]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                if(command != null)
                {
                    if(ICommand.Qable) Add(command);
                    else command.Execute();
                }
            }
        }
    }
    public class Receiver
    {
        private readonly Dictionary<string, Type> _types;
        private readonly Dictionary<string, List<(string, Type)>> _properties; //zastępowalne przez klasę PropertyInfo, oferującą informacje takie jak Name czy PropertyType
        private IMyCollection<object> _collection;
        public Receiver(IMyCollection<object> collection, Dictionary<string, Type> types, Dictionary<string, List<(string, Type)>> properties)
        {
            _types = types;
            _collection = collection;
            _properties = properties;
        }
        public bool Contains(string name) { return _types.ContainsKey(name); }
        public List<(string, Type)> GetProperties(string name) { return _properties[name]; }
        public void List(string name)
        {
            if (_collection.Count == 0)
            {
                Console.WriteLine("Kolekcja jest pusta.\n");
                return;
            }
            Type type = _types[name];
            MyCollectionAlgorithms.ForEach(_collection.GetForwardBegin, (object x) => { if (type.IsInstanceOfType(x)) Console.WriteLine(x.ToString()); });
            Console.WriteLine();
        }
        public void Find(string name, (string, object, char)[] parameters)
        {
            if (_collection.Count == 0)
            {
                Console.WriteLine("Kolekcja jest pusta.\n");
                return;
            }
            Type type = _types[name];
            List<PropertyInfo> properties = new(type.GetProperties());

            MyCollectionAlgorithms.ForEach(_collection.GetForwardBegin, (object x) =>
            {
                if (type.IsInstanceOfType(x))
                {
                    bool flag = true;
                    foreach ((string propertyName, object value, char op) in parameters)
                    {
                        PropertyInfo property = properties.Find((PropertyInfo x) => { return x.Name == propertyName; })!;
                        if (!Tools.Compare(property.GetValue(x), value, op))
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag) Console.WriteLine(x.ToString());
                }
            });
            Console.WriteLine();
        }
        public void Add(object item)
        {
            _collection.Add(item);
        }
    }
    public static class Tools
    {
        public static string CurDir { get; }
        public static string Username { get; }
        static Tools()
        {
            Username = Environment.UserName;
            string[] dirs = Directory.GetCurrentDirectory().Split("\\");
            CurDir = dirs[^4] + "\\" + dirs[^3] + "\\" + dirs[^2] + "\\" + dirs[^1];
        }
        public static bool Compare(object? val1, object val2, char op)
        {
            if (val1 == null)
            {
                if (IsNumeric(val2)) val1 = 0;
                if (IsString(val2)) val1 = string.Empty;
            }
            switch (op)
            {
                case '=':
                    return Equals(val1, val2);
                case '>':
                    if (IsNumeric(val1))
                    {
                        double res = Convert.ToDouble(val1!) - Convert.ToDouble(val2);
                        if (Math.Abs(res) < double.Epsilon) return false;
                        return res > 0;
                    }
                    if (IsString(val1)) return string.Compare((string)val1!, (string)val2) > 0;
                    throw new ArgumentException($"Operator '{op}' jest wspierany tylko dla typów tekstowych i numerycznych");
                case '<':
                    if (IsNumeric(val1))
                    {
                        double res = Convert.ToDouble(val1!) - Convert.ToDouble(val2);
                        if (Math.Abs(res) < double.Epsilon) return false;
                        return res < 0;
                    }
                    if (IsString(val1)) return string.Compare((string)val1!, (string)val2) < 0;
                    throw new ArgumentException($"Operator '{op}' jest wspierany tylko dla typów tekstowych i numerycznych");
                default:
                    return false;
            }
        }
        public static bool IsString(object? value)
        {
            if (value == null) return false;
            return value is string;
        }
        public static bool IsNumeric(object? value)
        {
            if(value == null) return false;
            return value is
                byte or sbyte or ushort or uint or ulong or short or int or long or decimal or float or double;
        }
        public static char CheckOperator(string value)
        {
            if (value.Contains('=')) return '=';
            else if (value.Contains('>')) return '>';
            else if (value.Contains('<')) return '<';
            else return '\0';
        }
        public static string[] SpaceSplit(string input)
        {
            bool inQuotes = false;
            List<string> frazes = new();
            int start = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '\"')
                {
                    inQuotes = !inQuotes;
                }
                else if (!inQuotes && input[i] == ' ')
                {
                    string fraze = input[start..i];
                    if (!string.IsNullOrWhiteSpace(fraze)) frazes.Add(fraze.Replace("\"", null));

                    start = i + 1;
                }
            }
            if (start < input.Length) frazes.Add(input[start..].Replace("\"", null));
            return frazes.ToArray();
        }
    }
}
