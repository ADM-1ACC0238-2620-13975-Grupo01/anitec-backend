using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;

namespace Anitec.Platform.Subscriptions.Interfaces.Rest;

/// <summary>
/// Pages Stripe returns to after a checkout that started in the mobile app.
/// Stripe only returns to http(s) addresses, so each page just opens the app through its own link scheme
/// (anitec://subscriptions/...). The app then confirms the payment with the confirm endpoint.
/// These pages are public on purpose: the browser tab that shows them has no session.
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("api/v1/subscriptions/mobile-return")]
public partial class SubscriptionReturnController : ControllerBase
{
    private const string AppLink = "anitec://subscriptions";

    /// <summary>Checkout session ids look like cs_test_a1B2c3; anything else is dropped before it reaches the page.</summary>
    [GeneratedRegex("^[A-Za-z0-9_]{1,200}$")]
    private static partial Regex SessionIdPattern();

    /// <summary>Opens the app at the success screen with the checkout session to confirm.</summary>
    [HttpGet("success")]
    public ContentResult Success([FromQuery(Name = "session_id")] string? sessionId)
    {
        var target = sessionId is not null && SessionIdPattern().IsMatch(sessionId)
            ? $"{AppLink}/success?session_id={sessionId}"
            : $"{AppLink}/success";
        return Page(target, "Payment received", "Return to AniTec to finish your subscription.");
    }

    /// <summary>Opens the app at the screen shown when the user leaves the checkout.</summary>
    [HttpGet("cancel")]
    public ContentResult Cancel() =>
        Page($"{AppLink}/cancel", "Payment cancelled", "Return to AniTec. You were not charged.");

    private ContentResult Page(string appUrl, string title, string message)
    {
        var url = WebUtility.HtmlEncode(appUrl);
        var html = $$"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>AniTec</title>
              <meta http-equiv="refresh" content="0;url={{url}}">
              <style>
                body { font-family: sans-serif; background: #F5F0E6; color: #1E2520; margin: 0;
                       display: flex; min-height: 100vh; align-items: center; justify-content: center; }
                main { background: #FFFAF2; border: 1px solid #E7DFD1; border-radius: 8px; padding: 24px;
                       max-width: 360px; margin: 16px; text-align: center; }
                a { display: inline-block; margin-top: 16px; padding: 12px 20px; border-radius: 8px;
                    background: #4F8F45; color: #fff; text-decoration: none; font-weight: bold; }
              </style>
            </head>
            <body>
              <main>
                <h1>{{WebUtility.HtmlEncode(title)}}</h1>
                <p>{{WebUtility.HtmlEncode(message)}}</p>
                <a href="{{url}}">Open AniTec</a>
              </main>
              <script>window.location.href = "{{url}}";</script>
            </body>
            </html>
            """;
        return Content(html, "text/html");
    }
}
