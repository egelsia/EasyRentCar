using System;
using System.Security.Principal;
using System.Web;
using Moq;

public static class HttpContextMock
{
    public static HttpContextBase CreateMockHttpContext(bool isAuthenticated)
    {
        var context = new Mock<HttpContextBase>();
        var request = new Mock<HttpRequestBase>();
        var response = new Mock<HttpResponseBase>();
        var session = new Mock<HttpSessionStateBase>();
        var user = new Mock<IPrincipal>();
        var identity = new Mock<IIdentity>();

        identity.Setup(i => i.IsAuthenticated).Returns(isAuthenticated);
        user.Setup(u => u.Identity).Returns(identity.Object);
        context.Setup(c => c.Request).Returns(request.Object);
        context.Setup(c => c.Response).Returns(response.Object);
        context.Setup(c => c.Session).Returns(session.Object);
        context.Setup(c => c.User).Returns(user.Object);

        return context.Object;
    }
}
