using PseudoDoodle.Models;
using System.Text;
using System.Text.Json;

namespace PseudoDoodle.Services;

public class JsonFileDataStore : IDataStore
{
    private readonly JsonSerializerOptions _serializerOpts = new(JsonSerializerDefaults.Web);
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "dataStore.json");
    private readonly ILogger<JsonFileDataStore> _logger;

    public JsonFileDataStore(ILogger<JsonFileDataStore> logger)
    {
        if (!File.Exists(_filePath))
        {
            using var fs = new FileStream(_filePath, FileMode.OpenOrCreate);
            string emptyArray = "[]";
            var textBytes = Encoding.UTF8.GetBytes(emptyArray).AsSpan();
            fs.Write(textBytes);
        }

        _logger = logger;
    }

    public async Task<Guid> Create(string name, DateOnly[] dates)
    {
        List<Event> events = (await ReadFileContents()).ToList();
        Guid newEventGuid = Guid.NewGuid();
        events.Add(new Event(newEventGuid, name, dates, [], []));
        await WriteContentsToFile(events);
        return newEventGuid;
    }

    public async Task<Event> Get(Guid id)
    {
        List<Event> events = (await ReadFileContents()).ToList();
        var evt = events.SingleOrDefault(x => x.Id == id);
        if (evt == null)
        {
            _logger.LogError("Unable to find event with ID {id}", id);
            throw new Exception("Event not found"); // TODO: Better error handling -> status code management
        }

        return evt;
    }

    public Task<Event[]> GetAll()
    {
        return ReadFileContents();
    }

    public async Task<EventResults> GetResults(Guid id)
    {
        List<Event> events = (await ReadFileContents()).ToList();
        var evt = events.SingleOrDefault(x => x.Id == id);
        if (evt == null)
        {
            _logger.LogError("Unable to find event with ID {id}", id);
            throw new Exception("Event not found"); // TODO: Better error handling -> status code management
        }

        PossibleDate[] suitableDates = evt.Votes
            .Where(vote => evt.Participants.All(participant => vote.People.Contains(participant)))
            .ToArray();

        return new EventResults(evt.Id, evt.Name, suitableDates);
    }

    public async Task<Event> Vote(Guid id, string voterName, DateOnly[] votes)
    {
        List<Event> events = (await ReadFileContents()).ToList();
        var evt = events.SingleOrDefault(x => x.Id == id);
        if (evt == null)
        {
            _logger.LogError("Unable to find event with ID {id}", id);
            throw new Exception("Event not found"); // TODO: Better error handling -> status code management
        }

        if (!evt.Participants.Contains(voterName))
        {
            evt.Participants.Add(voterName);
        }

        foreach (DateOnly date in votes)
        {
            PossibleDate? existingDate = evt.Votes.SingleOrDefault(x => x.Date == date);
            if (existingDate == null)
            {
                evt.Votes.Add(new PossibleDate(date, [voterName]));
            }
            else
            {
                evt.Votes.Remove(existingDate);
                var updatedVote = existingDate with { People = [.. existingDate.People, voterName] };
                evt.Votes.Add(updatedVote);
            }
        }

        await WriteContentsToFile(events);
        return evt;
    }

    private async Task<Event[]> ReadFileContents()
    {
        Event[]? deserialized = null;

        await _fileLock.WaitAsync();
        try
        {
            string json = File.ReadAllText(_filePath);
            deserialized = JsonSerializer.Deserialize<Event[]>(json, _serializerOpts);
        }
        finally
        {
            _fileLock.Release();
        }

        if (deserialized == null)
        {
            _logger.LogError("Got null when attempting to deserialize data JSON file.");
            throw new Exception("Unable to read from the data store.");
        }

        return deserialized;
    }

    private async Task WriteContentsToFile(IEnumerable<Event> tasks)
    {
        await _fileLock.WaitAsync();
        try
        {
            string json = JsonSerializer.Serialize(tasks, _serializerOpts);
            await File.WriteAllTextAsync(_filePath, json, Encoding.UTF8);
        }
        finally
        {
            _fileLock.Release();
        }
    }
}
