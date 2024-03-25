using System;
namespace LibraryApp.Core.Util;

public interface IJwtToken
{
    DateTime ValidTo { get; }
    string Value { get; }
}
