using System.Text;
using pastebin;
using System.Net;

var yandexStorage = new YandexStorage();

var builder = WebApplication.CreateBuilder();
var app = builder.Build();
 
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/", async (HttpContext context) => {
    var form = context.Request.Form;
    string? author = form["name"];
    string? text = form["paste"];
    if(author is null || text is null) {
        return Results.BadRequest("Not all gaps were filled");
    }
    using(var db = new PastebinContext()) {
        using(var transaction = await db.Database.BeginTransactionAsync()) {
            try {
                var count = db.Posts.Count().ToString();
                while(count.Length < 10) {
                    count = "0" + count;
                }
                var hash = Convert.ToBase64String(Encoding.UTF8.GetBytes(count));
                var newPost = new Post{
                    Hash = hash,
                    Author = author
                };

                var dbNewPost = db.Posts.AddAsync(newPost);
                var placeText = yandexStorage.PlaceText(text, hash);

                await dbNewPost;
                await placeText;
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                return Results.Redirect($"/{hash}");
            }
            catch {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
});

app.MapGet("/", async (HttpContext context) =>  await context.Response.SendFileAsync("wwwroot/index.html"));

app.MapGet("/{hash}", async (string hash, HttpContext context) => {
    using(var db = new PastebinContext()) {
        using(var transaction = await db.Database.BeginTransactionAsync()) {
            try {
                var post = db.Posts.Where(p => p.Hash == hash).FirstOrDefault();
                if(post != null) {
                    string text = await yandexStorage.GetText(hash);
                    string html = $@"
                    <!DOCTYPE html>
                    <html lang='en'>
                    <head>
                        <meta charset='UTF-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <title>{post.Author} Paste</title>
                    </head>
                    <body>
                        <h3> Author: {post.Author} </h3>
                        <h3> Paste: </h3>
                        <pre>{WebUtility.HtmlEncode(text)}</pre>
                    </body>
                    </html>";
                    await context.Response.WriteAsync(html);    
                } else {
                    await context.Response.SendFileAsync("wwwroot/index.html");
                }
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
});

app.Run();