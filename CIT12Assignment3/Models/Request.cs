using System.Text.Json;

public class Request
{
    public required string Method { get; set; } = "none"; // Chiara: added sentinels - will remove maybe
    public required string Path { get; set; } = "none";
    public long Date { get; set; } = 0; // unix timestamps are stored as longs | to convert datetime to long unix ts (64bit) use ToUnixTimeSeconds()
    public string? Body { get; set; } // objects will be saved parsed as a literal string first then -> deserialized appropriately
}

public class CreateOrUpdateBody
{
    public int id { get; set; }
    public required string name { get; set; }
}

// echo body is just string
