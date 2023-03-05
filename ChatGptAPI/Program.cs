using OpenAI.GPT3.Extensions;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenAIService();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/bot", async (string input, IOpenAIService openAiService) =>
{
    var answer = string.Empty;
    var completionResult = openAiService.Completions.CreateCompletionAsStream(new CompletionCreateRequest()
    {
        Prompt = input,
        MaxTokens = 4000
    }, Models.TextDavinciV3);

    await foreach (var completion in completionResult)
    {
        if (completion.Successful)
        {
            answer += completion.Choices.FirstOrDefault()?.Text;
        }
        else
        {
            if (completion.Error == null)
            {
                throw new Exception("Unknown Error");
            }

            throw new InvalidOperationException($"{completion.Error.Code}: {completion.Error.Message}");
        }
    }

    return Results.Ok(answer);
})
.WithName("GetBot");

app.MapGet("/img", async (string input, IOpenAIService openAiService) =>
{
    var answer = string.Empty;
    var imageResult = await openAiService.Image.CreateImage(new ImageCreateRequest
    {
        Prompt = input,
        N = 1,
        Size = StaticValues.ImageStatics.Size.Size1024,
        ResponseFormat = StaticValues.ImageStatics.ResponseFormat.Url
    });


    if (imageResult.Successful)
    {
        answer = string.Join("\n", imageResult.Results.Select(r => r.Url));
    }
    else
    {
        if (imageResult.Error == null)
        {
            throw new Exception("Unknown Error");
        }

        throw new InvalidOperationException($"{imageResult.Error.Code}: {imageResult.Error.Message}");
    }

    return Results.Ok(answer);
})
.WithName("GetImage");

app.Run();