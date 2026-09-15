# Database Standards - nopCommerce

## Purpose
Database access, naming conventions, and migration standards for nopCommerce plugin and core work.

**Referenced by:** backend instruction file, nopcommerce-developer agent, nopcommerce-technical-designer agent

---

## 1. Data Access Pattern

### 1.1. Technology Stack
- Entity Framework Core / Linq2DB-backed data provider (`INopDataProvider`) - nopCommerce's own abstraction over the underlying provider.
- Supported database engines depend on target version (MS SQL Server is the primary supported engine; check target version for others).

### 1.2. Repository Pattern
- All entity access goes through `IRepository<T>` - never a raw `DbContext`/direct SQL unless the task explicitly requires a new data source outside nopCommerce's generic repository.
- New entities implement `BaseEntity` and live in the plugin's `Domain/` namespace (or core `Nop.Core.Domain` only for genuine core modifications).

## 2. Migrations
- Every schema change (new table, new column, new index) ships as a versioned migration class inheriting `Migration`/`AutoReversingMigration`, decorated with `[NopMigration("yyyy/mm/dd hh:mm:ss", "description")]` (or the version-appropriate migration attribute).
- Migrations live in the plugin's `Migrations/` folder, ordered so install-time migrations run before any code that depends on the new schema.
- Never hand-edit the database directly or ship a loose `.sql` script outside the migration system - it breaks nopCommerce's own upgrade/install tracking.
- Settings and permission records are typically NOT separate schema migrations - they're created via `ISettingService`/`IPermissionService` calls inside the plugin's `Install()` method, but still versioned alongside the plugin's install logic.

## 3. Naming Conventions
- Table/entity names follow nopCommerce's existing conventions for the domain area they extend (match casing and prefix style already used in `Nop.Core.Domain` for consistency, even in plugin-owned tables).
- Plugin-owned tables are prefixed distinctly enough to avoid collision with core or other plugins' tables (commonly reflecting the plugin's `SystemName`).

## 4. Multi-Store & Caching
- If an entity needs per-store variation, use nopCommerce's `StoreMapping` pattern rather than a custom store-ID column bolted onto the entity ad hoc.
- Respect existing `IStaticCacheManager` cache-key conventions; invalidate the correct keys on entity changes so stale data doesn't linger in cache.

## 5. Core Modification Exception
If a genuine core schema change is unavoidable (rare - most needs are met by a plugin-owned table with a foreign key to the core entity), it still follows the same migration discipline above, plus a Core Modification Notice explaining why a plugin-owned table with a relation wasn't sufficient.
