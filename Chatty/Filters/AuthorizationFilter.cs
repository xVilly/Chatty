

using Microsoft.AspNetCore.SignalR;

namespace Chatty.Filters
{
    public class AuthorizationFilter : Attribute, IHubFilter
    {
        public async ValueTask<object> InvokeMethodAsync(HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object>> next)
        {
            Console.WriteLine("Authorized user");
            return await next(invocationContext);
        }
    }
}
