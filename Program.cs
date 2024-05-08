using PseudoDoodle.Models;
using PseudoDoodle.Services;

var builder = WebApplication.CreateBuilder(args);

// -- Configure --

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Local services
builder.Services.AddSingleton<IDataStore, JsonFileDataStore>();

var app = builder.Build();

// -- Configure Services --

// Swagger UI and routing
app.UseSwagger();
app.UseSwaggerUI();

// Routing

app.MapGet(
        "/api/v1/event/list",
        async (IDataStore dataStore) =>
        {
            var events = await dataStore.GetAll();
            var response = new AllEventsResponse(events);
            return Results.Ok(response);
        }
    )
    .WithName("GetAllEvents")
    .WithOpenApi();

app.MapPost(
        "/api/v1/event",
        async (CreateEventRequest req, IDataStore dataStore) =>
        {
            var created = await dataStore.Create(req.Name, req.Dates);
            var response = new CreateEventResponse(created);
            return Results.Ok(response);
        }
    )
    .WithName("CreateEvent")
    .WithOpenApi();

app.MapGet(
        "/api/v1/event/{id}",
        async (Guid id, IDataStore dataStore) =>
        {
            var evt = await dataStore.Get(id);
            var response = new EventResponse(evt);
            return Results.Ok(response);
        }
    )
    .WithName("GetEvent")
    .WithOpenApi();

app.MapPost(
        "/api/v1/event/{id}/vote",
        async (Guid id, VoteRequest req, IDataStore dataStore) =>
        {
            var voteResult = await dataStore.Vote(id, req.VoterName, req.Votes);
            var response = new VoteResponse(voteResult);
            return Results.Ok(response);
        }
    )
    .WithName("Vote")
    .WithOpenApi();

app.MapGet(
        "/api/v1/event/{id}/results",
        async (Guid id, IDataStore dataStore) =>
        {
            var results = await dataStore.GetResults(id);
            var response = new ResultsResponse(results);
            return Results.Ok(response);
        }
    )
    .WithName("GetResults")
    .WithOpenApi();

// Run!
app.Run();
