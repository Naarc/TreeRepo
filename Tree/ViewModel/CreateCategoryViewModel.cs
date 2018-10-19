using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Tree.Models;

namespace Tree.ViewModel
{
	public class CreateCategoryViewModel : IEntity
	{
        [DisplayName("Nazwa kategorii")]
        [Required]
		public string Name { get; set; }

		public int ParentId { get; set; }

        [DisplayName("Kategoria nadrzędna")]
        public IEnumerable<SelectListItem> CategorySelectItems { get; set; }
	}
}