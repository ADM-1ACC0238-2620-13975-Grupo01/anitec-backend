using System.Net.Mime;
using Anitec.Platform.Subscriptions.Application.CommandServices;
using Anitec.Platform.Subscriptions.Application.QueryServices;
using Anitec.Platform.Subscriptions.Domain.Model.Commands;
using Anitec.Platform.Subscriptions.Domain.Model.Queries;
using Anitec.Platform.Subscriptions.Interfaces.Rest.Resources;
using Anitec.Platform.Subscriptions.Interfaces.Rest.Transform;
using Anitec.Platform.Iam.Domain.Model.Aggregates;
using Anitec.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Stripe;
using Stripe.Checkout;

namespace Anitec.Platform.Subscriptions.Interfaces.Rest;

[Authorize("Rancher", "Veterinarian")]
[ApiController]
[Route("api/v1/subscriptions")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Subscriptions endpoints")]
public class SubscriptionsController(
    ISubscriptionCommandService commandService,
    ISubscriptionQueryService queryService,
    ISubscriptionPlanQueryService planQueryService,
    IPaymentCommandService paymentCommandService,
    IPaymentQueryService paymentQueryService,
    IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await queryService.Handle(new GetAllSubscriptionsQuery(), cancellationToken);
        return Ok(result.Select(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await queryService.Handle(new GetSubscriptionByIdQuery(id), cancellationToken);
        if (result is null) return NotFound();
        return Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSubscriptionResource resource, CancellationToken cancellationToken)
    {
        var command = CreateSubscriptionCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await commandService.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id },
            SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(result.Value));
    }

    [HttpGet("users/{userId:int}/active")]
    public async Task<IActionResult> GetActiveByUser(int userId, CancellationToken cancellationToken)
    {
        var subscriptions = await queryService.Handle(new GetAllSubscriptionsQuery(), cancellationToken);
        var subscription = subscriptions
            .Where(item => item.UserId == userId)
            .Where(item => item.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.StartedAt)
            .FirstOrDefault();

        if (subscription is null) return NotFound();

        return Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscription));
    }

    [HttpGet("users/{userId:int}/payments")]
    public async Task<IActionResult> GetPaymentsByUser(int userId, CancellationToken cancellationToken)
    {
        var payments = await paymentQueryService.Handle(userId, cancellationToken);
        return Ok(payments.Select(PaymentResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPost("mock-checkout")]
    public async Task<IActionResult> MockCheckout(MockCheckoutResource resource, CancellationToken cancellationToken)
    {
        var subscriptionCommand = new CreateSubscriptionCommand(
            resource.UserId,
            resource.PlanId,
            $"mock_customer_{resource.UserId}",
            $"mock_subscription_{Guid.NewGuid():N}",
            "Active",
            DateOnly.FromDateTime(DateTime.UtcNow),
            null);

        var subscriptionResult = await commandService.Handle(subscriptionCommand, cancellationToken);
        if (subscriptionResult.IsFailure) return BadRequest(new { message = subscriptionResult.Message });

        var paymentCommand = new CreatePaymentCommand(
            resource.UserId,
            subscriptionResult.Value!.Id,
            resource.Amount,
            resource.Currency,
            "MockStripe",
            $"mock_payment_{Guid.NewGuid():N}",
            "Paid",
            DateTime.UtcNow);

        var paymentResult = await paymentCommandService.Handle(paymentCommand, cancellationToken);
        if (paymentResult.IsFailure) return BadRequest(new { message = paymentResult.Message });

        return Ok(new MockCheckoutResultResource(
            SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscriptionResult.Value),
            PaymentResourceFromEntityAssembler.ToResourceFromEntity(paymentResult.Value!)));
    }

    [HttpPost("stripe-checkout")]
    public async Task<IActionResult> CreateStripeCheckout(
        StripeCheckoutResource resource,
        CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized();

        var plan = await planQueryService.Handle(new GetSubscriptionPlanByIdQuery(resource.PlanId), cancellationToken);
        if (plan is null || !plan.IsActive) return NotFound(new { message = "Subscription plan not found." });

        var secretKey = configuration["StripeSettings:SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Contains("change_me", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Stripe test secret key is not configured." });

        var successUrl = configuration["StripeSettings:SuccessUrl"];
        var cancelUrl = configuration["StripeSettings:CancelUrl"];
        if (string.IsNullOrWhiteSpace(successUrl) || string.IsNullOrWhiteSpace(cancelUrl))
            return BadRequest(new { message = "Stripe success and cancel URLs are not configured." });

        StripeConfiguration.ApiKey = secretKey;

        var unitAmount = (long)Math.Round(plan.Price * 100, MidpointRounding.AwayFromZero);
        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            ClientReferenceId = user.Id.ToString(),
            Metadata = new Dictionary<string, string>
            {
                ["userId"] = user.Id.ToString(),
                ["planId"] = plan.Id.ToString()
            },
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "pen",
                        UnitAmount = unitAmount,
                        Recurring = new SessionLineItemPriceDataRecurringOptions
                        {
                            Interval = "month"
                        },
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = plan.Name,
                            Description = $"AniTec subscription plan for up to {plan.MaxAnimals} animals"
                        }
                    }
                }
            ]
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

        return Ok(new StripeCheckoutSessionResource(session.Id, session.Url));
    }

    [HttpGet("stripe-checkout/{sessionId}/confirm")]
    public async Task<IActionResult> ConfirmStripeCheckout(string sessionId, CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user is null) return Unauthorized();

        var existingPayment = await paymentQueryService.Handle(sessionId, cancellationToken);
        if (existingPayment is not null)
        {
            var existingSubscription = await queryService.Handle(
                new GetSubscriptionByIdQuery(existingPayment.SubscriptionId),
                cancellationToken);

            if (existingSubscription is null) return NotFound();

            return Ok(new StripeCheckoutResultResource(
                SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(existingSubscription),
                PaymentResourceFromEntityAssembler.ToResourceFromEntity(existingPayment)));
        }

        var secretKey = configuration["StripeSettings:SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Contains("change_me", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Stripe test secret key is not configured." });

        StripeConfiguration.ApiKey = secretKey;
        var service = new SessionService();
        var session = await service.GetAsync(sessionId, cancellationToken: cancellationToken);

        if (!session.Status.Equals("complete", StringComparison.OrdinalIgnoreCase) &&
            !session.PaymentStatus.Equals("paid", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Stripe checkout session is not completed." });

        if (!session.Metadata.TryGetValue("userId", out var userIdValue) ||
            !int.TryParse(userIdValue, out var stripeUserId) ||
            stripeUserId != user.Id)
            return Forbid();

        if (!session.Metadata.TryGetValue("planId", out var planIdValue) ||
            !int.TryParse(planIdValue, out var planId))
            return BadRequest(new { message = "Stripe checkout metadata is incomplete." });

        var plan = await planQueryService.Handle(new GetSubscriptionPlanByIdQuery(planId), cancellationToken);
        if (plan is null) return NotFound(new { message = "Subscription plan not found." });

        var subscriptions = await queryService.Handle(new GetAllSubscriptionsQuery(), cancellationToken);
        var activeSubscriptions = subscriptions
            .Where(item => item.UserId == user.Id)
            .Where(item => item.Status.Equals("Active", StringComparison.OrdinalIgnoreCase));

        foreach (var subscription in activeSubscriptions)
        {
            var updateCommand = new UpdateSubscriptionCommand(
                subscription.Id,
                subscription.UserId,
                subscription.PlanId,
                subscription.StripeCustomerId,
                subscription.StripeSubscriptionId,
                "Inactive",
                subscription.StartedAt,
                DateOnly.FromDateTime(DateTime.UtcNow));

            await commandService.Handle(updateCommand, cancellationToken);
        }

        var stripeCustomerId = session.CustomerId ?? string.Empty;
        var stripeSubscriptionId = session.SubscriptionId ?? session.Id;
        var subscriptionCommand = new CreateSubscriptionCommand(
            user.Id,
            plan.Id,
            stripeCustomerId,
            stripeSubscriptionId,
            "Active",
            DateOnly.FromDateTime(DateTime.UtcNow),
            null);

        var subscriptionResult = await commandService.Handle(subscriptionCommand, cancellationToken);
        if (subscriptionResult.IsFailure) return BadRequest(new { message = subscriptionResult.Message });

        var paymentCommand = new CreatePaymentCommand(
            user.Id,
            subscriptionResult.Value!.Id,
            plan.Price,
            "PEN",
            "StripeTest",
            session.Id,
            "Paid",
            DateTime.UtcNow);

        var paymentResult = await paymentCommandService.Handle(paymentCommand, cancellationToken);
        if (paymentResult.IsFailure) return BadRequest(new { message = paymentResult.Message });

        return Ok(new StripeCheckoutResultResource(
            SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(subscriptionResult.Value),
            PaymentResourceFromEntityAssembler.ToResourceFromEntity(paymentResult.Value!)));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateSubscriptionResource resource, CancellationToken cancellationToken)
    {
        var command = UpdateSubscriptionCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await commandService.Handle(command, cancellationToken);
        if (result.IsFailure) return NotFound();
        return Ok(SubscriptionResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new DeleteSubscriptionCommand(id), cancellationToken);
        if (result.IsFailure) return NotFound();
        return NoContent();
    }
}

