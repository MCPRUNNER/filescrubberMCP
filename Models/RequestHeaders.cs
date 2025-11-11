namespace filescrubberMCP.Models;

/// <summary>
/// Model representing HTTP request headers collection
/// </summary>
public class RequestHeaders
{
private readonly Dictionary<string, string> _headers;

    /// <summary>
    /// Initializes a new instance of the RequestHeaders class
  /// </summary>
public RequestHeaders()
    {
     _headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Initializes a new instance of the RequestHeaders class with initial headers
    /// </summary>
 /// <param name="headers">Initial headers to add</param>
    public RequestHeaders(Dictionary<string, string> headers)
    {
      _headers = new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Adds or updates a header
    /// </summary>
    /// <param name="key">The header name</param>
    /// <param name="value">The header value</param>
    public void Add(string key, string value)
{
        if (string.IsNullOrWhiteSpace(key))
  throw new ArgumentException("Header key cannot be null or empty", nameof(key));

        _headers[key] = value ?? string.Empty;
    }

    /// <summary>
    /// Removes a header
  /// </summary>
    /// <param name="key">The header name to remove</param>
    /// <returns>True if the header was removed, false otherwise</returns>
    public bool Remove(string key)
    {
        return _headers.Remove(key);
    }

    /// <summary>
    /// Tries to get a header value
    /// </summary>
    /// <param name="key">The header name</param>
    /// <param name="value">The header value if found</param>
    /// <returns>True if the header exists, false otherwise</returns>
    public bool TryGetValue(string key, out string? value)
 {
return _headers.TryGetValue(key, out value);
    }

  /// <summary>
 /// Checks if a header exists
    /// </summary>
    /// <param name="key">The header name</param>
    /// <returns>True if the header exists, false otherwise</returns>
    public bool Contains(string key)
    {
     return _headers.ContainsKey(key);
 }

    /// <summary>
    /// Gets the number of headers
    /// </summary>
public int Count => _headers.Count;

  /// <summary>
    /// Gets all header keys
    /// </summary>
    public IEnumerable<string> Keys => _headers.Keys;

    /// <summary>
    /// Gets all header values
    /// </summary>
    public IEnumerable<string> Values => _headers.Values;

    /// <summary>
 /// Converts the headers to a dictionary
    /// </summary>
    /// <returns>Dictionary representation of headers</returns>
    public Dictionary<string, string> ToDictionary()
    {
  return new Dictionary<string, string>(_headers, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets or sets a header value by key
    /// </summary>
    /// <param name="key">The header name</param>
    /// <returns>The header value</returns>
    public string this[string key]
    {
 get => _headers[key];
        set => Add(key, value);
    }

    /// <summary>
    /// Creates a RequestHeaders instance from a dictionary
    /// </summary>
 /// <param name="headers">The dictionary to convert</param>
    /// <returns>RequestHeaders instance</returns>
    public static RequestHeaders FromDictionary(Dictionary<string, string>? headers)
    {
      return headers == null ? new RequestHeaders() : new RequestHeaders(headers);
    }

  /// <summary>
    /// Implicit conversion from Dictionary to RequestHeaders
  /// </summary>
    public static implicit operator RequestHeaders?(Dictionary<string, string>? headers)
    {
     return headers == null ? null : new RequestHeaders(headers);
    }

    /// <summary>
    /// Implicit conversion from RequestHeaders to Dictionary
    /// </summary>
    public static implicit operator Dictionary<string, string>?(RequestHeaders? headers)
    {
   return headers?.ToDictionary();
    }
}
