using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tree.Models;

namespace Tree.ViewModel
{
	public class IndexCategoryViewModel
	{
		public List<CategoryRow> categories; 
	}

	public class CategoryRow
	{
		public string Name { get; set; }
		public int Id { get; set; }
		public int Depth { get; set; }
		public int LftId { get; set; }
		public int RgtId { get; set; }
	}
}