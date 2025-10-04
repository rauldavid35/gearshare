using Microsoft.AspNetCore.Identity;
using System;

namespace GearShare.Api.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? DisplayName { get; set; }
        public double RatingAvg { get; set; } = 0.0;
    }
}