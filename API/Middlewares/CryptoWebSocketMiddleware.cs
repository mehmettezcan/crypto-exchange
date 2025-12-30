using API.Websocket;
namespace API.Middlewares;

public class CryptoWebSocketMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, CryptoWsHandler handler)
    {
        if (context.Request.Path == "/ws/crypto")
        {
            await handler.HandleAsync(context);
            return;
        }

        await _next(context);
    }
}
