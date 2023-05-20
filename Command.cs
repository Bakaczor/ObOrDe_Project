using Interfaces;
using Algorithms;
using Builders;
using System.Text;
using System.Reflection;

namespace Command
{
    public interface ICommand
    {
        bool Qable { get; }
        void Execute();
        //string ToString();
    }
    public class ExitCommand : ICommand
    {
        public bool Qable => false;
        public void Execute()
        {
            Console.Write("Do zobaczenia następny razem użytkowniku ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("<3");
            Console.ForegroundColor = ConsoleColor.Gray;
            Environment.Exit(0);
        }
        public override string ToString()
        {
            return "exit";
        }
    }
    public class ListCommand : ICommand
    {
        public bool Qable => true;
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
        public override string ToString()
        {
            return $"list {_name}";
        }
    }
    public class FindCommand : ListCommand
    {
        protected (string, object, char)[] _parameters;
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
            _receiver.FindOrDelete(_name, _parameters , false);
        }
    }
    public class DeleteCommand : FindCommand
    {
        public DeleteCommand(string name, Receiver receiver, (string, string, char)[] parameters) : base(name, receiver, parameters) { }
        public override void Execute()
        {
            _receiver.FindOrDelete(_name, _parameters, true);
        }
    }
    public class EditCommand : FindCommand
    {
        private List<(string property, object value)> _settings;
        public bool IsDone { get; }
        public EditCommand(string name, Receiver receiver, (string, string, char)[] parameters) : base(name, receiver, parameters)
        {
            int count = _receiver.Count(_name, _parameters);
            if (count == 0) throw new ArgumentException("Nie udało się znaleźć obiektu, który spełniałby zadane kryteria.");
            if (count != 1) throw new ArgumentException("Znaleziono więcej niż jeden obiekt spełniający zadane kryteria.");
            IsDone = GetInput.Loop(_name, _receiver, out _settings);
        }
        public override void Execute()
        {
            _receiver.Edit(_name, _parameters, _settings);
        }
    }
    public class AddCommand : ICommand
    {
        public bool Qable => true;
        private Receiver _receiver;
        private string _name;
        private string _representation;
        private List<(string property, object value)> _parameters;
        public bool IsDone { get; }
        public AddCommand(string name, string representation, Receiver receiver)
        {
            _receiver = receiver;
            _name = name;
            _representation = representation;
            IsDone = GetInput.Loop(name, receiver, out _parameters);
        }
        public void Execute()
        {
            IBuilder builder = _receiver.GetBuilder(_name);
            foreach (var (property, value) in _parameters) builder.Add(property, value);
            _receiver.Add(builder.Build(_representation));
            builder.Reset();
        }
    }
    public static class GetInput
    {
        public static bool Loop(string name, Receiver receiver, out List<(string, object)> parameters)
        {
            StringBuilder sb = new("[Dostępne pola: ");
            List<(string, Type)> properties = receiver.GetProperties(name);
            foreach (var property in properties) sb.Append(property.Item1 + " (" + property.Item2.Name + "), ");
            sb.Remove(sb.Length - 2, 2);
            sb.Append(']');
            Console.WriteLine(sb.ToString());

            bool IsDone = true;
            parameters = new();
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
                if (paramValue != null) parameters.Add((argument[0], paramValue));
            }
            if (!IsDone) parameters.Clear();
            return IsDone;
        }
    }
    public abstract class QueueCommand : ICommand
    {
        public bool Qable => false;
        protected Queue<ICommand> _queue;
        public QueueCommand(Queue<ICommand> queue)
        {
            _queue = queue;
        }
        public abstract void Execute();
    }
    public class QueuePrint : QueueCommand
    {
        public QueuePrint(Queue<ICommand> queue) : base(queue) { }
        public override void Execute()
        {
            foreach (var q in _queue)
                Console.WriteLine(q.ToString());
        }
    }
    public class QueueCommit : QueueCommand
    {
        public QueueCommit(Queue<ICommand> queue) : base(queue) { }
        public override void Execute()
        {
            while(_queue.Count > 0)
                _queue.Dequeue().Execute();
        }
    }
    public class QueueDismiss : QueueCommand
    {
        public QueueDismiss(Queue<ICommand> queue) : base(queue) { }
        public override void Execute()
        {
            _queue.Clear();
        }
    }
    public class QueueExport : QueueCommand
    {
        private string _path;
        private bool _toXML;
        public QueueExport(Queue<ICommand> queue, string[] arguments) : base(queue)
        {
            _path = arguments[0];
            if (!File.Exists(_path)) throw new FileNotFoundException();
            _toXML = true;
            if(arguments.Length > 1 && arguments[1] == "plaintext") _toXML = false;
        }
        public override void Execute()
        {
            //EXPORT TO XML OR PLAINTEXT (requiers ToString() in commands)
            throw new NotImplementedException();
        }
    }
    public class QueueLoad : QueueCommand
    {
        private string _path;
        public QueueLoad(Queue<ICommand> queue, string path) : base(queue)
        {
            if (!File.Exists(path)) throw new FileNotFoundException();
            _path = path;
        }
        public override void Execute()
        {
            //LOAD FROM THE GIVEN FILE
            throw new NotImplementedException();
        }
    }
    public class CommandCreator
    {
        private readonly Dictionary<string, Func<string[], ICommand?>> _commands;
        private Receiver _receiver;
        public CommandCreator(Receiver receiver)
        {
            _commands = new Dictionary<string, Func<string[], ICommand?>>
            {
                { "exit", CreateExit },
                { "list", CreateList },
                { "add", CreateAdd }
            };
            _receiver = receiver;
        }
        public ICommand? CreateCommand(string name, string[] arguments)
        {
            if (_commands.ContainsKey(name)) return _commands[name](arguments);
            if (name == "find" || name == "delete" || name == "edit") return CreateFindOrDerived(name, arguments);
            throw new ArgumentException("Nie znaleziono podanej komendy.");
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
        private ICommand CreateFindOrDerived(string command, string[] arguments)
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
                return command switch
                {
                    "find" => new FindCommand(name, _receiver, parameters),
                    "delete" => new DeleteCommand(name, _receiver, parameters),
                    "edit" => new EditCommand(name, _receiver, parameters),
                    _ => throw new ArgumentException("Nie znaleziono podanej komendy.")
                };
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
            AddCommand command = new(name, representation, _receiver);
            if (command.IsDone) return command;
            else return null;
        }
    }
    public class Invoker
    {
        private readonly Dictionary<string, ICommand> _simpleCommands;
        private readonly CommandCreator _commandCreator;
        private Queue<ICommand> _queue;
        public Invoker(Receiver receiver)
        {
            _commandCreator = new CommandCreator(receiver);
            _queue = new Queue<ICommand>();
            _simpleCommands = new Dictionary<string, ICommand>
            {
                { "print", new QueuePrint(_queue) },
                { "commit", new QueueCommit(_queue) },
                { "dismiss", new QueueDismiss(_queue) }
            };
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
                if (frazes[0] == "queue" && frazes.Length > 1)
                {
                    if (!_simpleCommands.ContainsKey(frazes[1]))
                    {
                        try
                        {
                            if (frazes[1] == "export" && frazes.Length > 2) command = new QueueExport(_queue, frazes[2..]);
                            else if (frazes[1] == "load" && frazes.Length > 2) command = new QueueLoad(_queue, frazes[2]);
                            else Console.WriteLine("Nie znaleziono podanej komendy.");
                        }
                        catch (Exception ex) { Console.WriteLine(ex.Message); }
                    }
                    else command = _simpleCommands[frazes[1]];
                }
                else
                {
                    try
                    {
                        command = _commandCreator.CreateCommand(frazes[0], frazes[1..]);
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                }
                if(command != null)
                {
                    if(command.Qable) _queue.Enqueue(command);
                    else command.Execute();
                }
            }
        }
    }
    public class Receiver
    {
        private readonly Dictionary<string, Type> _types;
        private readonly Dictionary<string, List<(string, Type)>> _properties; //zastępowalne przez klasę PropertyInfo, oferującą informacje takie jak Name czy PropertyType
        private readonly Dictionary<string, IBuilder> _builders;
        private IMyCollection<object> _collection;
        public Receiver(IMyCollection<object> collection, Dictionary<string, Type> types, Dictionary<string, List<(string, Type)>> properties, Dictionary<string, IBuilder> builders)
        {
            _types = types;
            _collection = collection;
            _properties = properties;
            _builders = builders;
        }
        public bool Contains(string name) { return _types.ContainsKey(name); }
        public List<(string, Type)> GetProperties(string name) { return _properties[name]; }
        public IBuilder GetBuilder(string name) { return _builders[name]; }
        public void List(string name)
        {
            if (_collection.Count == 0)
            {
                Console.WriteLine("Kolekcja jest pusta.");
                return;
            }
            Type type = _types[name];
            MyCollectionAlgorithms.ForEach(_collection.GetForwardBegin, (object x) => { if (type.IsInstanceOfType(x)) Console.WriteLine(x.ToString()); });
            Console.WriteLine();
        }
        public void FindOrDelete(string name, (string, object, char)[] parameters, bool delete)
        {
            if (_collection.Count == 0)
            {
                if(!delete) Console.WriteLine("Kolekcja jest pusta.");
                return;
            }
            Type type = _types[name];
            var properties = new List<PropertyInfo>(type.GetProperties());

            IMyCollection<object>? new_collection = null;
            if (delete) new_collection = (IMyCollection<object>)Activator.CreateInstance(_collection.GetType())!;
            MyCollectionAlgorithms.ForEach(_collection.GetForwardBegin, (object x) =>
            {
                if (type.IsInstanceOfType(x))
                {
                    bool flag = true;
                    foreach ((string propertyName, object value, char op) in parameters)
                    {
                        var property = properties.Find((PropertyInfo x) => { return x.Name == propertyName; })!;
                        if (!Tools.Compare(property.GetValue(x), value, op))
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag && !delete) Console.WriteLine(x.ToString());
                    if (!flag && delete) new_collection!.Add(x);
                }
                else if (delete) new_collection!.Add(x);
            });
            if (delete) _collection = new_collection!;
            else Console.WriteLine();
        }
        public void Add(object item)
        {
            _collection.Add(item);
        }
        public int Count(string name, (string, object, char)[] parameters)
        {
            if (_collection.Count == 0) return 0;
            Type type = _types[name];
            var properties = new List<PropertyInfo>(type.GetProperties());
            return MyCollectionAlgorithms.CountIf(_collection.GetForwardBegin, (object x) => Tools.VerifyObject(x, type, parameters, properties));
        }
        public void Edit(string name, (string, object, char)[] parameters, List<(string property, object value)> settings)
        {
            Type type = _types[name];
            var properties = new List<PropertyInfo>(type.GetProperties());
            object? item = MyCollectionAlgorithms.Find(_collection.GetForwardBegin, (object x) => Tools.VerifyObject(x, type, parameters, properties));
            if (item == null) return;

            foreach ((string propertyName, object value) in settings)
            {
                var property = properties.Find((PropertyInfo x) => { return x.Name == propertyName; })!;
                property.SetValue(item, value);
            }
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
        public static bool VerifyObject(object x, Type type, (string, object, char)[] parameters, List<PropertyInfo> properties)
        {
            if (type.IsInstanceOfType(x))
            {
                bool flag = true;
                foreach ((string propertyName, object value, char op) in parameters)
                {
                    var property = properties.Find((PropertyInfo x) => { return x.Name == propertyName; })!;
                    if (!Compare(property.GetValue(x), value, op))
                    {
                        flag = false;
                        break;
                    }
                }
                return flag;
            }
            return false;
        }
    }
}
