using Base_Representation;

namespace Interfaces
{
    public interface IBaseRep
    {
        string ToString();
    }
    public interface ILine
    {
        string NumberHex { get; }
        int NumberDec { get; }
        string CommonName { get; }
        List<int> GetStopIds {  get; }
        List<int> GetVehicleIds { get; }
        string ToString();
        bool Equals(object? other)
        {
            if (other != null && other is ILine l)
                return l.NumberDec == NumberDec;
            return false;
        }
    }
    public interface IStop
    {
        int Id { get; }
        string Name { get; }
        EType Type { get; }
        List<int> GetLineIds { get; }
        string ToString();
        bool Equals(object? other)
        {
            if (other != null && other is IStop s)
                return s.Id == Id;
            return false;
        }
    }
    public interface IVehicle
    { 
        int Id { get; }
        List<int> GetLineIds { get; }
        string ToString();
        bool Equals(object? other)
        {
            if (other != null && other is IVehicle v)
                return v.Id == Id;
            return false;
        }
    }
    public interface IDriver
    {
        string Name { get; }
        string Surname { get; }
        int Seniority { get; }
        List<int> GetVehicleIds { get; }
        string ToString();
        bool Equals(object? other)
        {
            if (other != null && other is IDriver d)
                return d.Name == Name && d.Surname == Surname;
            return false;
        }
    }
    public interface IMyCollection<T>
    {
        void Add(T value);
        bool Delete(T value); //using Equals method, returns false if no occurence has been found, deletes only first occurence
        int Count { get; }
        IMyIterator<T> GetForwardBegin { get; }
        IMyIterator<T> GetForwardEnd { get; }
        IMyIterator<T> GetReverseBegin { get; }
        IMyIterator<T> GetReverseEnd { get; }
    }

    public interface IMyIterator<T>
    {
        int CurrentIndex { get; }
        T CurrentValue { get; }
        bool MoveNext();
    }

}
