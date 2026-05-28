using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Online_Auction.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        // Обработка ошибки 500
        // [HttpGet + HttpPost] — потому что ошибка может прийти любым методом
        [HttpGet("/error")]
        [HttpPost("/error")]
        public IActionResult HandleError()
        {
            // Логируем ошибку с деталями (для мониторинга)
            var exceptionHandlerPathFeature =
                HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature?.Error != null)
            {
                _logger.LogError(exceptionHandlerPathFeature.Error,
                    "Произошла необработанная ошибка на странице: {Path}",
                    exceptionHandlerPathFeature.Path);
            }

            // Возвращаем твою красивую страницу
            return View("~/Views/Shared/error.cshtml");
        }

        // Обработка кодов статуса: 404, 403, 500 и т.д.
        [HttpGet("/error/{code:int}")]
        public IActionResult HandleStatusCode(int code)
        {
            // Отдельная страница для 404
            if (code == StatusCodes.Status404NotFound)
            {
                _logger.LogWarning($"404: Страница не найдена: {Request.Path}");
                return View("~/Views/Shared/notFound.cshtml");
            }

            // Для остальных кодов — общая страница с кодом ошибки
            ViewBag.StatusCode = code;
            _logger.LogWarning($"Ошибка HTTP {code}: {Request.Path}");

            return View("~/Views/Shared/error.cshtml");
        }
    
    }
}
