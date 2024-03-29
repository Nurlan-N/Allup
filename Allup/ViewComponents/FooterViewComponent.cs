﻿using Allup.DataAccessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Allup.ViewComponents
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;

        public FooterViewComponent(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            IDictionary<string,string> settings = await _context.Setting.ToDictionaryAsync(s => s.Key , s => s.Value);

            return View(await Task.FromResult(settings));
        }
    }
}
