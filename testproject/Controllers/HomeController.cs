using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; 
using System;
using System.Collections.Generic;
using testproject.ViewModels;

namespace testproject.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var model = new AccountViewModel
            {
                LoginModel = new LoginViewModel(),
                RegisterModel = new RegisterViewModel(),
                SelectLists = GetSelectLists()
            };

            return View(model);
        }

        // ... остальные методы ...

        private SelectListsViewModel GetSelectLists()
        {
            return new SelectListsViewModel
            {
                Days = GetDays(),
                Months = GetMonths(),
                Years = GetYears()
            };
        }

        private List<SelectListItem> GetYears()
        {
            var years = new List<SelectListItem>();
            var currentYear = DateTime.Now.Year;

            // Добавляем placeholder
            years.Add(new SelectListItem
            {
                Value = "",
                Text = "Год",
                Selected = true,
                Disabled = true
            });

            // Добавляем годы
            for (int year = currentYear; year >= currentYear - 100; year--)
            {
                years.Add(new SelectListItem
                {
                    Value = year.ToString(),
                    Text = year.ToString()
                });
            }

            return years;
        }

        private List<SelectListItem> GetMonths()
        {
            var months = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "",
                    Text = "Месяц",
                    Selected = true,
                    Disabled = true
                }
            };

            var monthNames = new[]
            {
                "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
                "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
            };

            for (int i = 0; i < monthNames.Length; i++)
            {
                months.Add(new SelectListItem
                {
                    Value = (i + 1).ToString(),
                    Text = monthNames[i]
                });
            }

            return months;
        }

        private List<SelectListItem> GetDays()
        {
            var days = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "",
                    Text = "День",
                    Selected = true,
                    Disabled = true
                }
            };

            for (int day = 1; day <= 31; day++)
            {
                days.Add(new SelectListItem
                {
                    Value = day.ToString(),
                    Text = day.ToString()
                });
            }

            return days;
        }
    }
}