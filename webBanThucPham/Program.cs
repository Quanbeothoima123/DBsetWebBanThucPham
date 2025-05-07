using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using webBanThucPham.Models;
using AspNetCoreHero.ToastNotification;
using AspNetCoreHero.ToastNotification.Extensions;
using AspNetCoreHero.ToastNotification.Notyf;
using Microsoft.AspNetCore.Http.Features;
using SendGrid;
using webBanThucPham.Middlewares;
using DinkToPdf;
using DinkToPdf.Contracts;
using webBanThucPham.Helper;
using OfficeOpenXml;
using webBanThucPham.Models.Momo;
using webBanThucPham.Services.Momo;
var builder = WebApplication.CreateBuilder(args);


// Connect MomoAPi
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();

// Dat LicenseContext ngay sau khi khoi tao builder
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB
});


builder.Services.AddControllersWithViews();



builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50MB
});
// Dang ký DbContext voi connection string duoc dinh nghia trong appsettings.json
builder.Services.AddDbContext<DbBanThucPhamContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ConnectedDb"),
        new MySqlServerVersion(new Version(9, 0, 2))
    )
);


// Dang ky HtmlEncoder voi allowedRanges là UnicodeRanges.All
builder.Services.AddSingleton<HtmlEncoder>(HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.All }));
// Them cac service can thiet cho container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();


// Cau hinh Notyf
builder.Services.AddNotyf(config =>
{
    config.DurationInSeconds = 5; // thoi gian hien thi thong bao (giay)
    config.IsDismissable = true;  // Cho phep dong thong bao bang tay
    config.Position = NotyfPosition.TopRight; // vi tri hien thi
});


// Dang ky IWebHostEnvironment
builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);


// Dang ki SendGridClient voi DI container
builder.Services.AddSingleton<ISendGridClient>(provider =>
{
    var apiKey = builder.Configuration["SendGrid:ApiKey"];
    if (string.IsNullOrEmpty(apiKey))
    {
        throw new InvalidOperationException("SendGrid API Key is not configured.");
    }
    return new SendGridClient(apiKey);
});


var context = new CustomAssemblyLoadContext();
context.LoadUnmanagedLibrary(Path.Combine(builder.Environment.ContentRootPath, "DinkToPdf/Native/libwkhtmltox.dll"));
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); //Thiet lap thoi gian  timeout cho session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});




var app = builder.Build();

// Cau hinh middleware pipeline.

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Them UseSession vao Middleware
app.UseSession();


// Dieu huong thu cong neu truy cap dung "/admin"
app.Use(async (context, next) =>
{
    if (context.Request.Path.Equals("/admin", StringComparison.OrdinalIgnoreCase))
    {
        context.Response.Redirect("/Admin/AdminAccounts/Login");
        return;
    }
    await next();
});


app.UseMiddleware<AdminAuthorizationMiddleware>();

app.UseAuthorization();

// Them middleware Notyf
app.UseNotyf();

// Cau hinh dinh tuyen  cho Areas.
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

// Cau hinh route mac dinh
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
