using System.Net;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes; // OpenAPI attributes
using Microsoft.OpenApi.Models;
using TodoFunctions.Models;

namespace TodoFunctions.Functions;

public class TodosApi
{
    private readonly Container _container;
    public TodosApi(Container container) => _container = container;

    // POST /api/todos
    [Function("CreateTodo")]
    [OpenApiOperation(operationId: "CreateTodo", tags: new[] { "Todos" })]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(TodoItem), Required = true, Description = "Todo payload")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json", bodyType: typeof(TodoItem), Description = "Created todo")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "text/plain", bodyType: typeof(string), Description = "Invalid request")]
    public async Task<HttpResponseData> CreateTodo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todos")] HttpRequestData req)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        var item = JsonSerializer.Deserialize<TodoItem>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (item is null || string.IsNullOrWhiteSpace(item.title))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Body must include { title: string }");
            return bad;
        }

        item.id ??= Guid.NewGuid().ToString();
        item.partitionKey ??= item.id;

        var created = await _container.CreateItemAsync(item, new PartitionKey(item.partitionKey));

        var res = req.CreateResponse(HttpStatusCode.Created);
        await res.WriteAsJsonAsync(created.Resource);
        return res;
    }

    // GET /api/todos
    [Function("ListTodos")]
    [OpenApiOperation(operationId: "ListTodos", tags: new[] { "Todos" })]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<TodoItem>), Description = "All todos")]
    public async Task<HttpResponseData> ListTodos(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos")] HttpRequestData req)
    {
        var iter = _container.GetItemQueryIterator<TodoItem>("SELECT * FROM c");
        var items = new List<TodoItem>();
        while (iter.HasMoreResults)
        {
            var page = await iter.ReadNextAsync();
            items.AddRange(page);
        }

        var res = req.CreateResponse(HttpStatusCode.OK);
        await res.WriteAsJsonAsync(items);
        return res;
    }

    // GET /api/todos/{id}
    [Function("GetTodo")]
    [OpenApiOperation(operationId: "GetTodo", tags: new[] { "Todos" })]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Todo id")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TodoItem), Description = "The todo")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "Not found")]
    public async Task<HttpResponseData> GetTodo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos/{id}")] HttpRequestData req,
        string id)
    {
        try
        {
            var r = await _container.ReadItemAsync<TodoItem>(id, new PartitionKey(id));
            var ok = req.CreateResponse(HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(r.Resource);
            return ok;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }
    }

    // POST/PATCH /api/todos/{id}/done
    [Function("MarkDone")]
    [OpenApiOperation(operationId: "MarkDone", tags: new[] { "Todos" })]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Todo id")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(TodoItem), Description = "Updated todo")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NotFound, Description = "Not found")]
    public async Task<HttpResponseData> MarkDone(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "patch", Route = "todos/{id}/done")] HttpRequestData req,
        string id)
    {
        try
        {
            var r = await _container.ReadItemAsync<TodoItem>(id, new PartitionKey(id));
            var item = r.Resource;
            item.isDone = true;

            var replaced = await _container.ReplaceItemAsync(item, item.id, new PartitionKey(item.partitionKey));
            var ok = req.CreateResponse(HttpStatusCode.OK);
            await ok.WriteAsJsonAsync(replaced.Resource);
            return ok;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }
    }

    [Function("DeleteTodo")]
    [OpenApiOperation(operationId: "DeleteTodo", tags: new[] { "Todos" })]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Todo id")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Deleted")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Not found")]
    public async Task<HttpResponseData> DeleteTodo(
    [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todos/{id}")] HttpRequestData req,
    string id)
    {
        try
        {
            await _container.DeleteItemAsync<TodoItem>(id, new PartitionKey(id));
            return req.CreateResponse(HttpStatusCode.NoContent);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }
    }

}
