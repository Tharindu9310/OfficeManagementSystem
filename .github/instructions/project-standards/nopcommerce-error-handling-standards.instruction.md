# Error Handling Standards - nopCommerce

## Purpose
Standardized error handling patterns for nopCommerce plugin and core work. ALL agents MUST follow these standards.

**Referenced by:** All nopCommerce agents, backend and frontend instruction files

---

## 1. Backend Error Handling

### 1.1. Admin Controller Pattern (MANDATORY)
- Validate `ModelState` first in POST/PUT admin actions; return the view with validation summary on failure rather than throwing.
- Wrap external calls (payment gateway APIs, shipping carrier APIs, third-party services) in try/catch, log via `ILogger`, and surface a user-facing message through `INotificationService.ErrorNotification(...)` rather than letting the exception bubble to a generic error page.
- Never swallow an exception silently - at minimum log it via `ILogger`.

### 1.2. Storefront Error Handling
- Customer-facing failures (e.g., payment declined, out of stock at checkout) use nopCommerce's existing notification/error display conventions (`INotificationService`, model-level `Warning`/`Error` lists) - don't introduce a separate error UI pattern.
- Never expose internal exception details (stack traces, connection strings) in storefront-facing output.

### 1.3. Plugin Lifecycle Errors
- `Install()`/`Uninstall()` failures must not leave the plugin in a half-installed state where possible - if a step fails, log clearly enough that an admin can identify and manually clean up, and avoid throwing an unhandled exception that crashes the admin plugin list page.
- Event consumers (`IConsumer<T>`) must not let an unhandled exception propagate back into the core flow that raised the event (e.g., a consumer failure on `OrderPlacedEvent` must not break order placement itself) - catch, log, and fail gracefully within the consumer.

## 2. Frontend (Razor/Theme) Error Handling
- Views defensively check for null/empty model state where a service call could plausibly return nothing (e.g., a widget with no configured content) - render nothing or a sensible fallback, not an unhandled exception page.
- Client-side JS errors in a widget/component should not break the rest of the page - scope event handlers/try-catch appropriately.

## 3. Third-Party Integration Errors
- Payment/shipping/tax provider calls: handle both HTTP-level failures (timeout, 5xx) and provider-level business errors (declined, invalid address) distinctly - a timeout is not the same as a decline, and the customer-facing message should reflect that.
- Webhook handlers validate payload/signature before processing, and return the response code the provider expects even on internal failure (many providers retry on non-2xx) - log the failure separately from the response sent.

## 4. Logging
- Use nopCommerce's `ILogger` (which persists to the `Log` table) for anything an admin might need to investigate later - not just `Console.WriteLine`/`Debug.WriteLine`.
- Include enough context (customer ID, order ID, plugin system name) in log messages to make them actionable, without logging sensitive data (see security standards).
