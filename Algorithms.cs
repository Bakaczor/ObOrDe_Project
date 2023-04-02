using Interfaces;

namespace Algorithms
{
    public static class MyCollectionAlgorithms
    {
        public static T? Find<T>(IMyCollection<T> collection, Func<T, bool> predicate, bool searchForward = true)
        {
            IMyIterator<T> it, end;
            if (searchForward)
            {
                it = collection.GetForwardBegin;
                end = collection.GetForwardEnd;
            }
            else
            {
                it = collection.GetReverseBegin;
                end = collection.GetReverseEnd;   
            }
            while (it != end)
            {
                if (predicate(it.CurrentValue)) return it.CurrentValue;
                it.MoveNext();
            }
            return default;
        }
        public static void Print<T>(IMyCollection<T> collection, Func<T, bool> predicate, bool searchForward = true)
        {
            IMyIterator<T> it, end;
            if (searchForward)
            {
                it = collection.GetForwardBegin;
                end = collection.GetForwardEnd;
            }
            else
            {
                it = collection.GetReverseBegin;
                end = collection.GetReverseEnd;
            }
            while (it != end)
            {
                if(it.CurrentValue != null) Console.WriteLine(it.CurrentValue.ToString());
                it.MoveNext();
            }
        }
    }


}
