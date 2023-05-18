using Base_Representation;

namespace Interfaces
{
    public interface IBaseRep
    {
        string ToString();
    }
    public interface ILine
    {
        string NumberHex { get; set; }
        int NumberDec { get; set; }
        string CommonName { get; set; }
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
        int Id { get; set; }
        string Name { get; set; }
        EType Type { get; set; }
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
        int Id { get; set; }
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
        string Name { get; set; }
        string Surname { get; set; }
        int Seniority { get; set; }
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
        void Delete() { } //deletes the element of the highest priority
        bool Delete(T value) { return false; } //using Equals method, returns false if no occurence has been found, deletes only first occurence
        int Count { get; }
        IMyIterator<T> GetForwardBegin { get; }
        IMyIterator<T> GetReverseBegin { get; }
    }
    public interface IMyIterator<T>
    {
        int CurrentIndex { get; }
        T CurrentValue { get; }
        bool MoveNext();
    }

}
