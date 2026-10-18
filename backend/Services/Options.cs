namespace Pickleball.Api.Services;

public class AppException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public class JwtOptions
{
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public int AccessMinutes { get; set; } = 15;
    public int RefreshDays { get; set; } = 7;
}

public class BookingOptions
{
    public string TimeZone { get; set; } = "UTC";
    public int OpenHour { get; set; } = 7;
    public int CloseHour { get; set; } = 22;
    public int MaxHours { get; set; } = 3;
}
