using System;

internal class AppSettingsFileNotFoundException : Exception
{
    public AppSettingsFileNotFoundException() : base("Appsettings file not found")
    {
    }

    public AppSettingsFileNotFoundException(string? message) : base(message)
    {
    }

    public AppSettingsFileNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}