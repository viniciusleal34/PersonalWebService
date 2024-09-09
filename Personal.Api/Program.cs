using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Personal.Api.Filters;
using Personal.Application.Auth;
using Personal.Application.Cases.AnamnesisQuestion;
using Personal.Application.Cases.Exercises;
using Personal.Application.Cases.ExerciseTrainings;
using Personal.Application.Cases.PhysicalAssessment;
using Personal.Application.Cases.Trainings;
using Personal.Application.Cases.Users;
using Personal.Application.Config;
using Personal.Application.UserLogger;
using Personal.Application.Validations;
using Personal.Domain.Repositories;
using Personal.Infrastructure.AuthServices;
using Personal.Infrastructure.Persistence.Model.Context;
using Personal.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Configuration MYSQL
var connection = builder.Configuration["MySQLConnection:MySQLConnectionString"];
builder.Services.AddDbContext<MySqlContext>(options =>
    options.UseMySql(connection, new MySqlServerVersion(new Version(8, 0, 5))));


// Configuration AutoMapper
var mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);

// Configuration Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IExerciseRepository, ExerciseRepository>();
builder.Services.AddScoped<IExerciseTrainingRepository, ExerciseTrainingRepository>();
builder.Services.AddScoped<ITrainingRepository, TrainingRepository>();
builder.Services.AddScoped<IPerimeterRepository, PerimeterRepository>();
builder.Services.AddScoped<IPhysicalAssessmentRepository, PhysicalAssessmentRepository>();
builder.Services.AddScoped<ISkinFoldRepository, SkinFoldRepository>();
builder.Services.AddScoped<IAnamnesisQuestionRepository, AnamnesisQuestionRepository>();
builder.Services.AddScoped<IBodyCompositionRepository, BodyCompositionRepository>();


// Configuration IAuthServices
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IUserLogged, UserLogged>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

//Configuration Application
builder.Services.AddScoped<IUserCase, UserCase>();
builder.Services.AddScoped<IAuthApplication, AuthApplication>();
builder.Services.AddScoped<IExerciseCase, ExerciseCase>();
builder.Services.AddScoped<IExerciseTrainingCase, ExerciseTrainingCase>();
builder.Services.AddScoped<ITrainingCase, TrainingCase>();
builder.Services.AddScoped<IAnamnesisQuestionCase, AnamnesisQuestionCase>();
builder.Services.AddScoped<IPhysicalAssessmentCase, PhysicalAssessmentCase>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});
//Validation
builder.Services.AddScoped<IUserValidation, UserValidation>();
// Controller
builder.Services.AddMvc(options => options.Filters.Add(typeof(ExceptionsFilter)));

// Configurações de authenticação no swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Personal.API", Version = "v1" });
                
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header usando o esquema Bearer"
    });
                
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

// Configurações do JWT 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

var app = builder.Build();


// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

// app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin"); // Enable CORS using the specified policy
app.UseAuthorization();

app.MapControllers();

app.Run();