using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [AllowNull]
        public string Description { get; set; }

        [AllowNull]
        public Stock Stock { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }
    }

    public enum Stock
    {
        Low,
        Medium,
        High
    }
}