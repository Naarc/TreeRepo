using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Tree.Models;
using Tree.ViewModel;

namespace Tree.Controllers
{
	public class CategoriesController : Controller
	{
		private ApplicationDbContext db = new ApplicationDbContext();

		public Category ParentId { get; private set; }

		// GET: Categories
		public ActionResult Index()
		{
			var ViewModel = new IndexCategoryViewModel();
			var categories = (
				from node in db.Categories
				orderby node.LftId ascending
				select new
				{
					LftId = node.LftId,
					RgtId = node.RgtId,
					Name = node.Name,
					Id = node.Id,
					//find all when left < myleft and right > myright will give you all parents
					Depth = (
						from parent in db.Categories
						where parent.LftId < node.LftId && parent.RgtId > node.RgtId
						select (parent.Id)
					).Count()
					
				}
			).ToList();
			List<CategoryRow> CategoryList = new List<CategoryRow>();
			//Create new viemodel, which will be named 'IndexCategoryViewModel
			foreach (var cat in categories)
			{
				CategoryRow category = new CategoryRow();
				category.Name = cat.Name;
				category.Id = cat.Id;
				category.Depth = cat.Depth;
				category.LftId = cat.LftId;
				category.RgtId = cat.RgtId;
				CategoryList.Add(category);
			}
			
			ViewModel.categories = CategoryList;
			return View(ViewModel);
			
		}

		// GET: Categories/Details/5
		public ActionResult Details(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Category category = db.Categories.Find(id);
			if (category == null)
			{
				return HttpNotFound();
			}
			return View(category);
		}

		// GET: Categories/Create
		[Authorize(Roles = "admin")]
		public ActionResult Create()
		{
			var viewModel = new CreateCategoryViewModel();
			viewModel.CategorySelectItems = getCategorySelectItems();
			return View(viewModel);
		}

		private List<SelectListItem> getCategorySelectItems()
		{
			var categories = (
				from node in db.Categories
				orderby node.LftId ascending
				select new
				{
					Name = node.Name,
					Id = node.Id,
					//find all when left < myleft and right > myright will give you all parents
					Depth = (
						from parent in db.Categories
						where parent.LftId < node.LftId && parent.RgtId > node.RgtId
						select (parent.Id)
					).Count()
				}
			).ToList();

			List<SelectListItem> selectItems = new List<SelectListItem> {
				new SelectListItem{Text = "-- wybierz --", Value="0" }
			};
			foreach (var item in categories)
			{
				SelectListItem itemList = new SelectListItem
				{
					Text = String.Concat(Enumerable.Repeat("-", item.Depth)) + item.Name,
					Value = item.Id.ToString()
				};

				selectItems.Add(itemList);
			}
			return selectItems;
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "admin")] 
		public ActionResult Create([Bind(Include = "Name, ParentId")] CreateCategoryViewModel category)
		{
			if (ModelState.IsValid)
			{
				
				if (category.ParentId > 0)
				{
					//my new left is right of my current parent
					int newLeft = (
						from c in db.Categories
						where c.Id == category.ParentId
						select c.RgtId
					).ToList()[0];

					//select all categories that have right higher or equals to my new left
					var lefts = db.Categories.Where(c => c.RgtId >= newLeft).ToList();
					//select all categories that have left higher than my new left
					var rights = db.Categories.Where(c => c.LftId > newLeft).ToList();

					lefts.ForEach(c => c.RgtId = c.RgtId + 2);
					db.SaveChanges();
					rights.ForEach(c => c.LftId = c.LftId + 2);
					db.SaveChanges();

					Category newCategory = new Category();
					newCategory.LftId = newLeft;
					newCategory.RgtId = newLeft + 1;
					newCategory.Name = category.Name;
					newCategory.ParentId = category.ParentId;

					db.Categories.Add(newCategory);
					db.SaveChanges();
				}
				else
				{
					int maxValue = 0;
					if (db.Categories.Any())
					{
						maxValue = db.Categories.Max(x => x.RgtId);
					}

					Category newCategory = new Category();
					newCategory.LftId = maxValue + 1;
					newCategory.RgtId = newCategory.LftId + 1;
					newCategory.Name = category.Name;

					db.Categories.Add(newCategory);
					db.SaveChanges();
				}

				return RedirectToAction("Index");
			}
			return View(category);
		}

		// GET: Categories/Edit/5
		[Authorize(Roles = "admin")]
		public ActionResult Edit(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			Category category = db.Categories.Find(id);
			if (category == null)
			{
				return HttpNotFound();
			}
			return View(category);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "admin")]
		public ActionResult Edit([Bind(Include = "Id,Name, LftId, RgtId, ParentId")] Category category)
		{
			
			if (ModelState.IsValid)
			{
				db.Entry(category).State = EntityState.Modified;
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			return View(category);
		}

		// GET: Categories/Delete/5
		[Authorize(Roles = "admin")]
		public ActionResult Delete(int? id)
		{
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			Category category = db.Categories.Find(id);
			if (category == null)
			{
				return HttpNotFound();
			}

			//SELECT lft, rgt, (rgt - lft), (rgt - lft + 1), parent_id
			//INTO new_lft, new_rgt, has_leafs, width, superior_parent
			//FROM tree_map WHERE node_id = pnode_id;
			var removalData = (
				from node in db.Categories
				where node.Id == category.Id
				select new
				{
					NewLeft = node.LftId,
					NewRight = node.RgtId,
					HasLeafs = node.RgtId - node.LftId > 1,
					Width = node.RgtId - node.LftId + 1,
					ParentId = node.ParentId
				}
			).ToList()[0];

			//DELETE FROM tree_content WHERE node_id = pnode_id;
			db.Categories.Remove(category);
			db.SaveChanges();

			if (removalData.HasLeafs)
			{
				//DELETE FROM tree_map WHERE lft BETWEEN new_lft AND new_rgt;
				var toRemove = db.Categories.Where(c => c.LftId > removalData.NewLeft && c.LftId < removalData.NewRight).ToList();
				db.Categories.RemoveRange(toRemove);
				db.SaveChanges();

				var rights = db.Categories.Where(c => c.RgtId > removalData.NewRight).ToList();
				var lefts = db.Categories.Where(c => c.LftId > removalData.NewRight).ToList();

				//UPDATE tree_map SET rgt = rgt - width WHERE rgt > new_rgt;
				//UPDATE tree_map SET lft = lft - width WHERE lft > new_rgt;
				rights.ForEach(c => c.RgtId = c.RgtId - removalData.Width);
				db.SaveChanges();
				lefts.ForEach(c => c.LftId = c.LftId - removalData.Width);
				db.SaveChanges();
			}
			else
			{
				//DELETE FROM tree_map WHERE lft = new_lft;
				var toRemove = db.Categories.Where(c => c.LftId == removalData.NewLeft).ToList();
				db.Categories.RemoveRange(toRemove);
				db.SaveChanges();

				//UPDATE tree_map SET rgt = rgt - 1, lft = lft - 1, parent_id = superior_parent WHERE lft BETWEEN new_lft AND new_rgt;
				var toUpdate = db.Categories.Where(c => c.LftId >= removalData.NewLeft && c.LftId < removalData.NewRight).ToList();
				toUpdate.ForEach(c =>
				{
					c.RgtId = c.RgtId - 1;
					c.LftId = c.LftId - 1;
					c.ParentId = removalData.ParentId;
				});
				db.SaveChanges();

				//UPDATE tree_map SET rgt = rgt - 2 WHERE rgt > new_rgt;
				//UPDATE tree_map SET lft = lft - 2 WHERE lft > new_rgt;
				var rights = db.Categories.Where(c => c.RgtId > removalData.NewRight).ToList();
				var lefts = db.Categories.Where(c => c.LftId > removalData.NewRight).ToList();

				rights.ForEach(c => c.RgtId = c.RgtId - 2);
				db.SaveChanges();
				lefts.ForEach(c => c.LftId = c.LftId - 2);
				db.SaveChanges();
			}

			return RedirectToAction("Index");
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				db.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
