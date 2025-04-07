using System;

namespace CwrStatusChecker.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string ManagerEmail { get; set; }
        public DateTime? LastPasswordChange { get; set; }
        public DateTime? AccountExpirationDate { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsArchived { get; set; }
        public string LdapEndpoint { get; set; }
        public DateTime LastChecked { get; set; }
    }
} 