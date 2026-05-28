using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Online_Auction.Models;
using Online_Auction.Services;

var builder = WebApplication.CreateBuilder(args);

// получаем строку подключения из файла конфигурации
string connection =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();  // поддержка контроллеров

// регистрация AuctionDbContext в качестве сервиса
builder.Services.AddDbContext<AuctionDbContext>
    (options => options.UseNpgsql(connection));

// регистрация в качестве сервиса
// сервисы, работающие с БД, должны быть Scoped
// создают один экзем. объекта на весь HTTP-запрос
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<LotService>();
builder.Services.AddScoped<BidService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<AuctionService>();
builder.Services.AddScoped<PurchaseService>();
builder.Services.AddScoped<FeedbackService>();

// добавление авторизации и аутентификации 
builder.Services.AddAuthentication(CookieAuthenticationDefaults
    .AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/login";  // если нет доступа

        options.Cookie.HttpOnly = true; // настройка безопасности
        //Куки передаются только по защищённому протоколу HTTPS
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        options.Cookie.SameSite = SameSiteMode.Lax;  // Защита от CSRF
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);  // Время жизни сессии
        options.SlidingExpiration = true;  // Продлевать сессию при активности
    });

builder.Services.AddAuthorization(options =>
{ 
    // Политики для ролей
    options.AddPolicy("AdminOnly", policy =>
    policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy =>
    policy.RequireRole("User", "Admin"));
});

var app = builder.Build();

app.UseStaticFiles(); // поддержка статических файлов
app.UseRouting();    // маршрутизация
app.UseAuthentication();
app.UseAuthorization();

// Для тестирования 
app.Environment.EnvironmentName = "Production"; 

// Глобальный перехват исключений
if (app.Environment.IsDevelopment())
{
    // В разработке — показываем детали для отладки
    app.UseDeveloperExceptionPage();
}
else
{
    // В production — перехватываем все исключения и редиректим на /error
    app.UseExceptionHandler("/error");

    // Перехват 404, 403 и других кодов статуса
    app.UseStatusCodePagesWithReExecute("/error/{0}");
}

// ChatHub будет обрабатывать запросы по пути /chat
app.MapHub<ChatHub>("/chat");   

// устанавливаем сопоставление маршрутов с контроллерами
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// логирование запросов
app.Use(async (context, next) =>
{
    var logger = context.RequestServices
    .GetRequiredService<ILogger<Program>>();

    var startTime = DateTime.UtcNow;
    var path = context.Request.Path.ToString();
    var method = context.Request.Method;
    var user = context.User.Identity?.Name ?? "Аноним";

    // Логируем входящий запрос
    logger.LogInformation("[{Method}] {Path} | Пользователь: {User}",
        method, path, user);

    try
    {
        await next(); // передаём управление дальше по конвейеру

        // Логируем успешный ответ
        var duration = DateTime.UtcNow - startTime; //сколько миллисекунд заняла обработка
        var statusCode = context.Response.StatusCode;

        logger.LogInformation("[{StatusCode}] {Path} | За {Duration} мс",
            statusCode, path, duration.TotalMilliseconds);
    }
    catch (Exception ex)
    {
        // Логируем ошибку
        logger.LogError(ex, "Ошибка при обработке [{Method}] {Path} | Пользователь: {User}",
            method, path, user);
        throw; 
    }
});


app.Run();
