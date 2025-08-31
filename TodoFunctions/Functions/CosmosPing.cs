using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text;

public class CosmosPing
{
    private readonly Container _container;
    public CosmosPing(Container container) => _container = container;

    [Function("CosmosPing")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cosmos/ping")] HttpRequestData req)
    {
        var res = req.CreateResponse();
        try
        {
            await _container.ReadContainerAsync(); // simple call
            res.StatusCode = HttpStatusCode.OK;
            await res.WriteStringAsync("Cosmos OK");
        }
        catch (Exception ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Cosmos error: {ex.GetType().Name} - {ex.Message}");
            var inner = ex.InnerException;
            while (inner != null)
            {
                sb.AppendLine($"Inner: {inner.GetType().Name} - {inner.Message}");
                inner = inner.InnerException;
            }
            sb.AppendLine(ex.StackTrace);

            res.StatusCode = HttpStatusCode.InternalServerError;
            await res.WriteStringAsync(sb.ToString());
        }
        return res;
    }
}
