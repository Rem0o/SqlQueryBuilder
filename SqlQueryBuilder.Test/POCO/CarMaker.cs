using System;

namespace SqlQueryBuilder.Test.POCO
{
    public class CarMaker
    {
        public Guid Id { get; set; }
        public string Name { get; set;  }
        public Guid CountryOfOriginId { get; set; }
        public DateTime FoundationDate { get; set; }
    }
}
