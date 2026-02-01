using TheBooker.Domain.Common.Results;

namespace TheBooker.Domain;

/// <summary>
/// Centralized domain errors for consistent error handling.
/// </summary>
public static class DomainErrors
{
    public static class Tenant
    {
        public static readonly Error SlugAlreadyExists = Error.Conflict(
            "Tenant.SlugAlreadyExists", "A tenant with this slug already exists.");

        public static readonly Error NotFound = Error.NotFound(
            "Tenant", Guid.Empty);

        public static Error NotFoundById(Guid id) => Error.NotFound("Tenant", id);
    }

    public static class TenantUser
    {
        public static readonly Error EmailAlreadyExists = Error.Conflict(
            "TenantUser.EmailAlreadyExists", "A user with this email already exists in this tenant.");

        public static readonly Error NotFound = Error.NotFound(
            "TenantUser", Guid.Empty);

        public static readonly Error InvalidCredentials = Error.Validation(
            "TenantUser.InvalidCredentials", "Invalid email or password.");

        public static Error NotFoundById(Guid id) => Error.NotFound("TenantUser", id);
    }

    public static class Appointment
    {
        public static readonly Error NotFound = Error.NotFound(
            "Appointment", Guid.Empty);

        public static readonly Error SlotNotAvailable = Error.Conflict(
            "Appointment.SlotNotAvailable", "The requested time slot is no longer available.");

        public static readonly Error AlreadyConfirmed = Error.Validation(
            "Appointment.AlreadyConfirmed", "This appointment is already confirmed.");

        public static readonly Error CannotCancel = Error.Validation(
            "Appointment.CannotCancel", "This appointment cannot be cancelled in its current state.");

        public static readonly Error CannotConfirm = Error.Validation(
            "Appointment.CannotConfirm", "This appointment cannot be confirmed in its current state.");

        public static readonly Error LockExpired = Error.Validation(
            "Appointment.LockExpired", "The slot lock has expired. Please try again.");

        public static readonly Error InvalidSessionId = Error.Validation(
            "Appointment.InvalidSessionId", "Invalid session ID for this appointment.");

        public static readonly Error EndTimeBeforeStartTime = Error.Validation(
            "Appointment.EndTimeBeforeStartTime", "End time cannot be before start time.");

        public static Error NotFoundById(Guid id) => Error.NotFound("Appointment", id);
    }

    public static class Service
    {
        public static readonly Error NotFound = Error.NotFound(
            "Service", Guid.Empty);

        public static readonly Error InvalidDuration = Error.Validation(
            "Service.InvalidDuration", "Service duration must be between 5 and 480 minutes.");

        public static Error NotFoundById(Guid id) => Error.NotFound("Service", id);
    }

    public static class ServiceProvider
    {
        public static readonly Error NotFound = Error.NotFound(
            "ServiceProvider", Guid.Empty);

        public static Error NotFoundById(Guid id) => Error.NotFound("ServiceProvider", id);
    }

    public static class ScheduleOverride
    {
        public static readonly Error OverlappingOverride = Error.Conflict(
            "ScheduleOverride.Overlapping", "An override already exists for this date range.");

        public static readonly Error InvalidDateRange = Error.Validation(
            "ScheduleOverride.InvalidDateRange", "End date must be on or after start date.");
    }

    public static class BusinessHours
    {
        public static readonly Error OpenDayRequiresTimes = Error.Validation(
            "BusinessHours.OpenDayRequiresTimes", "Open days must have both open and close times.");

        public static readonly Error CloseTimeBeforeOpenTime = Error.Validation(
            "BusinessHours.CloseTimeBeforeOpenTime", "Close time must be after open time.");

        public static readonly Error AtLeastOneDayMustBeOpen = Error.Validation(
            "BusinessHours.AtLeastOneDayMustBeOpen", "At least one day must be open.");
    }

    public static class Email
    {
        public static readonly Error Empty = Error.Validation(
            "Email.Empty", "Email address is required.");

        public static readonly Error TooLong = Error.Validation(
            "Email.TooLong", $"Email address cannot exceed {ValueObjects.Email.MaxLength} characters.");

        public static readonly Error InvalidFormat = Error.Validation(
            "Email.InvalidFormat", "Email address format is invalid.");
    }

    public static class Slug
    {
        public static readonly Error Empty = Error.Validation(
            "Slug.Empty", "Slug is required.");

        public static readonly Error TooShort = Error.Validation(
            "Slug.TooShort", $"Slug must be at least {ValueObjects.Slug.MinLength} characters.");

        public static readonly Error TooLong = Error.Validation(
            "Slug.TooLong", $"Slug cannot exceed {ValueObjects.Slug.MaxLength} characters.");

        public static readonly Error InvalidFormat = Error.Validation(
            "Slug.InvalidFormat", "Slug can only contain lowercase letters, numbers, and hyphens.");
    }

    public static class TimeRange
    {
        public static readonly Error EndBeforeStart = Error.Validation(
            "TimeRange.EndBeforeStart", "End time must be after start time.");
    }

    public static class Money
    {
        public static readonly Error NegativeAmount = Error.Validation(
            "Money.NegativeAmount", "Amount cannot be negative.");

        public static readonly Error InvalidCurrency = Error.Validation(
            "Money.InvalidCurrency", "Currency must be a valid 3-letter ISO code.");
    }

    public static class CustomerInfo
    {
        public static readonly Error NameRequired = Error.Validation(
            "CustomerInfo.NameRequired", "Customer name is required.");

        public static readonly Error NameTooLong = Error.Validation(
            "CustomerInfo.NameTooLong", "Customer name cannot exceed 100 characters.");
    }
}
