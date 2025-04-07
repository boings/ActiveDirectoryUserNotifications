using System;
using System.ComponentModel.DataAnnotations;

namespace CwrStatusChecker.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public required string Username { get; set; }

        [Required]
        public required string Email { get; set; }

        [Required]
        public required string ManagerEmail { get; set; }

        public DateTime? LastPasswordChange { get; set; }

        public DateTime? AccountExpirationDate { get; set; }

        public bool IsDisabled { get; set; }

        public bool IsArchived { get; set; }

        [Required]
        public required string LdapEndpoint { get; set; }

        public DateTime LastChecked { get; set; }
    }
} 