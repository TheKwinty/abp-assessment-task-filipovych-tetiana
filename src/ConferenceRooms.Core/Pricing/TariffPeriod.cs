namespace ConferenceRooms.Core.Pricing;

internal readonly record struct TariffPeriod(
    TimeOnly Start,
    TimeOnly End,
    decimal Multiplier);
