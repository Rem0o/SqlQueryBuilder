using System;

namespace SqlQueryBuilder.Test.POCO
{
    public class Car
    {
        public Guid Id { get; set; }
        public int ModelYear { get; set; }
        public int Mileage { get; set; }
        public decimal Price { get; set; }
        public Guid CarMakerId { get; set; }
    }
}
