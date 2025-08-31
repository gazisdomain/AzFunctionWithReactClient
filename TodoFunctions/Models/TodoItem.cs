namespace TodoFunctions.Models;

public class TodoItem
{
    public string id { get; set; } = Guid.NewGuid().ToString();
    public string title { get; set; } = default!;
    public bool isDone { get; set; }
    public string partitionKey { get; set; } = default!;
}
