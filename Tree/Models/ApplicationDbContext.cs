using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Tree.Models
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext()
			: base("DefaultConnection", throwIfV1Schema: false)
		{
		}
		
		
		public DbSet<Category> Categories { get; set; }
		
		

		internal IEnumerable<T> ExecuteQuery<T>(string v)
		{
			throw new NotImplementedException();
		}

		

		public static ApplicationDbContext Create()
		{
			return new ApplicationDbContext();
		}
	}
}