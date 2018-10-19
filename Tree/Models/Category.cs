using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Tree.Models
{
	public class Category : IEntity
	{
		public string Name { get; set; }

		public int LftId { get; set; }

		public int RgtId { get; set; }

		public int ParentId { get; set; }

        [NotMapped]
        public int Depth;
	}
}