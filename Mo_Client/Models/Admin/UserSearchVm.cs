using System;
using System.Collections.Generic;

namespace Mo_Client.Models.Admin
{
    public class UserSearchVm
    {
        public long? UserId { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsEKYCVerified { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class UserSearchResultVm
    {
        public List<ListAccountVm> Users { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public string? Error { get; set; }
        public string? Success { get; set; }
    }
}
