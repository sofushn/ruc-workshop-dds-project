using ImageStoreAPI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

builder.Services.AddHttpClient();
builder.Services.AddHostedService<SyncBackgroundService>();

builder.Services.AddScoped<ReplicationService>();

builder.Services.AddOptions<ReplicationOptions>()
    .Configure<IConfiguration>((options, configuration) => {
        configuration.Bind("ReplicationOptions", options);
    })
    .ValidateDataAnnotations();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment()) {
    app.UseCors(policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
    );
}

app.UseHttpsRedirection();

var image = app.MapGroup("/image-api");

image.MapGet("images", ApiHandler.GetAll)
    .WithName("GetImageLinks")
    .Produces<string[]>(StatusCodes.Status200OK);

image.MapGet("images/{fileName}", ApiHandler.Get)
    .WithName("GetImage")
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

image.MapPost("images", ApiHandler.Post)
    .DisableAntiforgery() // Disable CSRF protection for this endpoint
    .WithName("PostImage")
    .Accepts<IFormFile>("multipart/form-data")
    .Produces(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);

bool isPrimary = app.Configuration.GetValue<bool>("ReplicationOptions:IsPrimary");
if (app.Environment.IsDevelopment() || !isPrimary) {
    RouteGroupBuilder sync = app.MapGroup("/sync");
    sync.MapPost("request", SyncHandler.Request)
        .DisableAntiforgery()
        .WithName("SyncRequest")
        .Accepts<SyncRequest>("multipart/form-data")
        .Produces(StatusCodes.Status200OK);

    sync.MapPost("check", SyncHandler.Check)
        .WithName("SyncCheck")
        .Produces<string[]>(StatusCodes.Status200OK);   
}

app.Run();
