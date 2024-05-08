using PseudoDoodle.Models;

namespace PseudoDoodle.Services;

internal interface IDataStore
{
    Task<Event[]> GetAll();
    Task<Event> Get(Guid id);
    Task<Guid> Create(string name, DateOnly[] dates);
    Task<Event> Vote(Guid id, string voterName, DateOnly[] votes);
    Task<EventResults> GetResults(Guid id);
}
