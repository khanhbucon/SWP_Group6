using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mo_Client.Models
{
    public class ProfileVm
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Username là bắt buộc")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username phải từ 3-50 ký tự")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool IsEKYCVerified { get; set; }
        public int TotalOrders { get; set; }
        public int TotalShops { get; set; }
        public int TotalProductsSold { get; set; }
        public string? IdentificationF { get; set; }  // Thêm field này
        public string? IdentificationB { get; set; }  // Thêm field này
    }
}
