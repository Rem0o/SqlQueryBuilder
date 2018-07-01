using System;

namespace SqlQueryBuilder.Test.POCO
{
    public class Car
    {
        public Guid Id { get; set; }
        public int ModelYear { get; set; }
        public int Mileage { get; set; }
        public Guid MakerId { get; set; }
    }
}
