using System;

internal class NoNetWorkRegisterException : Exception
{
    public NoNetWorkRegisterException() : base("NetWork Not Registered")
    {
    }

    public NoNetWorkRegisterException(string? message) : base(message)
    {
    }

    public NoNetWorkRegisterException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}