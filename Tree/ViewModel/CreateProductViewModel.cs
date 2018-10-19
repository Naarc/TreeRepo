using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tree.Models;

namespace Tree.ViewModel
{
	public class CreateProductViewModel : IEntity
	{
		public string Name { get; set; }
		public int CategoryId { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public IEnumerable<SelectListItem> Categories { get; set; }

	}
}