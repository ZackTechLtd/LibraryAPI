


namespace LibraryAPIApp.Util
{
    using System;

    public interface IJwtToken
    {
        DateTime ValidTo { get; }
        string Value { get; }
    }
}
