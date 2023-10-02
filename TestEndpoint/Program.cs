var builder = WebApplication.CreateBuilder(args);



var app = builder.Build();



app.UseHttpsRedirection();


app.MapPost("/api/someservice", () => {
    Console.WriteLine("We got a post request");

    return Results.Ok();
});


app.Run();


