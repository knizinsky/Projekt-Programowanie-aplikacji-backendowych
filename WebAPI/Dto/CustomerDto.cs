using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace WebAPI.Dto
{
    public class CustomerDto
    {
        public int Id { get; }

        [Required]
        public string Name { get; set; }

        [AllowNull]
        public string Description { get; set; }
    }
}