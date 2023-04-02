using Collections;
using Interfaces;

namespace Iterators
{
    public class MyVectorIterator<T> : IMyIterator<T>
    {
        private readonly bool _isForward;
        private int _currentIndex;
        private readonly MyVector<T> _vector;

        public MyVectorIterator(MyVector<T> vector, bool isForward, bool isBegin)
        {
            _isForward = isForward;
            _vector = vector;
            if (isForward) _currentIndex = isBegin ? 0 : vector.Count;
            else _currentIndex = isBegin ? vector.Count - 1 : -1;
        }
        public int CurrentIndex => _currentIndex;
        public T CurrentValue => _vector.GetValue(_currentIndex);
        public bool MoveNext()
        {
            if (_isForward && _currentIndex <= _vector.Count - 1)
            {
                ++_currentIndex;
                return true;
            }
            else if (!_isForward && _currentIndex >= 0)
            {
                --_currentIndex;
                return true;
            }
            else return false;
        }
    }
    public class MyDoubleLinkedListIterator<T> : IMyIterator<T>
    {
        private readonly bool _isForward;
        private int _currentIndex;
        private MyNode<T>? _currentNode;
        private readonly MyDoubleLinkedList<T> _list;
        public MyDoubleLinkedListIterator(MyDoubleLinkedList<T> list, bool isForward, bool isBegin)
        {
            _isForward = isForward;
            _list = list;
            if (isForward)
            {
                _currentIndex = isBegin ? 0 : list.Count;
                _currentNode = isBegin ? _list.Head : null;
            }
            else
            {
                _currentIndex = isBegin ? list.Count - 1 : -1;
                _currentNode = isBegin ? _list.Tail : null;
            }
        }
        public int CurrentIndex => _currentIndex;
        public T CurrentValue => _currentNode != null ? _currentNode.Value : throw new NullReferenceException("Current node is null");
        public bool MoveNext()
        {
            if (_currentNode == null) return false;
            else if (_isForward)
            {
                _currentNode = _currentNode.Next;
                ++_currentIndex;
                return true;
            }
            else
            {
                _currentNode =_currentNode.Prev;
                --_currentIndex;
                return true;
            }
        }
    }
}
