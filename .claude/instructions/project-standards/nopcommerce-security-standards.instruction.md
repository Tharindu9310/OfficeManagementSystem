# Security Standards (MANDATORY) - nopCommerce

## Purpose
MANDATORY security requirements for nopCommerce plugin and core work. ALL agents MUST follow these standards.

**Referenced by:** All nopCommerce agents, backend and frontend instruction files

---

## 1. Authentication & Authorization

### 1.1. Admin Area
- Every admin controller/action that reads or mutates store data requires `[AuthorizeAdmin]`.
- Every admin action that mutates data additionally requires `[AutoValidateAntiforgeryToken]` (or the version-appropriate CSRF attribute).
- Gate feature access with `IPermissionService.Authorize(PermissionRecord)` - register a dedicated `PermissionRecord` on plugin install rather than reusing an unrelated existing permission.

### 1.2. Storefront
- Any action exposing customer-specific data requires the standard nopCommerce customer-authentication checks (`ICustomerService`/`IWorkContext.CurrentCustomer` validated, not just "logged in" assumed from a cookie).
- Public API endpoints (if any) validate the caller the same way core nopCommerce endpoints do - no bespoke auth scheme introduced without explicit instruction.

## 2. Input Validation
- Validate `ModelState` first in every POST/PUT admin and storefront action.
- Use nopCommerce's existing model validation attributes/FluentValidation setup (if the version uses it) rather than manual if-checks scattered through controllers.
- Never trust query-string or route values for authorization decisions (e.g., customer/order IDs) - always re-verify ownership/permission server-side.

## 3. Data Access
- MUST use `IRepository<T>` / EF Core parameterized queries - never string-concatenated SQL.
- Never bypass nopCommerce's store-mapping (`StoreMapping`) or ACL filtering when querying customer-facing data, even for "internal" admin reports, unless the report is explicitly cross-store.

## 4. Secrets
- NEVER commit API keys, connection strings, or credentials in plugin code or `plugin.json`.
- Payment/shipping/tax provider credentials go through the plugin's `ISettings`-derived settings class, stored via nopCommerce's existing settings persistence - not a custom `.env` or config file bundled with the plugin.
- NEVER log sensitive data (payment details, customer PII beyond what nopCommerce's own logger already handles) via `ILogger`/`ICustomerActivityService`.

## 5. Plugin-Specific
- Uninstall MUST remove settings, permissions, and locale resources - orphaned data left after uninstall is a security/hygiene issue, not just a cleanup nicety.
- Event consumers (`IConsumer<T>`) must not perform privileged actions without their own permission/ownership check - firing on a domain event doesn't imply the current context is authorized for what the consumer does.

## 6. Third-Party Integrations
- External API calls (payment gateways, shipping carriers) validate/sanitize responses before acting on them - don't trust a webhook payload without verifying its signature/source per the provider's documented mechanism.
