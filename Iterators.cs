using Collections;
using Interfaces;

namespace Iterators
{
    public class MyVectorIterator<T> : IMyIterator<T>
    {
        private readonly bool _isForward;
        private int _currentIndex;
        private readonly MyVector<T> _vector;

        public MyVectorIterator(MyVector<T> vector, bool isForward)
        {
            _isForward = isForward;
            _vector = vector;
            _currentIndex = isForward ? 0 : vector.Count - 1;
        }
        public int CurrentIndex => _currentIndex;
        public T CurrentValue => _vector.GetValue(_currentIndex);
        public bool MoveNext()
        {
            if (_isForward && _currentIndex < _vector.Count - 1)
            {
                ++_currentIndex;
                return true;
            }
            else if (!_isForward && _currentIndex > 0)
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

        public MyDoubleLinkedListIterator(MyDoubleLinkedList<T> list, bool isForward)
        {
            _isForward = isForward;
            _list = list;
            _currentIndex = isForward ? 0 : list.Count - 1;
            _currentNode = isForward ? _list.Head : _list.Tail;
        }
        public int CurrentIndex => _currentIndex;
        public T CurrentValue => _currentNode != null ? _currentNode.Value : throw new NullReferenceException("Current node is null");
        public bool MoveNext()
        {
            if (_currentNode == null)
            {
                _currentNode = _isForward ? _list.Head : _list.Tail;
                return _currentNode != null;
            }
            else if (_isForward && _currentNode.Next != null)
            {
                _currentNode = _currentNode.Next;
                ++_currentIndex;
                return true;
            }
            else if (!_isForward && _currentNode.Prev != null)
            {
                _currentNode = _currentNode.Prev;
                --_currentIndex;
                return true;
            }
            return false;
        }
    }

    public class MyBinaryHeapIterator<T> : IMyIterator<T>
    {
        private readonly bool _isForward;
        private int _currentIndex;
        private readonly MyBinaryHeap<T> _heap;
        public MyBinaryHeapIterator(MyBinaryHeap<T> heap, bool isForward)
        {
            _isForward = isForward;
            _heap = heap;
            _currentIndex = isForward ? 0 : _heap.Count - 1;
        }
        public int CurrentIndex => _currentIndex;
        public T CurrentValue => _heap.GetValue(_currentIndex);
        public bool MoveNext()
        {
            if (_isForward && _currentIndex < _heap.Count - 1)
            {
                ++_currentIndex;
                return true;
            }
            else if (!_isForward && _currentIndex > 0)
            {
                --_currentIndex;
                return true;
            }
            else return false;
        }
    }
}
