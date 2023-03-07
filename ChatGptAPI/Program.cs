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

app.MapGet("/chat", async (string input, IOpenAIService openAiService) =>
{
    var answer = string.Empty;

    var completionResult = await openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
    {
        Messages = new List<ChatMessage>
    {
        ChatMessage.FromUser(input),
    },
        Model = Models.ChatGpt3_5Turbo
    });
    if (completionResult.Successful)
    {
        answer = completionResult.Choices.First().Message.Content;
    }
    else
    {
        if (completionResult.Error == null)
        {
            throw new Exception("Unknown Error");
        }

        throw new InvalidOperationException($"{completionResult.Error.Code}: {completionResult.Error.Message}");
    }

    return Results.Ok(answer);
})
.WithName("GetChat");

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