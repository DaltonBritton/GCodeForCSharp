
namespace GcodeParser;

/// <summary>
/// An Exception representing an error when reading or saving GCode.
/// </summary>
/// <param name="message">The message to be displayed when thrown</param>
public class InvalidGCode(string message) : Exception(message);

/// <summary>
/// An Exception representing an error when reading or saving GCode.
/// </summary>
/// <param name="message">The message to be displayed when thrown</param>
public class DuplicateArgumentException(string message) : InvalidGCode(message);