# The Booker - Multi-tenant SaaS Appointment Scheduling Platform

## Original Problem Statement
Build "The Booker" - A High-Performance Multi-tenant SaaS Appointment Scheduling Platform using:
- .NET 10 (Preview) / C# 13
- PostgreSQL 16+ via Entity Framework Core
- React (Vite) + TypeScript + Tailwind CSS + Shadcn/UI
- Clean Architecture with CQRS, DDD, Result Pattern

## User Choices
- **Database**: Local PostgreSQL
- **Authentication**: JWT-based custom auth (to be implemented)
- **Multi-tenancy**: Shared schema with TenantId discriminator
- **Deployment**: Docker/Kubernetes containerized

## Architecture
```
/app/backend/src/
├── TheBooker.Domain/           # Core business logic, entities, value objects
├── TheBooker.Application/      # CQRS handlers, services, interfaces
├── TheBooker.Infrastructure/   # EF Core, repositories, background services
└── TheBooker.Api/              # ASP.NET Core Minimal API endpoints
```

## Core Requirements
1. ✅ Multi-tenant support with TenantId discriminator
2. ✅ Service catalog per tenant
3. ✅ Provider management with N:N service assignments
4. ✅ Availability calculation engine
5. ✅ Soft-lock booking (10-min hold)
6. ✅ Background cleanup of expired holds
7. ⬜ JWT Authentication
8. ⬜ Email notifications

## What's Been Implemented (2026-02-01)

### Phase 1: Project Skeleton ✅
- Clean Architecture solution structure
- Shared Kernel (Entity, ValueObject, Result pattern)

### Phase 2: Domain Modeling ✅
- Tenant, TenantUser, Service, ServiceProvider, Appointment, ScheduleOverride
- AppointmentStatus smart enum (Pending/Confirmed/Cancelled/NoShow/Completed)
- BusinessHours value object with JSONB serialization

### Phase 3: Infrastructure ✅
- PostgreSQL with EF Core 9
- JSONB mapping for BusinessHours
- Strategic indexes for availability queries

### Phase 4: Availability Engine ✅
- Slot generation algorithm
- Buffer policy support
- Schedule override handling

### Phase 5-6: CQRS Implementation ✅
- GetAvailabilityQuery
- HoldSlotCommand (soft lock)
- ConfirmAppointmentCommand
- CancelAppointmentCommand

### Phase 7: Background Jobs ✅
- ExpiredAppointmentCleanupService (runs every minute)

### Phase 8: Frontend ✅
- Booking wizard (4-step flow)
- Admin dashboard
- TanStack Query for state management

## API Endpoints
- `GET /api/health` - Health check
- `GET /api/tenants` - List tenants
- `POST /api/tenants` - Create tenant
- `GET /api/tenants/{slug}` - Get tenant by slug
- `GET /api/tenants/{id}/services` - List services
- `POST /api/tenants/{id}/services` - Create service
- `GET /api/tenants/{id}/providers` - List providers
- `POST /api/tenants/{id}/providers` - Create provider
- `GET /api/availability/{tenant}/{provider}/{service}/{date}` - Get availability
- `POST /api/appointments/hold` - Hold slot
- `POST /api/appointments/{id}/confirm` - Confirm appointment
- `POST /api/appointments/{id}/cancel` - Cancel appointment

## Prioritized Backlog
### P0 (Critical)
- [ ] JWT Authentication with TenantUser
- [ ] Authorization middleware

### P1 (High)
- [ ] Email notifications (booking confirmation)
- [ ] Customer self-service portal
- [ ] Appointment rescheduling

### P2 (Medium)
- [ ] Schedule overrides CRUD
- [ ] Provider availability customization
- [ ] Reporting dashboard

### P3 (Nice to have)
- [ ] Real-time availability (SignalR)
- [ ] SMS notifications
- [ ] Payment integration

## Tech Stack Details
- Backend: .NET 9.0.310, ASP.NET Core Minimal APIs
- Database: PostgreSQL 16
- ORM: EF Core 9.0.1
- Frontend: React 19, TanStack Query, Tailwind CSS
- UI Components: Shadcn/UI (Radix primitives)
