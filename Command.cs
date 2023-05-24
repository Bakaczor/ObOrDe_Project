using Interfaces;
using Algorithms;
using Builders;
using System.Text;
using System.Reflection;
using System.Xml.Serialization;

namespace Command
{
    public interface ICommand
    {
        bool Qable { get; }
        void Execute();
        void SetReceiver(Receiver receiver) { } //default
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
            return "exit\n";
        }
    }
    [Serializable]
    [XmlType("ListCommand")]
    public class ListCommand : ICommand
    {
        [XmlIgnore]
        public bool Qable => true;

        [XmlAttribute]
        public string name;

        protected Receiver _receiver;
        public ListCommand() { }
        public ListCommand(string name, Receiver receiver)
        {
            this.name = name;
            _receiver = receiver;
        }
        public virtual void SetReceiver(Receiver receiver) { _receiver = receiver; }
        public virtual void Execute()
        {
            _receiver.List(name);
        }
        public override string ToString()
        {
            return $"list {name}\n";
        }
    }
    [Serializable]
    [XmlType("FindCommand")]
    public class FindCommand : ListCommand
    {
        [XmlArray]
        [XmlArrayItem("parameter", typeof((string, object, char)))]
        public (string property, object value, char sign)[] parameters;

        public FindCommand() { }
        public FindCommand(string name, Receiver receiver, (string, string, char)[] parameters) : base(name, receiver)
        {
            List<(string, Type)> properties = receiver.GetProperties(name);

            this.parameters = new (string, object, char)[parameters.Length];
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
                    this.parameters[i] = (property.Item1, paramValue, parameters[i].Item3);
                }
            }
        }
        public override void Execute()
        {
            _receiver.FindOrDelete(name, parameters , false);
        }
        public override string ToString()
        {
            var sb = new StringBuilder($"find {name}");
            foreach(var (property, value, sign) in parameters)
                sb.Append(" " + property + sign.ToString() + value.ToString());
            sb.Append('\n');
            return sb.ToString();
        }
    }
    [Serializable]
    [XmlType("DeleteCommand")]
    public class DeleteCommand : FindCommand
    {
        public DeleteCommand() { }
        public DeleteCommand(string name, Receiver receiver, (string, string, char)[] parameters) : base(name, receiver, parameters) { }
        public override void Execute()
        {
            _receiver.FindOrDelete(name, parameters, true);
        }
        public override string ToString()
        {
            var sb = new StringBuilder($"delete {name}");
            foreach (var (property, value, sign) in parameters)
                sb.Append(" " + property + sign.ToString() + value.ToString());
            sb.Append('\n');
            return sb.ToString();
        }
    }
    [Serializable]
    [XmlType("EditCommand")]
    public class EditCommand : FindCommand
    {
        [XmlArray]
        [XmlArrayItem("setting", typeof((string, object)))]
        public List<(string property, object value)> settings;

        [XmlIgnore]
        public bool IsDone { get; }

        public EditCommand() { IsDone = true; }
        public EditCommand(string name, Receiver receiver, (string, string, char)[] parameters) : base(name, receiver, parameters)
        {
            int count = _receiver.Count(base.name, base.parameters);
            if (count == 0) throw new ArgumentException("Nie udało się znaleźć obiektu, który spełniałby zadane kryteria.");
            if (count != 1) throw new ArgumentException("Znaleziono więcej niż jeden obiekt spełniający zadane kryteria.");
            IsDone = GetInput.Loop(base.name, _receiver, out settings);
        }
        public EditCommand(string name, Receiver receiver, (string, string, char)[] parameters, List<string> lines) : base(name, receiver, parameters)
        {
            IsDone = GetInput.Read(receiver.GetProperties(name), in lines, out settings);
        }
        public override void Execute()
        {
            _receiver.Edit(name, parameters, settings);
        }
        public override string ToString()
        {
            var sb = new StringBuilder($"edit {name}");
            foreach (var (property, value, sign) in parameters)
                sb.Append(" " + property + sign.ToString() + value.ToString());
            sb.Append('\n');
            foreach (var (property, value) in settings)
                sb.Append("> " + property + "=" + value.ToString() + "\n");
            return sb.ToString();
        }
    }
    [Serializable]
    [XmlType("AddCommand")]
    public class AddCommand : ICommand
    {
        [XmlIgnore]
        public bool Qable => true;

        [XmlAttribute]
        public string name;

        [XmlAttribute]
        public string representation;

        [XmlArray]
        [XmlArrayItem("setting", typeof((string, object)))]
        public List<(string property, object value)> settings;

        [XmlIgnore]
        public bool IsDone { get; }

        private Receiver _receiver;
        public AddCommand() { IsDone = true; }
        public AddCommand(string name, string representation, Receiver receiver)
        {
            this.name = name;
            this.representation = representation;
            _receiver = receiver;
            IsDone = GetInput.Loop(this.name, _receiver, out settings);
        }
        public AddCommand(string name, string representation, Receiver receiver, List<string> lines)
        {
            this.name = name;
            this.representation = representation;
            _receiver = receiver;
            IsDone = GetInput.Read(receiver.GetProperties(name), in lines, out settings);
        }
        public void SetReceiver(Receiver receiver) { _receiver = receiver; }
        public void Execute()
        {
            IBuilder builder = _receiver.GetBuilder(name);
            foreach (var (property, value) in settings) builder.Add(property, value);
            _receiver.Add(builder.Build(representation));
            builder.Reset();
        }
        public override string ToString()
        {
            var sb = new StringBuilder($"add {name} {representation}\n");
            foreach (var (property, value) in settings)
                sb.Append("> " + property + "=" + value.ToString() + "\n");
            return sb.ToString();
        }
    }
    public static class GetInput
    {
        public static bool Loop(string name, Receiver receiver, out List<(string, object)> settings)
        {
            StringBuilder sb = new("[Dostępne pola: ");
            List<(string, Type)> properties = receiver.GetProperties(name);
            foreach (var property in properties) sb.Append(property.Item1 + " (" + property.Item2.Name + "), ");
            sb.Remove(sb.Length - 2, 2);
            sb.Append(']');
            Console.WriteLine(sb.ToString());

            bool IsDone = true;
            settings = new();
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
                (string property, Type type) property = properties.Find(((string, Type) x) => { return x.Item1 == argument[0]; });
                if (property == default)
                {
                    Console.WriteLine($"Podana klasa nie zawiera pola '{argument[0]}'.");
                    continue;
                }
                argument[1] = argument[1].Replace("\"", "");
                object? paramValue = null;
                try
                {
                    if (property.type.IsEnum) paramValue = Enum.Parse(property.type, argument[1]);
                    else paramValue = Convert.ChangeType(argument[1], property.type);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Wartość pola '{argument[0]}' jest nieprawidłowa: {ex.Message}");
                    continue;
                }
                if (paramValue != null) settings.Add((argument[0], paramValue));
            }
            if (!IsDone) settings.Clear();
            return IsDone;
        }
        public static bool Read(List<(string property, Type type)> properties, in List<string> lines, out List<(string, object)> settings)
        {
            settings = new();
            foreach(string line in lines)
            {
                string[] argument = line.Split("=");
                (string property, Type type) property = properties.Find(((string, Type) x) => { return x.Item1 == argument[0]; });
                argument[1] = argument[1].Replace("\"", null);
                object? paramValue;
                if (property.type.IsEnum) paramValue = Enum.Parse(property.type, argument[1]);
                else paramValue = Convert.ChangeType(argument[1], property.type);
                if (paramValue != null) settings.Add((argument[0], paramValue));
            }
            return true;
        }
        public static (string, string, char)[] PrepareParameters(string[] arguments)
        {
            (string parameter, string value, char op)[] parameters = new (string, string, char)[arguments.Length - 1];
            for (int i = 1; i < arguments.Length; i++)
            {
                char op = Tools.CheckOperator(arguments[i]);
                if (op == '\0') throw new ArgumentException("Błędny format argumentu - brak dopuszczalnego operatora porównania.");
                string[] argument = arguments[i].Split(op);
                if (argument.Length != 2) throw new ArgumentException("Błędny format argumentu - liczba operatorów porównania powinna wynosić jeden.");

                var sb = new StringBuilder();
                sb.Append(char.ToUpper(argument[0][0]));
                sb.Append(argument[0][1..]);
                parameters[i - 1] = (sb.ToString(), argument[1], op);
            }
            return parameters;
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
            _toXML = true;
            if(arguments.Length > 1 && arguments[1] == "plaintext") _toXML = false;
        }
        public override void Execute()
        {
            try
            {
                using FileStream fileStream = new(_path, FileMode.Create, FileAccess.Write);
                if (_toXML)
                {
                    var commands = new List<object>(_queue);
                    XmlSerializer serializer = new(typeof(List<object>), Invoker.GetCommandTypes());
                    serializer.Serialize(fileStream, commands);
                }
                else
                {
                    using StreamWriter writer = new(fileStream);
                    foreach (ICommand command in _queue) writer.WriteLine(command.ToString());
                }
                Console.WriteLine($"Komendy zostały zapisane do pliku: {_path}.");
            }
            catch (Exception ex) { Console.WriteLine("Nastąpił błąd związany z wprowadzoną ścieżką: " + ex.ToString()); }
        }
    }
    public class QueueLoad : QueueCommand
    {
        private string _path;
        private bool _fromXML;
        private readonly CommandCreator _commandCreator;
        private readonly Receiver _receiver;
        public QueueLoad(Queue<ICommand> queue, string path, CommandCreator commandCreator, Receiver receiver) : base(queue)
        {
            if (!File.Exists(path)) throw new FileNotFoundException();
            _path = path;
            _fromXML = Path.GetExtension(path).Equals(".xml", StringComparison.OrdinalIgnoreCase);
            _commandCreator = commandCreator;
            _receiver = receiver;
        }
        public override void Execute()
        {
            try
            {
                using FileStream fileStream = new(_path, FileMode.Open, FileAccess.Read);
                if (_fromXML)
                {
                    XmlSerializer serializer = new(typeof(List<object>), Invoker.GetCommandTypes());
                    var commands = (List<object>?)serializer.Deserialize(fileStream) ?? throw new Exception("Deserializacja komend zakończyła się niepowodzeniem.");

                    foreach (var command in commands)
                    {
                        if(command is ICommand com)
                        {
                            com.SetReceiver(_receiver);
                            _queue.Enqueue(com);
                        }
                    }
                }
                else
                {
                    using StreamReader reader = new(fileStream);
                    string? input;
                    while ((input = reader.ReadLine()) != null)
                    {
                        string[] frazes = Tools.SpaceSplit(input);
                        if (frazes.Length == 0) continue;
                        ICommand? command;
                        if (frazes[0] == "list" || frazes[0] == "find" || frazes[0] == "delete")
                            command = _commandCreator.CreateCommand(frazes[0], frazes[1..]);
                        else
                        {
                            string name = frazes[1];
                            List<string> lines = new();

                            string? line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                string[] setting = Tools.SpaceSplit(line);
                                if (setting.Length == 0 || setting[0] != ">") break;
                                else lines.Add(setting[1]);
                            }
                            if (frazes[0] == "edit")
                            {
                                var parameters = GetInput.PrepareParameters(frazes[1..]);
                                command = new EditCommand(name, _receiver, parameters, lines);
                            }
                            else
                            {
                                string representation = frazes[2];
                                command = new AddCommand(name, representation, _receiver, lines);
                            }
                        }
                        if (command != null) _queue.Enqueue(command);
                    }
                }
                Console.WriteLine($"Komendy zostały wczytane z pliku: {_path}.");
            }
            catch (Exception ex) { Console.WriteLine("Nastąpił błąd związany z wprowadzoną ścieżką: " + ex.ToString()); }
        }
    }
    public class CommandCreator
    {
        private readonly Dictionary<string, Func<string[], ICommand?>> _commands;
        private readonly Receiver _receiver;
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
            try
            {
                var parameters = GetInput.PrepareParameters(arguments);
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
        private readonly Dictionary<string, ICommand> _qCommands;
        private readonly CommandCreator _commandCreator;
        private readonly Receiver _receiver;
        private Queue<ICommand> _queue;
        public Invoker(Receiver receiver)
        {
            _receiver = receiver;
            _commandCreator = new CommandCreator(receiver);
            _queue = new Queue<ICommand>();
            _qCommands = new Dictionary<string, ICommand>
            {
                { "print", new QueuePrint(_queue) },
                { "commit", new QueueCommit(_queue) },
                { "dismiss", new QueueDismiss(_queue) }
            };
        }
        public static Type[] GetCommandTypes()
        {
            Type[] types = { typeof(ListCommand), typeof(FindCommand), typeof(DeleteCommand), typeof(EditCommand), typeof(AddCommand) };
            return types;
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
                    if (!_qCommands.ContainsKey(frazes[1]))
                    {
                        try
                        {
                            if (frazes[1] == "export" && frazes.Length > 2) command = new QueueExport(_queue, frazes[2..]);
                            else if (frazes[1] == "load" && frazes.Length > 2) command = new QueueLoad(_queue, frazes[2], _commandCreator, _receiver);
                            else Console.WriteLine("Nie znaleziono podanej komendy.");
                        }
                        catch (Exception ex) { Console.WriteLine(ex.Message); }
                    }
                    else command = _qCommands[frazes[1]];
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
