using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [AllowNull]
        public string Description { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public List<OrderItem> OrderItems { get; set; }
    }
}