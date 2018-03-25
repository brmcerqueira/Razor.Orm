using System;

namespace Razor.Orm.Example.Dto
{
    public class LocationDto
    {
        public int? LocationID { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Name { get; set; }
        public decimal CostRate { get; set; }
        public decimal Availability { get; set; }
    }
}
