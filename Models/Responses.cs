namespace PseudoDoodle.Models;

public record AllEventsResponse(EventResponse[] Events)
{
    public AllEventsResponse(Event[] events)
        : this(
            events
                .Select(x => new EventResponse(x.Id, x.Name, x.Dates, x.Votes.ToArray()))
                .ToArray()
        ) { }
}

public record CreateEventResponse(Guid Id);

public record EventResponse(Guid Id, string Name, DateOnly[] Dates, PossibleDate[] Votes)
{
    public EventResponse(Event evt)
        : this(evt.Id, evt.Name, evt.Dates, evt.Votes.ToArray()) { }
}

public record VoteResponse(Guid Id, string Name, DateOnly[] Dates, PossibleDate[] Votes)
{
    public VoteResponse(Event evt)
        : this(evt.Id, evt.Name, evt.Dates, evt.Votes.ToArray()) { }
}

public record ResultsResponse(Guid Id, string Name, PossibleDate[] SuitableDates)
{
    public ResultsResponse(EventResults results)
        : this(results.Id, results.Name, results.SuitableDates) { }
}
