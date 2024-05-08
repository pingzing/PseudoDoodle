namespace PseudoDoodle.Models;

public record Event(Guid Id, string Name, DateOnly[] Dates, List<PossibleDate> Votes, List<string> Participants);

public record PossibleDate(DateOnly Date, string[] People);

public record EventResults(Guid Id, string Name, PossibleDate[] SuitableDates);
