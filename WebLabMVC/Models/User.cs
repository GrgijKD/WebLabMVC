using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebLabMVC.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public bool IsClient { get; set; } = true;

        public bool IsWorker { get; set; } = false;

        public bool IsAdmin { get; set; } = false;

        [Precision(8, 2)]
        public decimal MoneyAmount { get; set; } = 0;
    }
}