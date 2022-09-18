using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc(name: "v1", info: new OpenApiInfo
    {
        Version = "v1",
        Title = "TestApi",
        Description = "MyFirst API",
        TermsOfService = new Uri(uriString: "http://www.google.com"),
        Contact = new OpenApiContact
        {
            Name = "ahmed",
            Email = "elneziliiyahmed@gmail.com"
        },
        
        License  = new OpenApiLicense
        {
            Name="mylicience",
            Url= new Uri(uriString: "http://www.google.com")
        }
        
    });
    option.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your KEY"
    });
    option.AddSecurityRequirement(securityRequirement: new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference=new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                },
                Name="Bearer",
                In=ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
