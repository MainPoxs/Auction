using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Online_Auction.Models;
using Online_Auction.Services;

namespace Online_Auction.Controllers;
public class HomeController : Controller
{
    private readonly LotService lotService;
    public HomeController (LotService lotService)
    {
        this.lotService = lotService;       
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {        
        var lots = await lotService.GetLotsForHomeAsync(4);    

        return View(lots);
    }    

    [HttpGet]
    [Route("/about")]
    public IActionResult About()
    {
        return View("~/Views/Home/about.cshtml");
    }

    [HttpGet]
    [Route("/rules")]
    public IActionResult Rules()
    {
        return View("~/Views/Home/rules.cshtml");
    }
 
}

