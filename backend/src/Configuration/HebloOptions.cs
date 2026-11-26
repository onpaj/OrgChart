using System.ComponentModel.DataAnnotations;

namespace OrgChart.API.Configuration;

/// <summary>
/// Where to ask Heblo for the signed-in user's permissions.
/// </summary>
public class HebloOptions
{
    public const string SectionName = "Heblo";

    /// <summary>Heblo base URL, e.g. https://heblo.anela.cz</summary>
    [Required(AllowEmptyStrings = false)]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Delegated Heblo scope requested on-behalf-of the user.</summary>
    [Required(AllowEmptyStrings = false)]
    public string Scope { get; set; } = string.Empty;

    /// <summary>How long a user's permission answer is reused.</summary>
    [Range(0, 60)]
    public int CacheMinutes { get; set; } = 5;
}
