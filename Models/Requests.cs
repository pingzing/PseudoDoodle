namespace PseudoDoodle.Models;

public record CreateEventRequest(string Name, DateOnly[] Dates);
public record VoteRequest(string VoterName, DateOnly[] Votes);
