using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProiectMPA.Models;
using System.Security.Claims;

namespace ProiectMPA.Data
{
    public class ProiectMPAContext : IdentityDbContext<IdentityUser>
    {
        public ProiectMPAContext (DbContextOptions<ProiectMPAContext> options) : base(options)
        {
        }

        public DbSet<ProiectMPA.Models.Car> Car { get; set; } = default!;
        public DbSet<ProiectMPA.Models.ChasisType> ChasisType { get; set; } = default!;
        public DbSet<ProiectMPA.Models.Client> Client { get; set; } = default!;
        public DbSet<ProiectMPA.Models.Manufacturer> Manufacturer { get; set; } = default!;
        public DbSet<ProiectMPA.Models.Order> Order { get; set; } = default!;
    }
}
