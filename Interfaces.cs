using Base_Representation;
using System;
using System.Collections.Generic;

namespace Interfaces
{
    public interface IBaseRep
    {
        string ToString();
    }
    public interface ILine
    {
        public string NumberHex { get; }
        public int NumberDec { get; }
        public string CommonName { get; }
        public List<int> GetStopIds {  get; }
        public List<int> GetVehicleIds { get; }
        string ToString();
    }
    public interface IStop
    {
        public int Id { get; }
        public string Name { get; }
        public EType Type { get; }
        public List<int> GetLineIds { get; }
        string ToString();
    }
    public interface IVehicle
    { 
        public int Id { get; }
        public List<int> GetLineIds { get; }
        string ToString();
    }
    //public interface IBytebus
    //{
    //    public int Id { get; }
    //    public EEngineClass EngineClass { get; }
    //    public List<int> GetLineIds { get; }
    //    string ToString();
    //}
    //public interface ITram
    //{
    //    public int Id { get; }
    //    public int CarsNumber { get; }
    //    public int GetLineId { get; }
    //    string ToString();
    //}
    public interface IDriver
    {
        public string Name { get; }
        public string Surname { get; }
        public int Seniority { get; }
        public List<int> GetVehicleIds { get; }
        string ToString();
    }
}
