// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NovelLogger.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        public IndexModel()
        {

        }

        public IActionResult OnGet()
        {
            return RedirectToPage("./Email");
        }
    }
}
