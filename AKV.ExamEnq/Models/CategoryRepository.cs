using AKV.ExanEnq.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AKV.ExamEnq.Models
{
    public class CategoryRepository
    {
        public static CategoryRepository Repository = new CategoryRepository();
        public IEnumerable<CategoryViewModel> GetCategory()
        {
            var list = new List<CategoryViewModel>();
            var result = new Entities().CATEGORies.ToList().Select(t => new CategoryViewModel {  categoryname = t.CATEGORYNAME });

            foreach (var item in result)
            {

                list.Add(new CategoryViewModel { categoryname = item.categoryname });
            }


            return list;
        }
    }
}