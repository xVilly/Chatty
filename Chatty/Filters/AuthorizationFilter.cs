

using Microsoft.AspNetCore.SignalR;

namespace Chatty.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AuthorizeUser : Attribute
    {
        public int RequiredPermission;
    }
    public class AuthorizeUserFilter : IHubFilter
    {
        public async ValueTask<object?> InvokeMethodAsync(HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
        {
            var attribute = (AuthorizeUser?)Attribute.GetCustomAttribute(invocationContext.HubMethod,
                typeof(AuthorizeUser));
            Console.WriteLine(invocationContext.HubMethodArguments.Count);
            if (attribute != null) {
                Console.WriteLine($"Works! {attribute.RequiredPermission}");
                return "Custom error message!";
            }
            return await next(invocationContext);
        }
    }
}
