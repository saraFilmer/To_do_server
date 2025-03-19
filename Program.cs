using TodoApi; 
using Pomelo.EntityFrameworkCore.MySql.Infrastructure; // מייבא את המודול המאפשר חיבור למסד נתונים MySQL
using Microsoft.EntityFrameworkCore; // מייבא את המודול של Entity Framework Core
using Microsoft.OpenApi.Models; // מייבא את המודול של Swagger

var builder = WebApplication.CreateBuilder(args); // יוצר את הבנאי של היישום

builder.Services.AddDbContext<ToDoDbContext>(options => // מוסיף את הקשר למסד הנתונים
options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"), // משתמש בחיבור שנמצא בקובץ ההגדרות
new MySqlServerVersion(new Version(8, 0, 40)))); // מציין את גרסת ה-MYSQL

builder.Services.AddCors(options => { // מוסיף הגדרות CORS
    options.AddPolicy("AllowAll", builder => builder 
    .AllowAnyOrigin() 
    .AllowAnyMethod() 
    .AllowAnyHeader()); 
});

builder.Services.AddEndpointsApiExplorer(); // מוסיף תמיכה בחקר נקודות הקצה
builder.Services.AddSwaggerGen(); // מוסיף תמיכה ב-Swagger

var app = builder.Build(); // בונה את היישום

app.UseCors("AllowAll"); // מפעיל את מדיניות ה-CORS
app.UseSwagger(); // מפעיל את Swagger
app.UseSwaggerUI(); // מפעיל את ממשק המשתמש של Swagger

app.MapGet("/tasks", async (ToDoDbContext db) => await db.Items.ToListAsync()); // נקודת קצה לקבלת כל המשימות

app.MapPost("/tasks", async (ToDoDbContext db, Item newItem) => { // נקודת קצה להוספת משימה חדשה
    db.Items.Add(newItem); // מוסיף את המשימה החדשה למסד הנתונים
    await db.SaveChangesAsync(); // שומר את השינויים
    return Results.Created($"/tasks/{newItem.Id}", newItem); // מחזיר את המשימה שנוצרה
});

app.MapPut("/tasks/{id}", async (ToDoDbContext db, int id, Item updatedItem) => // נקודת קצה לעדכון משימה
{
    var item = await db.Items.FindAsync(id); // מחפש את המשימה לפי מזהה
    if (item == null) return Results.NotFound(); // אם לא נמצאה, מחזיר שגיאה

    item.Name = item.Name; // שומר את השם המקורי של הפריט
    item.IsComplete = updatedItem.IsComplete; // מעדכן את המצב של המשימה

    await db.SaveChangesAsync(); // שומר את השינויים
    return Results.NoContent(); // מחזיר תשובה ללא תוכן
});

app.MapDelete("/tasks/{id}", async (ToDoDbContext db, int id) => { // נקודת קצה למחיקת משימה
    var item = await db.Items.FindAsync(id); // מחפש את המשימה לפי מזהה
    if (item == null) return Results.NotFound(); // אם לא נמצאה, מחזיר שגיאה
    db.Items.Remove(item); // מסיר את המשימה ממסד הנתונים
    await db.SaveChangesAsync(); // שומר את השינויים
    return Results.NoContent(); // מחזיר תשובה ללא תוכן
});

// הוספה
if (app.Environment.IsDevelopment()) // אם היישום נמצא במצב פיתוח
{
    app.UseDeveloperExceptionPage(); // מפעיל דף שגיאות לפיתוח
}

app.UseSwagger(); // מפעיל את Swagger
app.UseSwaggerUI(c => // מפעיל את ממשק המשתמש של Swagger
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); // מגדיר את נקודת הקצה של Swagger
    c.RoutePrefix = string.Empty; // מגדיר את ה-prefix של ה-route
});
app.MapGet("/", () => "Hello World!"); // נקודת קצה לברכת שלום

app.Run(); // מפעיל את היישום
