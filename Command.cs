using Interfaces;
using Algorithms;
using Builders;
using System.Text;
using System.Reflection;

namespace Command
{
    public interface ICommand
    {
        void Execute();
    }
    public class ExitCommand : ICommand
    {
        public void Execute()
        {
            Console.WriteLine("Do zobaczenia następny razem użytkowniku <3");
            Environment.Exit(0);
        }
    }
    public class ListCommand : ICommand
    {
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
        (string, object)[] _parameters;
        public FindCommand(string name, Receiver receiver, (string, string)[] parameters) : base(name, receiver)
        {
            List<(string, Type)> properties = receiver.GetProperties(name);

            _parameters = new (string, object)[parameters.Length];
            for(int i = 0; i < parameters.Length; i++)
            {
                (string, Type) property = properties.Find(((string, Type) x) => { return x.Item1 == parameters[i].Item1; });
                if (property == default) throw new ArgumentException($"Podana klasa nie zawiera pola '{parameters[i].Item1}'.");

                object? paramValue = null;
                try
                {
                    paramValue = Convert.ChangeType(parameters[i].Item2, property.Item2);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Wartość pola '{parameters[i].Item1}' jest nieprawidłowa: {ex.Message}");
                }

                if (paramValue != null) _parameters[i] = (property.Item1, paramValue);
            }
        }
        public override void Execute()
        {
            _receiver.Find(_name, _parameters);
        }
    }
    public class AddCommand : ICommand
    {
        private Receiver _receiver;
        private Dictionary<string, IBuilder> _builders;
        private string _name;
        private string _representation;
        public AddCommand(string name, string representation, Receiver receiver)
        {
            _receiver = receiver;
            _builders = new Dictionary<string, IBuilder>
            {
                { "line", new LineBuilder() },
                { "stop", new StopBuilder() },
                { "vehicle", new VehicleBuilder() },
                { "driver", new DriverBuilder() }
            };
            _name = name;
            _representation = representation;
        }
        public void Execute()
        {
            IBuilder builder = _builders[_name];
            StringBuilder sb = new("[Dostępne pola: ");
            List<(string, Type)> properties = _receiver.GetProperties(_name);
            foreach (var property in properties) sb.Append(property.Item1 + " (" + property.Item2.Name + "), ");
            sb.Remove(sb.Length - 2, 2);
            sb.Append(']');
            Console.WriteLine(sb.ToString());
            builder.Reset();
            bool flag = true;
            while (true)
            {
                Console.Write($"> ");
                string? input = Console.ReadLine();
                if (input == "DONE") break;
                if (input == null || input == "EXIT")
                {
                    flag = false;
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
                    paramValue = Convert.ChangeType(argument[1], property.Item2);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Wartość pola '{argument[0]}' jest nieprawidłowa: {ex.Message}");
                    continue;
                }
                if (paramValue != null) builder.Add(argument[0], paramValue);
            }
            if (flag) _receiver.Add(builder.Build(_representation));
            else builder.Reset();
        }
    }
    public class CommandCreator
    {
        private readonly Dictionary<string, Func<string[], ICommand>> _commands;
        private Receiver _receiver;
        public CommandCreator(Receiver receiver)
        {
            _commands = new Dictionary<string, Func<string[], ICommand>>
            {
                { "exit", CreateExit },
                { "list", CreateList },
                { "find", CreateFind },
                { "add", CreateAdd },
            };
            _receiver = receiver;
        }
        public ICommand CreateCommand(string name, string[] arguments)
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
            (string parameter, string value)[] parameters = new (string, string)[arguments.Length - 1];
            for (int i = 1; i < arguments.Length; i++)
            {
                string[] argument = arguments[i].Split("=");
                if (argument.Length != 2) throw new ArgumentException("Błędny format argumentu - liczba znaków '=' powinna wynosić jeden.");

                StringBuilder sb = new();
                sb.Append(char.ToUpper(argument[0][0]));
                sb.Append(argument[0][1..]);
                parameters[i - 1] = (sb.ToString(), argument[1]);
            }
            try
            {
                return new FindCommand(name, _receiver, parameters);
            }
            catch (Exception) { throw; }
        }
        private ICommand CreateAdd(string[] arguments)
        {
            if (arguments.Length < 2) throw new ArgumentException("Za mało argumentów! Oczekiwano nazwy klasy i jej reprezentacji.");
            string name = arguments[0];
            string representation = arguments[1];
            if (!_receiver.Contains(name)) throw new ArgumentException("Podana klasa nie jest obsługiwana.");
            if (representation != "base" && representation != "secondary") throw new ArgumentException("Podana reprezentacja nie jest obsługiwana.");
            return new AddCommand(name, representation, _receiver);
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
        private void Execute()
        {
            ICommand command = _queue.Dequeue();
            command.Execute();
        }
        public void Run()
        {
            Console.WriteLine("Witaj drogi użytowniku, w czym mogę służyć?");
            while (true)
            {
                Console.Write($"{Tools.Domain}> ");
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
                    Add(command);
                    Execute();
                }
            }
        }
    }
    public class Receiver
    {
        private readonly Dictionary<string, Type> _types;
        private readonly Dictionary<string, List<(string, Type)>> _properties;
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
        public void Find(string name, (string, object)[] parameters)
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
                    foreach ((string propertyName, object value) in parameters)
                    {
                        PropertyInfo property = properties.Find((PropertyInfo x) => { return x.Name == propertyName; })!;
                        if (!Equals(property.GetValue(x), value))
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
        private static readonly string _domain = AppDomain.CurrentDomain.BaseDirectory[..^1];
        public static string Domain { get { return _domain; } }
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
