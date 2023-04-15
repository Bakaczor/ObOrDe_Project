using Interfaces;

namespace Algorithms
{
    public static class MyCollectionAlgorithms
    {
        //TASK 3 ALGORITHMS
        public static T? Find<T>(IMyCollection<T> collection, Func<T, bool> predicate, bool searchForward = true)
        {
            IMyIterator<T> it = searchForward ? collection.GetForwardBegin : collection.GetReverseBegin;
            while (true)
            {
                if (predicate(it.CurrentValue)) return it.CurrentValue;
                if (!it.MoveNext()) break;
            }
            return default;
        }
        public static void Print<T>(IMyCollection<T> collection, Func<T, bool> predicate, bool searchForward = true)
        {
            IMyIterator<T> it = searchForward ? collection.GetForwardBegin : collection.GetReverseBegin;
            while (true)
            {
                if (predicate(it.CurrentValue) && it.CurrentValue != null) Console.WriteLine(it.CurrentValue.ToString());
                if (!it.MoveNext()) break;
            }
        }
        //TASK 4 ALGORITHMS
        public static T? Find<T>(in IMyIterator<T> iterator, Func<T, bool> predicate)
        {
            while (true)
            {
                if (predicate(iterator.CurrentValue)) return iterator.CurrentValue;
                if (!iterator.MoveNext()) break;
            }
            return default;
        }
        public static void ForEach<T>(in IMyIterator<T> iterator, Action<T> function)
        {
            while (true)
            {
                function(iterator.CurrentValue);
                if (!iterator.MoveNext()) break;
            }
        }
        public static int CountIf<T>(in IMyIterator<T> iterator, Func<T, bool> predicate)
        {
            int count = 0;
            while (true)
            {
                if (predicate(iterator.CurrentValue)) count++;
                if (!iterator.MoveNext()) break;
            }
            return count;
        }
    }


}
