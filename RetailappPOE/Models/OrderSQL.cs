using RetailappPOE.Models.SQLModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RetailappPOE.Models
{
    public class OrderSQL
    {
        public int Id { get; set; }

        public int CustomerId { get; set; } = 0; // Always set default for guest

        public DateTime? OrderDate { get; set; }

        public string Status { get; set; } = "PENDING";

        public List<CartItem> Items { get; set; } = new();
    }
}