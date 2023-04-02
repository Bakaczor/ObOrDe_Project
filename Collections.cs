using Interfaces;
using Iterators;

namespace Collections
{
    public class MyVector<T> : IMyCollection<T>
    {
        private T[]? _tab;
        private int _length;

        public MyVector()
        {
            _tab = null;
            _length = 0;
        }
        public MyVector(int length)
        {
            _tab = new T[length];
            _length = length;
        }
        public MyVector(T[] tab)
        {
            _tab = new T[tab.Length];
            _length = tab.Length;
            tab.CopyTo(_tab, 0);
        }
        public int Count => _length;
        public T GetValue(int index)
        {
            if (index < 0 || index >= _length) throw new ArgumentOutOfRangeException($"Current index ({index}) is out of range");
            if (_tab == null) throw new NullReferenceException("The inner array was not initialized");
            return _tab[index];
        }
        public void Add(T value)
        {
            T[] array = new T[++_length];
            if (_tab != null) _tab.CopyTo(array, 0);
            array[^1] = value;
            _tab = array;
        }
        public bool Delete(T value)
        {
            if (_tab == null) return false;

            int index = -1;
            for (int i = 0; i < _length; i++)
                if ( (value == null && _tab[i] == null) || (value != null && value.Equals(_tab[i])) )
                {
                    index = i;
                    break;
                }
            if (index == -1) return false;

            T[] array = new T[--_length];
            for (int i = 0, j = 0; i < _length; i++)
                if (index != i) array[j++] = _tab[i];
            _tab = array;
            return true;
        }
        public IMyIterator<T> GetForwardBegin => new MyVectorIterator<T>(this, true, true);
        public IMyIterator<T> GetForwardEnd => new MyVectorIterator<T>(this, true, false); 
        public IMyIterator<T> GetReverseBegin => new MyVectorIterator<T>(this, false, true);
        public IMyIterator<T> GetReverseEnd => new MyVectorIterator<T>(this, false, false); 
    }
    public class MyDoubleLinkedList<T> : IMyCollection<T>
    {
        private MyNode<T>? _head;
        private MyNode<T>? _tail;
        private int _count;
        public int Count => _count;
        public MyNode<T>? Head => _head;
        public MyNode<T>? Tail => _tail;
        public MyDoubleLinkedList()
        {
            _head = null;
            _tail = null;
            _count = 0;
        }
        public MyDoubleLinkedList(params T[] array) : this()
        {
            foreach(var item in array) Add(item);
        }
        public void Add(T value)
        {
            MyNode<T> node = new(value);

            if (_head == null)
            {
                _head = node;
                _tail = node;
            }
            else
            {
                node.Prev = _tail;
                node.Next = null;
                _tail!.Next = node;
                _tail = node;
            }
            _count++;
        }
        public bool Delete(T value)
        {
            MyNode<T>? current = _head;

            while (current != null)
            {
                if ( (value == null && current.Value == null) || (value != null && value.Equals(current.Value)) )
                {
                    if (current.Prev == null && current.Next == null) // jeden element
                    {
                        _head = null;
                        _tail = null;
                    }
                    else if (current.Prev == null) // początek
                    {
                        _head = current.Next;
                        _head!.Prev = null;
                    }
                    else if (current.Next == null) // koniec
                    {
                        _tail = current.Prev;
                        _tail!.Next = null;
                    }
                    else // pozostałe
                    {
                        current.Prev.Next = current.Next;
                        current.Next.Prev = current.Prev;
                    }
                    _count--;
                    return true;
                }
                current = current.Next;
            }
            return false;
        }
        public IMyIterator<T> GetForwardBegin => new MyDoubleLinkedListIterator<T>(this, true, true);
        public IMyIterator<T> GetForwardEnd => new MyDoubleLinkedListIterator<T>(this, true, false);
        public IMyIterator<T> GetReverseBegin => new MyDoubleLinkedListIterator<T>(this, false, true);
        public IMyIterator<T> GetReverseEnd => new MyDoubleLinkedListIterator<T>(this, false, false);
    }
    public class MyNode<T>
    {
        public T Value { get; set; }
        public MyNode<T>? Prev { get; set; }
        public MyNode<T>? Next { get; set; }
        public MyNode(T value)
        {
            Value = value;
        }
    }
}
