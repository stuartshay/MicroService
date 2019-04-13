using System.ComponentModel.DataAnnotations;

namespace MicroService.Data.Models
{
    public class TestData : BaseEntity
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public double Data { get; set; }
    }
}
