using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace testproject.ViewModels
{
    public class AccountViewModel
    {
        public required LoginViewModel LoginModel { get; set; }
        public required RegisterViewModel RegisterModel { get; set; }
        public required SelectListsViewModel SelectLists { get; set; }
    }

    public class SelectListsViewModel
    {
        public required List<SelectListItem> Days { get; set; }
        public required List<SelectListItem> Months { get; set; }
        public required List<SelectListItem> Years { get; set; }
    }
}